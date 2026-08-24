using UnityEngine;

public class PlayerReload : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private Weapon weapon;

    private void OnEnable()
    {
        input.ReloadPressed += HandleReload;
    }

    private void OnDisable()
    {
        input.ReloadPressed -= HandleReload;
    }

    private void HandleReload()
    {
        weapon.Reload();
    }
}