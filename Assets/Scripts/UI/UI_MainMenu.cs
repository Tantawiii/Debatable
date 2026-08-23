using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_MainMenu : UI
{
    public void ToggleSettings() => Toggle(settingsPanel);
    public void ToggleControls() => Toggle(controlsPanel);
    public void ToggleCredits() => Toggle(creditsPanel);
    public void LoadGame() => LoadScene("Level 1 BlockOut");
}
