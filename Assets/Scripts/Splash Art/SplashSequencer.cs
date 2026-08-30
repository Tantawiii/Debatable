using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bridges the Bootstrap splash to the menu with a black fade instead of a hard cut. Lives on
/// [SplashStage] (always active). At ~half the Ted clip it begins an async ADDITIVE load of the
/// Player scene, parked (not activated), so Player + Level 1 + MainMenu stream in behind the
/// still-playing splash. At the splash's end it fades to black, activates Player
/// (GameBootstrap -> BootToMenu raises the menu under the Player-side ScreenFader), sets Player
/// active, and unloads the Bootstrap scene. Replaces the old SplashArtToMain hard LoadScene.
/// </summary>
public class SplashSequencer : MonoBehaviour
{
    [Tooltip("When to start the background load (Ted clip is 0..3.6s, so 1.8 = halfway).")]
    [SerializeField] private float preloadAtSeconds = 1.8f;
    [Tooltip("When the splash is done and the fade-to-menu begins.")]
    [SerializeField] private float revealAtSeconds = 4.2f;
    [SerializeField] private float fadeSeconds = 0.5f;
    [SerializeField] private CanvasGroup fadeGroup;

    private IEnumerator Start()
    {
#if UNITY_EDITOR
        if (SceneManager.sceneCount > 1) yield break;   // a level scene was opened alongside Bootstrap
#endif
        if (fadeGroup != null) fadeGroup.alpha = 0f;

        // --- half the Ted clip: begin the background load, parked ---
        yield return new WaitForSeconds(Mathf.Max(0f, preloadAtSeconds));

        var op = SceneManager.LoadSceneAsync(SceneNames.Player, LoadSceneMode.Additive);
        op.allowSceneActivation = false;
        while (op.progress < 0.9f) yield return null;   // loaded, not activated, not rendered

        // --- rest of the splash, then fade to black ---
        float wait = Mathf.Max(0f, revealAtSeconds - preloadAtSeconds);
        for (float t = 0f; t < wait; t += Time.deltaTime) yield return null;

        if (fadeGroup != null)
        {
            float d = Mathf.Max(0.01f, fadeSeconds);
            for (float f = 0f; f < d; f += Time.deltaTime)
            {
                fadeGroup.alpha = Mathf.Clamp01(f / d);
                yield return null;
            }
            fadeGroup.alpha = 1f;
        }

        // --- swap: activate Player, let its rig camera come up live, THEN drop Bootstrap ---
        // Bootstrap's camera is left enabled until the scene unloads, so there is never a frame
        // with zero cameras ("There are no cameras rendering"). Both cameras overlap for a couple
        // of frames, hidden behind the black Fade + Player's ScreenFader.
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;
        yield return null;   // one frame so the persistent rig camera actually renders

        var player = SceneManager.GetSceneByName(SceneNames.Player);
        if (player.IsValid() && player.isLoaded) SceneManager.SetActiveScene(player);

        // This GameObject lives in Bootstrap and goes away with it — fire and don't await.
        var bootstrap = gameObject.scene;
        if (bootstrap.IsValid() && bootstrap.isLoaded && SceneManager.sceneCount > 1)
            SceneManager.UnloadSceneAsync(bootstrap);
    }
}
