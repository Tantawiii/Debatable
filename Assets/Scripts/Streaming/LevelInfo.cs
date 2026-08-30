using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One per gameplay level scene (on a <c>_Streaming/_LevelInfo</c> object).
/// Describes the level to <see cref="LevelStreamer"/> and self-registers on enable so the
/// streamer never has to search across additively-loaded scenes.
/// </summary>
public class LevelInfo : MonoBehaviour
{
    [Header("Spawn / entry")]
    [Tooltip("Where the player starts this level and where respawn sends them. Rotation = spawn facing.")]
    [SerializeField] private Transform entryPoint;

    [Tooltip("Below this world Y the player is considered to have fallen off and is respawned.")]
    [SerializeField] private float fallThresholdY = -20f;

    [Header("Next level")]
    [Tooltip("Scene name of the next level, or empty if this is the last level.")]
    [SerializeField] private string nextLevelSceneName = "";

    [Header("Triggers (this scene)")]
    [SerializeField] private LevelPreloadTrigger preloadTrigger;
    [SerializeField] private LevelExitTrigger exitTrigger;
    [Tooltip("Placed just inside THIS scene; fires when the player has walked in from the previous level.")]
    [SerializeField] private SeamConfirmTrigger seamConfirm;

    [Header("Seal")]
    [Tooltip("The seal that belongs to THIS scene and hides the PREVIOUS level once the player has crossed in.")]
    [SerializeField] private LevelSeal seal;

    [Header("On load")]
    [Tooltip("Roots to SetActive(true) the instant this scene is registered (e.g. Room, Furniture).")]
    [SerializeField] private GameObject[] revealOnLoad;

    public Transform EntryPoint => entryPoint;
    public float FallThresholdY => fallThresholdY;
    public string NextLevelSceneName => nextLevelSceneName;
    public bool HasNext => !string.IsNullOrEmpty(nextLevelSceneName);
    public LevelSeal Seal => seal;
    public SeamConfirmTrigger SeamConfirm => seamConfirm;
    public Scene Scene => gameObject.scene;
    // Cached on Awake so it stays readable after this scene is unloaded: LevelStreamer asks a
    // just-destroyed level for its name while tearing it down (SealAndUnloadRoutine).
    public string SceneName => string.IsNullOrEmpty(_sceneName) ? gameObject.scene.name : _sceneName;
    private string _sceneName;

    public void Reveal()
    {
        if (revealOnLoad == null) return;
        foreach (var go in revealOnLoad)
            if (go != null) go.SetActive(true);
    }

    private void OnEnable()
    {
        if (LevelStreamer.Instance != null)
            LevelStreamer.Instance.RegisterLevel(this);
        else
            _pendingRegister = true;
    }

    private void OnDisable()
    {
        if (LevelStreamer.Instance != null)
            LevelStreamer.Instance.UnregisterLevel(this);
    }

    private bool _pendingRegister;

    private void Awake()
    {
        _sceneName = gameObject.scene.name;
#if UNITY_EDITOR
        // Let designers press Play from any level scene on its own: pull in the
        // Player scene (which owns the player rig + streamer) additively.
        if (Application.isPlaying && LevelStreamer.Instance == null
            && !SceneManager.GetSceneByName(SceneNames.Player).isLoaded)
        {
            _editorSoloBoot = true;
            SceneManager.LoadSceneAsync(SceneNames.Player, LoadSceneMode.Additive);
        }
#endif
    }

    private void Update()
    {
        if (_pendingRegister && LevelStreamer.Instance != null)
        {
            _pendingRegister = false;
            LevelStreamer.Instance.RegisterLevel(this);
        }

#if UNITY_EDITOR
        // If we booted straight into this level (Play pressed on the level scene) the streamer
        // exists now but has no current level — enter this one.
        if (_editorSoloBoot && LevelStreamer.Instance != null
            && LevelStreamer.Instance.Current == null
            && LevelStreamer.Instance.State == StreamerState.Idle)
        {
            _editorSoloBoot = false;
            LevelStreamer.Instance.StartFromAlreadyLoaded(this);
        }
#endif
    }

#if UNITY_EDITOR
    private bool _editorSoloBoot;
#endif

    /// <summary>Fallback used only if a scene was loaded without going through the streamer.</summary>
    public static LevelInfo For(Scene s)
    {
        foreach (var root in s.GetRootGameObjects())
        {
            var li = root.GetComponentInChildren<LevelInfo>(true);
            if (li != null) return li;
        }
        return null;
    }
}
