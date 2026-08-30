using System.Collections;
using UnityEngine;

/// <summary>
/// Code-driven fallback for the splash sequence (teddy skydives onto the pillow pile, title pops).
/// NOT used by default — the Timeline (`SplashArtTimeline.playable` on `[SplashStage]`) drives the
/// splash. If that graph misbehaves on import: add this component to `[SplashStage]`, disable the
/// `PlayableDirector`, wire the four references below, and it runs the same beats from curves you
/// can tune in the Inspector. It ends by activating `SplashArtControl`, exactly like the ControlTrack.
/// </summary>
public class SplashDirector : MonoBehaviour
{
    [Header("Scene refs (children of [SplashStage])")]
    [SerializeField] private Transform tedPivot;
    [SerializeField] private Transform pillowPile;
    [SerializeField] private Transform title3D;
    [SerializeField] private GameObject splashArtControl;   // enabling this triggers SplashArtToMain

    [Header("Timing")]
    [SerializeField] private float dropSeconds = 3.2f;
    [SerializeField] private float titleDelay = 3.1f;
    [SerializeField] private float titleSeconds = 0.7f;
    [SerializeField] private float endPadding = 0.4f;

    [Header("Curves (0..1 normalised)")]
    [SerializeField] private AnimationCurve tedHeight =
        new(new Keyframe(0f, 1f), new Keyframe(0.23f, 0f), new Keyframe(0.36f, 0.4f),
            new Keyframe(0.48f, 0f), new Keyframe(0.58f, 0.18f), new Keyframe(0.67f, 0f),
            new Keyframe(0.75f, 0.1f), new Keyframe(0.85f, 0f), new Keyframe(1f, 0f));
    [SerializeField] private AnimationCurve titlePop =
        new(new Keyframe(0f, 0f), new Keyframe(0.55f, 1.18f), new Keyframe(0.78f, 0.94f), new Keyframe(1f, 1f));

    [SerializeField] private float topY = 7f;
    [SerializeField] private float restY = 1.62f;

    private IEnumerator Start()
    {
        if (splashArtControl != null) splashArtControl.SetActive(false);
        if (title3D != null) title3D.localScale = Vector3.zero;

        float total = Mathf.Max(dropSeconds, titleDelay + titleSeconds) + endPadding;
        float t = 0f;
        while (t < total)
        {
            t += Time.unscaledDeltaTime;

            if (tedPivot != null)
            {
                float dn = Mathf.Clamp01(t / dropSeconds);
                float h = tedHeight.Evaluate(dn);
                var p = tedPivot.localPosition;
                p.y = Mathf.Lerp(restY, topY, h);
                tedPivot.localPosition = p;

                float squash = 1f - 0.3f * Mathf.Clamp01((0.12f - h) / 0.12f);   // pinch near contact
                tedPivot.localScale = new Vector3(2f - squash, squash, 2f - squash) * 0.5f + Vector3.one * 0.5f;
                if (pillowPile != null)
                    pillowPile.localScale = new Vector3(1f, Mathf.Lerp(1f, 0.78f, Mathf.Clamp01((0.1f - h) / 0.1f)), 1f);
            }

            if (title3D != null && t >= titleDelay)
            {
                float tn = Mathf.Clamp01((t - titleDelay) / titleSeconds);
                title3D.localScale = Vector3.one * titlePop.Evaluate(tn);
            }
            yield return null;
        }

        if (tedPivot != null) { var p = tedPivot.localPosition; p.y = restY; tedPivot.localPosition = p; tedPivot.localScale = Vector3.one; }
        if (pillowPile != null) pillowPile.localScale = Vector3.one;
        if (title3D != null) title3D.localScale = Vector3.one;
        if (splashArtControl != null) splashArtControl.SetActive(true);
    }
}
