using UnityEngine;

/// <summary>
/// One-shot box that fires a narrator line (or run of lines) when the persistent player walks
/// through it. Mirrors the streaming triggers: BoxCollider trigger + <see cref="PersistentPlayer.IsPlayerCollider"/>
/// check + self-disable. Place under a scene's <c>_Narration</c> root.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class VoiceTrigger : MonoBehaviour
{
    [SerializeField] private NarrationLine[] lines;
    [Tooltip("Fire once then disable the collider. Off = re-fires on every entry.")]
    [SerializeField] private bool once = true;
    [Tooltip("Only fire while the tutorial is still unfinished (GameProgress.TutorialComplete == false).")]
    [SerializeField] private bool firstRunOnly = false;
    [Tooltip("Higher priority barges in over a lower-priority line already playing.")]
    [SerializeField] private int priority = 0;

    private bool _fired;

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((_fired && once) || !PersistentPlayer.IsPlayerCollider(other)) return;
        if (firstRunOnly && GameProgress.TutorialComplete) return;
        if (once)
        {
            _fired = true;
            GetComponent<Collider>().enabled = false;
        }
        if (NarrationDirector.Instance != null)
            NarrationDirector.Instance.Speak(lines, priority);
    }
}
