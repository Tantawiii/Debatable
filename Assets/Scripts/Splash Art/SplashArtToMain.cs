using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashArtToMain : MonoBehaviour
{
    private void OnEnable()
    {
#if UNITY_EDITOR
        // Editor solo-boot: a level scene pulled another scene in for the player rig — don't hijack it.
        if (SceneManager.sceneCount > 1)
            return;
#endif
        // Hand off to the persistent Player scene; it owns the rig + streamer and
        // brings up Level 1 + the MainMenu overlay itself (LevelStreamer.BootToMenu).
        SceneManager.LoadScene(SceneNames.Player);
    }
}
