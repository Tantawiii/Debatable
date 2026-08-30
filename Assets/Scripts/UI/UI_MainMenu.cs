using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_MainMenu : UI
{
    public void ToggleSettings() => Toggle(settingsPanel);
    public void ToggleControls() => Toggle(controlsPanel);
    public void ToggleCredits() => Toggle(creditsPanel);

    public void LoadGame()
    {
        GameProgress.HasReachedLevel1 = true;

        // Level 1 is already loaded underneath this menu — just fade the menu out and reveal it.
        if (LevelStreamer.Instance != null)
            LevelStreamer.Instance.RevealFirstLevel();
        else
            LoadScene(SceneNames.Bootstrap);   // launched straight into the menu with no streamer: go through the front door
    }

    public void OnStartPressed()     // Start_BTN
    {
        if (GameProgress.HasReachedLevel1) LoadGame();
        else QuitGame();
    }

    public void OnQuitPressed()      // Quit_BTN
    {
        if (GameProgress.HasReachedLevel1) QuitGame();
        else ToggleCredits();
    }

    public void OnOptionsPressed()   // Options_BTN
    {
        if (GameProgress.HasReachedLevel1) ToggleSettings();
        else LoadGame();
    }

    public void OnCreditsPressed()   // Credits_BTN
    {
        if (GameProgress.HasReachedLevel1) ToggleCredits();
        else ToggleSettings();
    }

    public void OnControlsPressed() => ToggleControls();   // Controls_BTN — no mayhem variant today

    [ContextMenu("Reset Has Reached Level 1")]
    private void ResetHasReachedLevel1() => GameProgress.HasReachedLevel1 = false;
}
