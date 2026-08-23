using UnityEngine;

public static class GameProgress
{
    private const string HasReachedLevel1Key = "HasReachedLevel1";

    public static bool HasReachedLevel1
    {
        get => PlayerPrefs.GetInt(HasReachedLevel1Key, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(HasReachedLevel1Key, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
