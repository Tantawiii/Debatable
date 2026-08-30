using UnityEngine;

/// <summary>
/// One-shot trigger placed just inside a level, past the seam with the previous level.
/// Firing means "the player has fully walked into the new level" — the streamer then
/// activates this level's seal (hiding the previous level) and unloads it.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class SeamConfirmTrigger : MonoBehaviour
{
    [Tooltip("The LevelInfo of THIS scene (the one being entered).")]
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
            LevelStreamer.Instance.OnSeamCrossed(owner);
    }
}
