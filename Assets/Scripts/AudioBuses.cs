using System;
using UnityEngine;

/// <summary>
/// Two volume buses (0..1), persisted in PlayerPrefs. <see cref="Master"/> scales everything the
/// game plays, including the narrator; <see cref="Music"/> scales only music. The narrator voice is
/// deliberately NOT touched by the Music bus. (A code bus rather than a Unity AudioMixer asset — the
/// routing is the same; swap in a real .mixer later if you want effects/ducking.)
/// </summary>
public static class AudioBuses
{
    private const string MasterKey = "vol_master";
    private const string MusicKey = "vol_music";

    public static float Master { get; private set; }
    public static float Music { get; private set; }

    /// <summary>Fires whenever either bus changes so live AudioSources can re-apply their volume.</summary>
    public static event Action Changed;

    static AudioBuses()
    {
        Master = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterKey, 1f));
        Music = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicKey, 1f));
    }

    public static void SetMaster(float v)
    {
        Master = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(MasterKey, Master); PlayerPrefs.Save();
        Changed?.Invoke();
    }

    public static void SetMusic(float v)
    {
        Music = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(MusicKey, Music); PlayerPrefs.Save();
        Changed?.Invoke();
    }

    /// <summary>Narrator / voice loudness — Master only.</summary>
    public static float VoiceVolume => Master;

    /// <summary>Music loudness — Master AND Music.</summary>
    public static float MusicVolume => Master * Music;
}
