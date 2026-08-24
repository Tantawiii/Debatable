using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private Weapon weapon;

    private void OnEnable()
    {
        input.ShootPressed += HandleShoot;
    }

    private void OnDisable()
    {
        input.ShootPressed -= HandleShoot;
    }

    private void HandleShoot()
    {
        weapon.Shoot();
    }
}