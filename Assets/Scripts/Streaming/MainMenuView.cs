using System.Collections;
using UnityEngine;

/// <summary>
/// Sits on the MainMenu Canvas. Gives <see cref="LevelStreamer"/> a handle on the menu's
/// <see cref="CanvasGroup"/> after the menu is additively loaded, so it can fade the whole menu
/// out on "Start" and snap it back on for a return-to-menu.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class MainMenuView : MonoBehaviour
{
    public static MainMenuView Instance { get; private set; }

    [SerializeField] private CanvasGroup group;

    private Coroutine _running;

    private void Awake()
    {
        if (group == null) group = GetComponent<CanvasGroup>();
    }

    private void OnEnable()  => Instance = this;

    private void OnDisable()
    {
        if (Instance == this) Instance = null;
        if (_running != null) { StopCoroutine(_running); _running = null; }
    }

    /// <summary>Show (alpha 1, interactive) or hard-hide the menu instantly.</summary>
    public void SetShown(bool shown)
    {
        if (_running != null) { StopCoroutine(_running); _running = null; }
        if (group == null) return;
        group.alpha = shown ? 1f : 0f;
        group.interactable = shown;
        group.blocksRaycasts = shown;
    }

    /// <summary>Fade the menu from whatever alpha it is now down to 0 and stop it catching input.</summary>
    public Coroutine FadeOut(float duration)
    {
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(FadeRoutine(Mathf.Max(0.0001f, duration)));
        return _running;
    }

    private IEnumerator FadeRoutine(float duration)
    {
        if (group == null) yield break;
        group.interactable = false;
        group.blocksRaycasts = false;

        float from = group.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, 0f, t / duration);
            yield return null;
        }
        group.alpha = 0f;
        _running = null;
    }
}
