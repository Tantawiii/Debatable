using System.Collections;
using UnityEngine;

/// <summary>
/// Background music with the attention span of a gnat. Plays a random track; after a random
/// interval the narrator complains and it swaps to a different one. Lives on [Systems] in the
/// Player scene (DontDestroyOnLoad). Routed through the Music volume bus (Master * Music), so the
/// music sliders move it and the narrator's own volume is untouched.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicDirector : MonoBehaviour
{
    public static MusicDirector Instance { get; private set; }

    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] tracks;
    [Tooltip("Seconds between track swaps (random in this range).")]
    [SerializeField] private Vector2 swapIntervalRange = new Vector2(30f, 75f);
    [SerializeField] private float startDelay = 4f;
    [SerializeField] private float quipToSwapGap = 1.5f;
    [Tooltip("Only start the music once the player is actually in a level.")]
    [SerializeField] private bool waitForGameplay = true;
    [Tooltip("How fast music fades in/out of the narration duck (units per second).")]
    [SerializeField] private float duckFadeSpeed = 3f;

    [TextArea]
    [SerializeField] private string[] swapQuips =
    {
        "Nah. This music is boring.",
        "Let's try something else.",
        "I don't like this song.",
        "New song. My choice.",
        "Ugh. Skip.",
        "This one's better. Probably.",
        "Wait, no. This one.",
        "Are you even listening to this?",
    };

    private int _current = -1;
    private float _duck = 1f;   // 1 = full, AudioBuses.MusicDuckFactor = ducked under narration

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (source == null) source = GetComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        AudioBuses.Changed += ApplyVolume;
    }

    private void OnDestroy()
    {
        AudioBuses.Changed -= ApplyVolume;
        if (Instance == this) Instance = null;
    }

    private void ApplyVolume()
    {
        if (source != null) source.volume = AudioBuses.MusicVolume * _duck;
    }

    private void Update()
    {
        bool speaking = NarrationDirector.Instance != null && NarrationDirector.Instance.IsSpeaking;
        float target = speaking ? AudioBuses.MusicDuckFactor : 1f;
        if (!Mathf.Approximately(_duck, target))
        {
            _duck = Mathf.MoveTowards(_duck, target, duckFadeSpeed * Time.unscaledDeltaTime);
            ApplyVolume();
        }
    }

    private IEnumerator Start()
    {
        if (tracks == null || tracks.Length == 0) yield break;

        if (waitForGameplay)
            while (LevelStreamer.Instance == null || LevelStreamer.Instance.State != StreamerState.Playing)
                yield return null;

        yield return new WaitForSeconds(Mathf.Max(0f, startDelay));
        NextTrack();

        while (tracks.Length > 1)
        {
            yield return new WaitForSeconds(Random.Range(swapIntervalRange.x, swapIntervalRange.y));

            if (NarrationDirector.Instance != null && swapQuips != null && swapQuips.Length > 0)
                NarrationDirector.Instance.SpeakOne(null, swapQuips[Random.Range(0, swapQuips.Length)]);

            yield return new WaitForSeconds(quipToSwapGap);
            NextTrack();
        }
    }

    private void NextTrack()
    {
        int n = _current;
        if (tracks.Length == 1) n = 0;
        else while (n == _current) n = Random.Range(0, tracks.Length);
        _current = n;

        source.clip = tracks[_current];
        ApplyVolume();
        source.Play();
    }
}
