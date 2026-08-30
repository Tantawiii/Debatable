using UnityEngine;

/// <summary>
/// One spoken narrator line: a clip and/or a caption, plus a pause held after it.
/// Authored as an array on a <see cref="VoiceTrigger"/>; the clip may be empty while only the
/// subtitle exists (record the .wav later and drop it in).
/// </summary>
[System.Serializable]
public class NarrationLine
{
    [Tooltip("Optional id for play-once bookkeeping. Leave blank for lines that may repeat.")]
    public string id;

    [Tooltip("The voice clip. May be empty while only subtitles exist.")]
    public AudioClip clip;

    [TextArea(1, 4)]
    [Tooltip("Subtitle text shown while this line plays.")]
    public string caption;

    [Tooltip("Seconds of silence held after this line before the next one.")]
    public float gapAfter = 0.3f;

    [Tooltip("Caption lifetime when there is no clip to time against.")]
    public float captionOnlySeconds = 2f;
}
