using UnityEngine;

public static class GameProgress
{
    private const string HasReachedLevel1Key = "HasReachedLevel1";
    private const string TutorialCompleteKey = "TutorialComplete";

    /// <summary>Set the first time the player leaves the main menu into gameplay.</summary>
    public static bool HasReachedLevel1
    {
        get => PlayerPrefs.GetInt(HasReachedLevel1Key, 0) == 1;
        set { PlayerPrefs.SetInt(HasReachedLevel1Key, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    /// <summary>
    /// Set the first time the player finishes Level 1. Until then the move controls are scrambled
    /// (forward/backward reversed) and the pause menu is locked — the tutorial-narrator joke.
    /// </summary>
    public static bool TutorialComplete
    {
        get => PlayerPrefs.GetInt(TutorialCompleteKey, 0) == 1;
        set { PlayerPrefs.SetInt(TutorialCompleteKey, value ? 1 : 0); PlayerPrefs.Save(); }
    }
}
