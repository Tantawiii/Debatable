using UnityEngine;

public static class GameProgress
{
    private const string HasReachedLevel1Key = "HasReachedLevel1";
    private const string TutorialCompleteKey = "TutorialComplete";
    private const string FirstRunDoneKey = "FirstRunDone";

    /// <summary>Set the first time the player leaves the main menu into gameplay.</summary>
    public static bool HasReachedLevel1
    {
        get => PlayerPrefs.GetInt(HasReachedLevel1Key, 0) == 1;
        set { PlayerPrefs.SetInt(HasReachedLevel1Key, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    /// <summary>Set the first time the player finishes Level 1. Gates the pause-menu lock and the
    /// end-of-tutorial control un-scramble.</summary>
    public static bool TutorialComplete
    {
        get => PlayerPrefs.GetInt(TutorialCompleteKey, 0) == 1;
        set { PlayerPrefs.SetInt(TutorialCompleteKey, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    /// <summary>Set once the player has finished a whole playthrough (the Level 3 outro). Until then
    /// it's their "first run": controls start scrambled and every <c>firstRunOnly</c> narrator line
    /// plays. On replays it's true — no scramble, no first-run dialogue.</summary>
    public static bool FirstRunDone
    {
        get => PlayerPrefs.GetInt(FirstRunDoneKey, 0) == 1;
        set { PlayerPrefs.SetInt(FirstRunDoneKey, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    /// <summary>Convenience: the player has not yet completed a full playthrough.</summary>
    public static bool IsFirstRun => !FirstRunDone;
}
