using UnityEngine;

/// <summary>
/// Lives on the [PlayerRig] root in the persistent Player scene, kept alive with DontDestroyOnLoad.
/// Owns control-enable and CharacterController-safe teleport for the whole session.
/// </summary>
public class PersistentPlayer : MonoBehaviour
{
    public static PersistentPlayer Instance { get; private set; }

    [Header("Wiring (children of this rig)")]
    [SerializeField] private Transform player;              // the actual Player transform
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private PlayerLook look;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private Renderer[] renderers;
    [Tooltip("The player's own FlashLight child object (starts inactive; turned on by a pickup).")]
    [SerializeField] private GameObject flashlight;

    public Transform Player => player;
    public Transform CameraTarget => cameraTarget;
    public PlayerInputHandler Input => input;
    public GameObject Flashlight => flashlight;
    public Vector3 LastWarpDelta { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetControlEnabled(false);
        SetVisible(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void SetControlEnabled(bool on)
    {
        if (input != null) input.enabled = on;
        if (look != null) look.enabled = on;
        if (movement != null) movement.enabled = on;
        if (controller != null) controller.enabled = on;
        Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !on;
    }

    public void SetVisible(bool on)
    {
        if (renderers == null) return;
        foreach (var r in renderers)
            if (r != null) r.enabled = on;
    }

    /// <summary>Teleport the rig, CharacterController-safe, killing any carried motion.</summary>
    public void TeleportTo(Vector3 position, Quaternion rotation)
    {
        Vector3 oldTargetPos = cameraTarget != null ? cameraTarget.position : position;

        bool ctlWasEnabled = controller != null && controller.enabled;
        bool moveWasEnabled = movement != null && movement.enabled;

        if (movement != null) movement.enabled = false;
        if (controller != null) controller.enabled = false;

        player.SetPositionAndRotation(position, rotation);
        if (movement != null) movement.ResetMotion();
        Physics.SyncTransforms();

        if (controller != null) controller.enabled = ctlWasEnabled;
        if (movement != null) movement.enabled = moveWasEnabled;
        Physics.SyncTransforms();

        LastWarpDelta = (cameraTarget != null ? cameraTarget.position : position) - oldTargetPos;
    }

    /// <summary>True if <paramref name="col"/> belongs to the persistent player rig.</summary>
    public static bool IsPlayerCollider(Collider col)
    {
        if (col == null) return false;
        if (Instance != null && col.transform.IsChildOf(Instance.transform)) return true;
        return col.GetComponentInParent<PersistentPlayer>() != null;
    }
}
