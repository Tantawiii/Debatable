using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Escape-to-pause. Lives on [Systems] in the Player scene (DontDestroyOnLoad). Freezes the game
/// (Time.timeScale = 0), shows the pause-menu Canvas, and frees the cursor. Always available once
/// the player is in a level — first run or replay — but the settings' control rebinds stay locked
/// until Level 1 ends (see PlayerInputHandler.RebindLocked).
/// </summary>
public class PauseController : MonoBehaviour
{
    public static PauseController Instance { get; private set; }

    [SerializeField] private GameObject pauseRoot;   // the pause-menu Canvas (starts inactive)

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (pauseRoot != null) pauseRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;

        if (IsPaused) { Resume(); return; }

        if (LevelStreamer.Instance == null
            || LevelStreamer.Instance.State != StreamerState.Playing) return;

        Pause();
    }

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;
        Time.timeScale = 0f;
        if (pauseRoot != null) pauseRoot.SetActive(true);
        if (PersistentPlayer.Instance != null) PersistentPlayer.Instance.SetControlEnabled(false);
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = 1f;
        if (pauseRoot != null) pauseRoot.SetActive(false);
        if (PersistentPlayer.Instance != null) PersistentPlayer.Instance.SetControlEnabled(true);
    }
}
