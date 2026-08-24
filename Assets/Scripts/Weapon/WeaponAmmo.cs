using UnityEngine;

public class WeaponAmmo : MonoBehaviour
{
    [Header("Ammo")]
    [SerializeField] private int magazineSize = 12;
    [SerializeField] private int startingReserveAmmo = 48;

    public int CurrentInMag { get; private set; }
    public int ReserveAmmo { get; private set; }

    public bool HasAmmoInMag => CurrentInMag > 0;

    public bool CanReload =>
        CurrentInMag < magazineSize && ReserveAmmo > 0;

    private void Awake()
    {
        CurrentInMag = magazineSize;
        ReserveAmmo = startingReserveAmmo;
    }

    public void ConsumeRound()
    {
        CurrentInMag = Mathf.Max(0, CurrentInMag - 1);
    }

    public void Reload()
    {
        int needed = magazineSize - CurrentInMag;
        int amountToLoad = Mathf.Min(needed, ReserveAmmo);

        CurrentInMag += amountToLoad;
        ReserveAmmo -= amountToLoad;
    }
}