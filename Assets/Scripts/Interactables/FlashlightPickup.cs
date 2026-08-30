using UnityEngine;

/// <summary>
/// A flashlight lying in the world. While the persistent player is inside this trigger box,
/// pressing the Interact action turns on the player's own FlashLight object and removes this pickup.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class FlashlightPickup : MonoBehaviour
{
    private bool _inRange;
    private PlayerInputHandler _input;

    private void Reset() => GetComponent<BoxCollider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (_inRange || !PersistentPlayer.IsPlayerCollider(other)) return;
        _input = PersistentPlayer.Instance != null ? PersistentPlayer.Instance.Input : null;
        if (_input == null) return;
        _inRange = true;
        _input.InteractPressed += Collect;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!PersistentPlayer.IsPlayerCollider(other)) return;
        Unsubscribe();
    }

    private void OnDisable() => Unsubscribe();

    private void Collect()
    {
        Unsubscribe();

        var pp = PersistentPlayer.Instance;
        GameObject fl = pp != null ? pp.Flashlight : null;
        if (fl == null && pp != null && pp.CameraTarget != null)
        {
            var t = pp.CameraTarget.Find("FlashLight");
            if (t != null) fl = t.gameObject;
        }
        if (fl != null) fl.SetActive(true);

        Destroy(gameObject);
    }

    private void Unsubscribe()
    {
        if (_input != null) _input.InteractPressed -= Collect;
        _input = null;
        _inRange = false;
    }
}
