using UnityEngine;

/// <summary>
/// One-shot trigger spanning the full corridor cross-section at a level's end (unavoidable).
/// When the persistent player enters, it commits the hand-off to the next level.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class LevelExitTrigger : MonoBehaviour
{
    [SerializeField] private LevelInfo owner;

    private bool _fired;

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        owner = GetComponentInParent<LevelInfo>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_fired || !PersistentPlayer.IsPlayerCollider(other)) return;
        _fired = true;
        GetComponent<Collider>().enabled = false;
        if (LevelStreamer.Instance != null && owner != null)
            LevelStreamer.Instance.CommitHandoff(owner);
    }
}
