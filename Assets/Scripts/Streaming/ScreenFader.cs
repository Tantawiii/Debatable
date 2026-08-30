using System.Collections;
using UnityEngine;

/// <summary>
/// Full-screen black overlay used to cover the odd frame during a hard hand-off.
/// Drive it via an overlay Canvas + CanvasGroup so it needs no camera.
/// </summary>
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [SerializeField] private CanvasGroup group;

    private Coroutine _running;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (group == null) group = GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.alpha = 1f;                 // covered until the first level is ready
            group.blocksRaycasts = false;
        }
    }

    public void SetOpaque(bool opaque)
    {
        Stop();
        if (group != null) group.alpha = opaque ? 1f : 0f;
    }

    public Coroutine FadeIn(float duration) => Play(1f, 0f, duration);   // opaque -> clear
    public Coroutine FadeOut(float duration) => Play(0f, 1f, duration);  // clear -> opaque

    private Coroutine Play(float from, float to, float duration)
    {
        Stop();
        _running = StartCoroutine(Fade(from, to, Mathf.Max(0.0001f, duration)));
        return _running;
    }

    private void Stop()
    {
        if (_running != null) { StopCoroutine(_running); _running = null; }
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (group == null) yield break;
        float t = 0f;
        group.alpha = from;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
        _running = null;
    }
}
