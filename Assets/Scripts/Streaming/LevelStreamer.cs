using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum StreamerState
{
    Idle, Booting, AtMenu, Revealing, LoadingFirst,
    Playing, Preloading, Committing, AwaitingSeamCross, Unloading, ReturningToMenu, Error
}

/// <summary>
/// Session-long singleton (on [Systems] in the persistent Player scene, DontDestroyOnLoad).
/// At boot it brings Level 1 up hidden behind the MainMenu overlay; "Start" fades the menu out to
/// reveal it. Then it drives seamless additive level hand-offs: a preload trigger starts an async
/// load, an exit trigger activates the next level behind the physically-aligned seam, and a
/// seam-confirm trigger fires the seal + unloads the previous level.
/// </summary>
public class LevelStreamer : MonoBehaviour
{
    public static LevelStreamer Instance { get; private set; }

    public static event Action<LevelInfo> LevelBecameCurrent;

    public StreamerState State { get; private set; } = StreamerState.Idle;
    public LevelInfo Current { get; private set; }

    [SerializeField] private float seamSafetyTimeout = 20f;
    [SerializeField] private float menuFadeDuration = 0.8f;

    private readonly Dictionary<string, LevelInfo> _loaded = new();
    private AsyncOperation _pendingOp;
    private string _pendingScene;
    private LevelInfo _pendingFrom;
    private LevelInfo _menuLevel;          // the level the MainMenu overlay is currently hiding
    private PlayerRespawn _respawn;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ---- registration -------------------------------------------------------

    public void RegisterLevel(LevelInfo info)
    {
        if (info == null) return;
        _loaded[info.SceneName] = info;
    }

    public void UnregisterLevel(LevelInfo info)
    {
        if (info == null) return;
        if (_loaded.TryGetValue(info.SceneName, out var cur) && cur == info)
            _loaded.Remove(info.SceneName);
    }

    // ---- public entry points ---------------------------------------------------

    /// <summary>Cold boot: bring Level 1 up hidden, then the MainMenu overlay on top. Called by GameBootstrap.</summary>
    public void BootToMenu()
    {
        if (State != StreamerState.Idle) return;
        StartCoroutine(BootRoutine());
    }

    /// <summary>"Start" pressed: fade the menu out and hand control to the already-loaded Level 1.</summary>
    public void RevealFirstLevel()
    {
        if (State != StreamerState.AtMenu) return;
        StartCoroutine(RevealRoutine());
    }

    /// <summary>Editor convenience: enter a level scene that is ALREADY loaded (Play pressed on it directly).</summary>
    public void StartFromAlreadyLoaded(LevelInfo info)
    {
        if (info == null) return;
        if (State != StreamerState.Idle && State != StreamerState.Error) return;
        RegisterLevel(info);
        StartCoroutine(EnterLoadedRoutine(info));
    }

    private IEnumerator EnterLoadedRoutine(LevelInfo info)
    {
        State = StreamerState.LoadingFirst;
        yield return null;
        DisableStrayCamerasAndListeners(info.Scene);
        SceneManager.SetActiveScene(info.Scene);
        info.Reveal();
        PlacePlayerAt(info.EntryPoint);
        if (PersistentCameraRig.Instance != null)
        {
            PersistentCameraRig.Instance.SetLive(true);
            PersistentCameraRig.Instance.SnapAfterWarp(
                PersistentPlayer.Instance != null ? PersistentPlayer.Instance.LastWarpDelta : Vector3.zero);
        }
        SetSpawn(info);
        if (PersistentPlayer.Instance != null)
        {
            PersistentPlayer.Instance.SetVisible(true);
            PersistentPlayer.Instance.SetControlEnabled(true);
        }
        if (ScreenFader.Instance != null) ScreenFader.Instance.SetOpaque(false);
        Current = info;
        State = StreamerState.Playing;
        LevelBecameCurrent?.Invoke(Current);
    }

    public void BeginPreload(LevelInfo from)
    {
        if (State != StreamerState.Playing) return;
        if (from != Current || !Current.HasNext) return;
        StartCoroutine(PreloadRoutine(Current.NextLevelSceneName));
    }

    public void CommitHandoff(LevelInfo from)
    {
        if (from != Current) return;
        if (State != StreamerState.Playing && State != StreamerState.Preloading) return;
        if (!Current.HasNext) return;

        // Left Level 1 for the first time — the tutorial is done: stop scrambling the move keys
        // and unlock the pause menu from here on.
        if (from.SceneName == SceneNames.Level1 && !GameProgress.TutorialComplete)
        {
            GameProgress.TutorialComplete = true;
            if (PersistentPlayer.Instance != null && PersistentPlayer.Instance.Input != null)
                PersistentPlayer.Instance.Input.UnscrambleControls();
        }

        StartCoroutine(CommitRoutine());
    }

    public void OnSeamCrossed(LevelInfo enteredLevel)
    {
        if (State != StreamerState.AwaitingSeamCross) return;
        if (_pendingFrom == null) return;
        StartCoroutine(SealAndUnloadRoutine());
    }

    public void ReturnToMenu()
    {
        if (State == StreamerState.ReturningToMenu || State == StreamerState.Booting
            || State == StreamerState.AtMenu) return;
        StopAllCoroutines();
        StartCoroutine(ReturnToMenuRoutine());
    }

    // ---- routines -----------------------------------------------------------

    private IEnumerator BootRoutine()
    {
        State = StreamerState.Booting;
        yield return BringUpMenuOverLevel1();
    }

    private IEnumerator RevealRoutine()
    {
        State = StreamerState.Revealing;

        // Fade the menu UI away — Level 1 is already rendering behind it, so no black screen.
        if (MainMenuView.Instance != null)
            yield return MainMenuView.Instance.FadeOut(menuFadeDuration);

        var menuScene = SceneManager.GetSceneByName(SceneNames.MainMenu);
        if (menuScene.isLoaded)
        {
            var op = SceneManager.UnloadSceneAsync(menuScene);
            while (op != null && !op.isDone) yield return null;
            yield return Resources.UnloadUnusedAssets();
        }

        var level1 = _menuLevel != null ? _menuLevel : ResolveLevel(SceneNames.Level1);
        if (level1 == null) { State = StreamerState.Error; yield break; }

        if (PersistentPlayer.Instance != null)
        {
            PersistentPlayer.Instance.SetVisible(true);
            PersistentPlayer.Instance.SetControlEnabled(true);
        }
        SetSpawn(level1);

        Current = level1;
        _menuLevel = null;
        State = StreamerState.Playing;
        LevelBecameCurrent?.Invoke(Current);
    }

    /// <summary>Shared by cold boot and return-to-menu: Level 1 loaded + hidden, menu overlay on top, park at AtMenu.</summary>
    private IEnumerator BringUpMenuOverLevel1()
    {
        if (!Application.CanStreamedLevelBeLoaded(SceneNames.Level1))
        {
            Debug.LogError($"[LevelStreamer] '{SceneNames.Level1}' is not in Build Settings.");
            State = StreamerState.Error;
            yield break;
        }

        if (ScreenFader.Instance != null) ScreenFader.Instance.SetOpaque(true);

        if (!SceneManager.GetSceneByName(SceneNames.Level1).isLoaded)
            yield return LoadAdditive(SceneNames.Level1, activateImmediately: true);

        var level1 = ResolveLevel(SceneNames.Level1);
        if (level1 == null) { State = StreamerState.Error; yield break; }

        DisableStrayCamerasAndListeners(level1.Scene);
        SceneManager.SetActiveScene(level1.Scene);
        level1.Reveal();

        PlacePlayerAt(level1.EntryPoint);
        if (PersistentCameraRig.Instance != null)
        {
            PersistentCameraRig.Instance.SetLive(true);
            PersistentCameraRig.Instance.SnapAfterWarp(
                PersistentPlayer.Instance != null ? PersistentPlayer.Instance.LastWarpDelta : Vector3.zero);
        }
        SetSpawn(level1);

        // Player is frozen + hidden behind the opaque menu until "Start".
        if (PersistentPlayer.Instance != null)
        {
            PersistentPlayer.Instance.SetControlEnabled(false);   // also unlocks the cursor for the menu
            PersistentPlayer.Instance.SetVisible(false);
        }

        if (!SceneManager.GetSceneByName(SceneNames.MainMenu).isLoaded
            && Application.CanStreamedLevelBeLoaded(SceneNames.MainMenu))
        {
            yield return LoadAdditive(SceneNames.MainMenu, activateImmediately: true);
            DisableStrayCamerasAndListeners(SceneManager.GetSceneByName(SceneNames.MainMenu));
        }
        if (MainMenuView.Instance != null) MainMenuView.Instance.SetShown(true);

        _menuLevel = level1;
        // Fade up from black into the menu (the splash / return-to-menu held it opaque).
        if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeIn(0.5f);
        State = StreamerState.AtMenu;

        // ACT 0 opener — clip wired later; subtitle carries it for now.
        if (NarrationDirector.Instance != null)
            NarrationDirector.Instance.Speak(new[]
            {
                new NarrationLine { id = "act0_hello", caption = "Hello.", captionOnlySeconds = 1.6f, gapAfter = 0.8f },
                new NarrationLine { id = "act0_loading", caption = "Oh. You're still loading. Take your time.", captionOnlySeconds = 3f },
            });
    }

    private IEnumerator PreloadRoutine(string sceneName)
    {
        if (_pendingOp != null || _loaded.ContainsKey(sceneName)) yield break;
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[LevelStreamer] Next scene '{sceneName}' not in Build Settings.");
            yield break;
        }

        State = StreamerState.Preloading;
        _pendingScene = sceneName;
        _pendingFrom = Current;
        _pendingOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        _pendingOp.allowSceneActivation = false;

        while (_pendingOp.progress < 0.9f) yield return null;
        // parked: loaded but not activated, not rendered.
    }

    private IEnumerator CommitRoutine()
    {
        State = StreamerState.Committing;
        _pendingFrom = Current;
        string nextName = Current.NextLevelSceneName;

        if (_pendingOp == null)
        {
            // player skipped the preload trigger — cover the hitch and load now
            if (ScreenFader.Instance != null) yield return ScreenFader.Instance.FadeOut(0.15f);
            _pendingScene = nextName;
            _pendingOp = SceneManager.LoadSceneAsync(nextName, LoadSceneMode.Additive);
            _pendingOp.allowSceneActivation = false;
            while (_pendingOp.progress < 0.9f) yield return null;
        }

        _pendingOp.allowSceneActivation = true;
        while (!_pendingOp.isDone) yield return null;
        _pendingOp = null;

        var next = ResolveLevel(nextName);
        if (next == null) { State = StreamerState.Error; yield break; }

        DisableStrayCamerasAndListeners(next.Scene);
        next.Reveal();
        SceneManager.SetActiveScene(next.Scene);
        if (ScreenFader.Instance != null) ScreenFader.Instance.SetOpaque(false);

        // stop the previous level's spawners before anything can Instantiate into the new scene
        _nextForHandoff = next;
        LevelBecameCurrent?.Invoke(next);   // spawners compare against their home scene and stop

        State = StreamerState.AwaitingSeamCross;
        _seamTimer = 0f;
    }

    private LevelInfo _nextForHandoff;
    private float _seamTimer;

    private void Update()
    {
        if (State == StreamerState.AwaitingSeamCross)
        {
            _seamTimer += Time.deltaTime;
            if (_seamTimer >= seamSafetyTimeout && _nextForHandoff != null)
                StartCoroutine(SealAndUnloadRoutine());
        }
    }

    private IEnumerator SealAndUnloadRoutine()
    {
        if (State != StreamerState.AwaitingSeamCross) yield break;
        State = StreamerState.Unloading;

        var next = _nextForHandoff;
        var prev = _pendingFrom;

        if (next != null && next.Seal != null) next.Seal.Activate();
        if (next != null) SetSpawn(next);

        if (prev != null && _loaded.ContainsKey(prev.SceneName))
        {
            var op = SceneManager.UnloadSceneAsync(prev.SceneName);
            while (op != null && !op.isDone) yield return null;
            _loaded.Remove(prev.SceneName);
        }
        yield return Resources.UnloadUnusedAssets();

        Current = next;
        _pendingFrom = null;
        _nextForHandoff = null;
        State = StreamerState.Playing;
        LevelBecameCurrent?.Invoke(Current);
    }

    private IEnumerator ReturnToMenuRoutine()
    {
        State = StreamerState.ReturningToMenu;

        if (ScreenFader.Instance != null) ScreenFader.Instance.SetOpaque(true);

        if (PersistentPlayer.Instance != null)
        {
            PersistentPlayer.Instance.SetControlEnabled(false);
            PersistentPlayer.Instance.SetVisible(false);
        }
        // Camera stays live the whole session now — the fader (opaque above) covers the reload.

        // Flush any parked preload.
        if (_pendingOp != null)
        {
            _pendingOp.allowSceneActivation = true;
            while (!_pendingOp.isDone) yield return null;
            _pendingOp = null;
        }
        if (!string.IsNullOrEmpty(_pendingScene) && _pendingScene != SceneNames.Level1
            && SceneManager.GetSceneByName(_pendingScene).isLoaded)
        {
            var pop = SceneManager.UnloadSceneAsync(_pendingScene);
            while (pop != null && !pop.isDone) yield return null;
            _loaded.Remove(_pendingScene);
        }
        _pendingScene = null;

        // Unload every loaded level except Level 1 (the menu will sit back over it).
        var toUnload = new List<string>();
        foreach (var name in _loaded.Keys)
            if (name != SceneNames.Level1) toUnload.Add(name);
        foreach (var name in toUnload)
        {
            if (SceneManager.GetSceneByName(name).isLoaded)
            {
                var op = SceneManager.UnloadSceneAsync(name);
                while (op != null && !op.isDone) yield return null;
            }
            _loaded.Remove(name);
        }

        Current = null;
        _pendingFrom = null;
        _nextForHandoff = null;
        yield return Resources.UnloadUnusedAssets();

        yield return BringUpMenuOverLevel1();
    }

    // ---- helpers ----------------------------------------------------------------

    private IEnumerator LoadAdditive(string sceneName, bool activateImmediately)
    {
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        op.allowSceneActivation = activateImmediately;
        if (!activateImmediately) while (op.progress < 0.9f) yield return null;
        while (!op.isDone) yield return null;
    }

    private LevelInfo ResolveLevel(string sceneName)
    {
        if (_loaded.TryGetValue(sceneName, out var li) && li != null) return li;
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            li = LevelInfo.For(scene);
            if (li != null) { _loaded[sceneName] = li; return li; }
        }
        Debug.LogError($"[LevelStreamer] No LevelInfo found in '{sceneName}'.");
        return null;
    }

    private void PlacePlayerAt(Transform entry)
    {
        if (PersistentPlayer.Instance == null || entry == null) return;
        PersistentPlayer.Instance.TeleportTo(entry.position, entry.rotation);
    }

    private void SetSpawn(LevelInfo info)
    {
        if (info == null || info.EntryPoint == null) return;
        if (_respawn == null && PersistentPlayer.Instance != null)
            _respawn = PersistentPlayer.Instance.GetComponentInChildren<PlayerRespawn>(true);
        if (_respawn != null)
        {
            _respawn.SetSpawnPoint(info.EntryPoint);
            _respawn.SetFallThreshold(info.FallThresholdY);
        }
    }

    private static void DisableStrayCamerasAndListeners(Scene scene)
    {
        if (!scene.IsValid()) return;
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var cam in root.GetComponentsInChildren<Camera>(true))
            {
                if (!cam.enabled) continue;
                Debug.LogWarning($"[LevelStreamer] Disabling stray Camera '{cam.name}' in {scene.name}.");
                cam.enabled = false;
            }
            foreach (var al in root.GetComponentsInChildren<AudioListener>(true))
            {
                if (!al.enabled) continue;
                Debug.LogWarning($"[LevelStreamer] Disabling stray AudioListener '{al.name}' in {scene.name}.");
                al.enabled = false;
            }
        }
    }
}
