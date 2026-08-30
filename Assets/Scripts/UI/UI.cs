using UnityEngine;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    [SerializeField] protected GameObject settingsPanel;
    [SerializeField] protected GameObject controlsPanel;
    [SerializeField] protected GameObject creditsPanel;
    public void Toggle(GameObject go) => go.SetActive(!go.activeSelf);
    public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);

    public void QuitGame()
    {
#if UNITY_WEBGL
        // A browser tab can't be closed from script, so instead of a dead button the narrator
        // plays the quit out as a bit and then admits it can't actually go through with it.
        var narrator = NarrationDirector.Instance;
        if (narrator == null) return;
        narrator.Speak(new[]
        {
            new NarrationLine { caption = "Quitting? Fine. Fine. Let me just shut it all down.", captionOnlySeconds = 3.2f, gapAfter = 0.35f },
            new NarrationLine { caption = "Wiping the levels. Rolling the credits. Killing the lights.", captionOnlySeconds = 3.4f, gapAfter = 0.35f },
            new NarrationLine { caption = "It's been real. Farewell.", captionOnlySeconds = 2.4f, gapAfter = 0.6f },
            new NarrationLine { caption = "Ah, this is a web version. Never mind, then.", captionOnlySeconds = 3.4f },
        }, priority: 10);
#else
        Application.Quit();
#endif
    }
}
