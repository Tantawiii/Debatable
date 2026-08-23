using UnityEngine;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    [SerializeField] protected GameObject settingsPanel;
    [SerializeField] protected GameObject controlsPanel;
    [SerializeField] protected GameObject creditsPanel;
    public void Toggle(GameObject go) => go.SetActive(!go.activeSelf);
    public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);
    public void QuitGame() => Application.Quit();
}
