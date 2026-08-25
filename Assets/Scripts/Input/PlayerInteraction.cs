using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private Camera playerCamera;

    [Header("Interaction")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private float interactRadius = 1f;

    private void OnEnable()
    {
        input.InteractPressed += HandleInteract;
    }

    private void OnDisable()
    {
        input.InteractPressed -= HandleInteract;
    }

    private void HandleInteract()
    {
        if (Physics.SphereCast(
            playerCamera.transform.position,
            interactRadius,
            playerCamera.transform.forward,
            out RaycastHit hit,
            interactRange,
            interactableMask))
        {
            if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.Interact();
            }
        }
    }
}