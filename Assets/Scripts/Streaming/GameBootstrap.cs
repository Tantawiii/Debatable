using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lives on [Systems] in the persistent Player scene. As soon as that scene is up it tells the
/// <see cref="LevelStreamer"/> to bring in Level 1 (hidden) and the MainMenu overlay on top.
/// Runs exactly once; a duplicate [Systems] from a scene reload is destroyed by the singleton
/// guards before this ever gets to Start.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    private static bool _booted;

    private IEnumerator Start()
    {
        if (_booted) yield break;

        // Wait one frame so LevelStreamer.Awake / the rig singletons have registered.
        yield return null;

        if (LevelStreamer.Instance == null)
        {
            Debug.LogError("[GameBootstrap] No LevelStreamer in the Player scene.");
            yield break;
        }
        if (LevelStreamer.Instance.State != StreamerState.Idle) yield break;

#if UNITY_EDITOR
        // If a designer pressed Play straight on a gameplay level scene, that scene's LevelInfo
        // pulled us in for the rig — let it drive its own solo-boot, don't raise the menu.
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (!s.isLoaded) continue;
            if (s.name != SceneNames.Player && s.name != SceneNames.MainMenu
                && s.name != SceneNames.Level1 && s.name != SceneNames.Bootstrap)
                yield break;
        }
#endif

        _booted = true;
        LevelStreamer.Instance.BootToMenu();
    }
}
