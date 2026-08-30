using UnityEngine;

/// <summary>
/// One-shot trigger placed a few metres before a level's exit. When the persistent player
/// enters, it tells the streamer to begin async-loading the next level (not yet activated).
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class LevelPreloadTrigger : MonoBehaviour
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
            LevelStreamer.Instance.BeginPreload(owner);
    }
}
