using UnityEngine;

/// <summary>
/// Rotates the player body (yaw) and camera target (pitch) based on mouse look input.
/// </summary>
public class PlayerLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private Transform cameraTarget;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private float pitch;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = input.LookInput.x * mouseSensitivity;
        float mouseY = input.LookInput.y * mouseSensitivity;

        // Yaw: rotate the whole player body left/right
        transform.Rotate(Vector3.up * mouseX);

        // Pitch: rotate only the camera target up/down, clamped
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        cameraTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}