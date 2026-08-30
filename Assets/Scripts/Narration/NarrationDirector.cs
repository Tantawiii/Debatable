using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The narrator. Plays queued <see cref="NarrationLine"/>s one at a time through a single 2D
/// AudioSource, with captions. Lives on [Systems] in Player.unity (DontDestroyOnLoad) so it
/// survives level hand-offs and renders through the one persistent AudioListener. A higher-priority
/// request barges in and replaces whatever is playing.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class NarrationDirector : MonoBehaviour
{
    public static NarrationDirector Instance { get; private set; }

    [SerializeField] private AudioSource source;
    [SerializeField] private NarrationSubtitle subtitle;
    [SerializeField] private bool showSubtitles = true;
    [Tooltip("Pause inserted between two lines, on top of each line's own gapAfter.")]
    [SerializeField] private float defaultGap = 0.15f;

    private readonly Queue<NarrationLine> _queue = new();
    private readonly HashSet<string> _spoken = new();
    private Coroutine _pump;
    private int _currentPriority = int.MinValue;

    public bool IsSpeaking { get; private set; }
    public bool ShowSubtitles { get => showSubtitles; set => showSubtitles = value; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (source == null) source = GetComponent<AudioSource>();
        if (source != null) { source.playOnAwake = false; source.loop = false; source.spatialBlend = 0f; }
        AudioBuses.Changed += ApplyVolume;
        ApplyVolume();
    }

    private void OnDestroy()
    {
        AudioBuses.Changed -= ApplyVolume;
        if (Instance == this) Instance = null;
    }

    private void ApplyVolume()
    {
        if (source != null) source.volume = AudioBuses.VoiceVolume;   // Master only — the Music bus never touches the narrator
    }

    /// <summary>
    /// Queue a run of lines. If <paramref name="priority"/> beats what's currently playing it
    /// interrupts and replaces it. Lines whose non-empty id was already spoken are skipped.
    /// </summary>
    public void Speak(NarrationLine[] lines, int priority = 0)
    {
        if (lines == null || lines.Length == 0) return;

        if (IsSpeaking && priority > _currentPriority) StopNow();
        _currentPriority = IsSpeaking ? Mathf.Max(_currentPriority, priority) : priority;

        foreach (var line in lines)
        {
            if (line == null) continue;
            if (!string.IsNullOrEmpty(line.id) && _spoken.Contains(line.id)) continue;
            _queue.Enqueue(line);
        }

        if (_pump == null && _queue.Count > 0)
        {
            if (subtitle == null) subtitle = NarrationSubtitle.Instance;
            _pump = StartCoroutine(Pump());
        }
    }

    public void SpeakOne(AudioClip clip, string caption, int priority = 0)
    {
        Speak(new[] { new NarrationLine { clip = clip, caption = caption } }, priority);
    }

    /// <summary>Silence the narrator and drop anything queued.</summary>
    public void StopNow()
    {
        if (_pump != null) { StopCoroutine(_pump); _pump = null; }
        _queue.Clear();
        if (source != null) source.Stop();
        if (subtitle != null) subtitle.HideNow();
        IsSpeaking = false;
        _currentPriority = int.MinValue;
    }

    private IEnumerator Pump()
    {
        IsSpeaking = true;
        while (_queue.Count > 0)
        {
            var line = _queue.Dequeue();
            if (!string.IsNullOrEmpty(line.id)) _spoken.Add(line.id);

            if (showSubtitles && subtitle != null && !string.IsNullOrEmpty(line.caption))
                subtitle.ShowHeld(line.caption);

            float wait;
            if (line.clip != null && source != null)
            {
                source.Stop();
                source.clip = line.clip;
                ApplyVolume();
                source.Play();
                wait = line.clip.length;
            }
            else
            {
                wait = Mathf.Max(0.5f, line.captionOnlySeconds);
            }

            float t = 0f;
            while (t < wait) { t += Time.unscaledDeltaTime; yield return null; }

            if (subtitle != null) subtitle.HideNow();

            float gap = defaultGap + Mathf.Max(0f, line.gapAfter);
            t = 0f;
            while (t < gap) { t += Time.unscaledDeltaTime; yield return null; }
        }
        IsSpeaking = false;
        _currentPriority = int.MinValue;
        _pump = null;
    }
}
