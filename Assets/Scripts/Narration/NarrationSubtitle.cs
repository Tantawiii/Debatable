using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Bottom-of-screen caption line for the narrator. Overlay canvas, no camera needed. Sits below
/// the <see cref="ScreenFader"/> canvas (sortingOrder 32000) so a fade still covers it.
/// </summary>
public class NarrationSubtitle : MonoBehaviour
{
    public static NarrationSubtitle Instance { get; private set; }

    [SerializeField] private CanvasGroup group;
    [SerializeField] private TMP_Text label;
    [SerializeField] private float fade = 0.18f;

    private Coroutine _running;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (group == null) group = GetComponent<CanvasGroup>();
        if (group != null) { group.alpha = 0f; group.blocksRaycasts = false; group.interactable = false; }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Show text and hold it up until <see cref="HideNow"/> (the director drives lifetime).</summary>
    public void ShowHeld(string text)
    {
        if (label == null || group == null) return;
        if (_running != null) StopCoroutine(_running);
        label.text = text;
        _running = StartCoroutine(FadeTo(1f));
    }

    /// <summary>Fire-and-forget: show text, hold for <paramref name="hold"/> seconds, fade out.</summary>
    public void Show(string text, float hold)
    {
        if (label == null || group == null) return;
        if (_running != null) StopCoroutine(_running);
        label.text = text;
        _running = StartCoroutine(ShowRoutine(Mathf.Max(0f, hold)));
    }

    public void HideNow()
    {
        if (group == null) return;
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(FadeTo(0f));
    }

    private IEnumerator ShowRoutine(float hold)
    {
        yield return FadeTo(1f);
        yield return new WaitForSecondsRealtime(hold);
        yield return FadeTo(0f);
        _running = null;
    }

    private IEnumerator FadeTo(float target)
    {
        float from = group.alpha;
        float dur = Mathf.Max(0.0001f, fade);
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, target, t / dur);
            yield return null;
        }
        group.alpha = target;
    }
}
