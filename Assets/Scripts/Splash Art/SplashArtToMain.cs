using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashArtToMain : MonoBehaviour
{
    private void OnEnable() 
    {
        SceneManager.LoadScene("MainMenu");
    }
}
