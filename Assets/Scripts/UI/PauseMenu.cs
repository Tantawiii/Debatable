using UnityEngine;

/// <summary>
/// The in-game pause menu — a trimmed copy of the main menu (Resume / Options / Quit, no Credits).
/// Keeps <see cref="UI_MainMenu"/>'s public method names so the copied Button OnClick bindings work
/// unchanged; only the "Start" slot behaves differently (it resumes).
/// </summary>
public class PauseMenu : UI
{
    public void ToggleSettings() => Toggle(settingsPanel);
    public void ToggleControls() => Toggle(controlsPanel);
    public void ToggleCredits() { if (creditsPanel != null) Toggle(creditsPanel); }

    public void OnStartPressed()                                   // the relabelled "Resume" button
    {
        if (PauseController.Instance != null) PauseController.Instance.Resume();
    }

    public void OnOptionsPressed() => ToggleSettings();
    public void OnCreditsPressed() => ToggleControls();            // no credits in the pause menu
    public void OnControlsPressed() => ToggleControls();
    public void OnQuitPressed() => QuitGame();
}
