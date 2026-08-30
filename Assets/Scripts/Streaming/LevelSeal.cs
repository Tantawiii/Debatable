using System.Collections;
using UnityEngine;

/// <summary>
/// A per-scene "seal" that visually cuts off the previous level once the player has crossed in,
/// so the previous scene can be unloaded. Either flips some objects on/off, or releases a
/// stack of kinematic pillows to fall as a curtain, or both.
/// </summary>
public class LevelSeal : MonoBehaviour
{
    public enum SealKind { EnableObjects, DropObjects }

    [SerializeField] private SealKind kind = SealKind.EnableObjects;

    [Header("Objects")]
    [SerializeField] private GameObject[] objectsToEnable;
    [SerializeField] private GameObject[] objectsToDisable;

    [Header("Drop (SealKind.DropObjects)")]
    [Tooltip("Kinematic rigidbodies parked above the corridor; released to fall on Activate().")]
    [SerializeField] private Rigidbody[] pillows;
    [SerializeField] private float releaseInterval = 0.03f;
    [SerializeField] private Vector3 releaseTorque = new Vector3(0f, 0f, 30f);

    public bool IsSealed { get; private set; }

    public void Activate()
    {
        if (IsSealed) return;
        IsSealed = true;

        if (objectsToEnable != null)
            foreach (var go in objectsToEnable)
                if (go != null) go.SetActive(true);

        if (objectsToDisable != null)
            foreach (var go in objectsToDisable)
                if (go != null) go.SetActive(false);

        if (kind == SealKind.DropObjects && pillows != null && pillows.Length > 0)
            StartCoroutine(ReleasePillows());
    }

    private IEnumerator ReleasePillows()
    {
        var wait = new WaitForSeconds(releaseInterval);
        foreach (var rb in pillows)
        {
            if (rb == null) continue;
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddRelativeTorque(Vector3.Scale(Random.insideUnitSphere, releaseTorque),
                                 ForceMode.VelocityChange);
            yield return wait;
        }
    }
}
