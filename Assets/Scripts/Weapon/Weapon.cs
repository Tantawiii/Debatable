using System.Collections;
using UnityEngine;

[RequireComponent(typeof(WeaponAmmo))]
public class Weapon : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Shooting")]
    [SerializeField] private float range = 100f;
    [SerializeField] private float damage = 10f;
    //[SerializeField] private LayerMask hittableMask;

    [Header("Debug")]
    [SerializeField] private float debugSphereSize = 0.1f;

    [Header("Reload")]
    [SerializeField] private float reloadDuration = 1.5f;

    private int playerLayer;
    private WeaponAmmo ammo;
    private bool isReloading;

    private Vector3 lastImpactPoint;
    private bool hasImpactPoint;

    private void Awake()
    {
        ammo = GetComponent<WeaponAmmo>();
        playerLayer = LayerMask.NameToLayer("Player");
    }

    public void Shoot()
    {
        if (isReloading)
        {
            return;
        }

        if (!ammo.HasAmmoInMag)
        {
            Debug.Log("Out of ammo!");
            return;
        }

        ammo.ConsumeRound();
        Debug.Log($"Shot fired! Ammo: {ammo.CurrentInMag}/{ammo.ReserveAmmo}");

        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;

        // Ignore the Player layer
        int layerMask = ~(1 << playerLayer);

        // Draw the ray in the Scene view
        Debug.DrawRay(origin, direction * range, Color.red, 5f);

        // Raycast stops at the FIRST collider it hits
        if (Physics.Raycast(origin, direction, out RaycastHit hit, range, layerMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log($"Raycast hit: {hit.collider.name}");
            Debug.Log($"Impact point: {hit.point}");

            lastImpactPoint = hit.point;
            hasImpactPoint = true;

            // Only damage objects that implement IDamageable
            if (hit.collider.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(damage);
            }
        }
        else
        {
            Debug.Log("Raycast did not hit anything.");
            hasImpactPoint = false;
        }
    }

    public void Reload()
    {
        if (isReloading)
        {
            return;
        }

        if (!ammo.CanReload)
        {
            Debug.Log("Cannot reload.");
            return;
        }

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        yield return new WaitForSeconds(reloadDuration);

        ammo.Reload();
        Debug.Log($"Reload complete! Ammo: {ammo.CurrentInMag}/{ammo.ReserveAmmo}");

        isReloading = false;
    }

    private void OnDrawGizmos()
    {
        if (!hasImpactPoint)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(lastImpactPoint, debugSphereSize);
    }
}