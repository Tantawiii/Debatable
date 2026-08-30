using UnityEngine;

/// <summary>
/// Put on a trigger collider (e.g. the Lava surface, layer "Lava").
/// Anything that carries a <see cref="PlayerRespawn"/> in its hierarchy is sent back
/// to its spawn point on contact.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LavaKill : MonoBehaviour
{
    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerRespawn>() is { } respawn)
        {
            respawn.Respawn();
        }
    }
}
