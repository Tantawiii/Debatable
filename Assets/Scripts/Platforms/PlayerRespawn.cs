using UnityEngine;

/// <summary>
/// CharacterController-safe teleport back to a spawn point. Lives on the persistent player rig;
/// <see cref="spawnPoint"/> and <see cref="fallThresholdY"/> are set per-level by LevelStreamer.
/// Also used by lava (<see cref="LavaKill"/>) and falling lava blobs (<see cref="Drop"/>).
/// </summary>
public class PlayerRespawn : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform spawnPoint;

    [Header("Fall Detection")]
    [SerializeField] private float fallThresholdY = -10f;

    private CharacterController controller;

    private void Awake()
    {
        if (player != null) controller = player.GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (spawnPoint != null && player != null && player.position.y <= fallThresholdY)
        {
            Respawn();
        }
    }

    public void SetSpawnPoint(Transform t) => spawnPoint = t;
    public void SetFallThreshold(float y) => fallThresholdY = y;

    public void Respawn()
    {
        if (player == null || spawnPoint == null) return;
        if (controller == null) controller = player.GetComponent<CharacterController>();

        // Disable the CharacterController briefly — it blocks direct
        // transform teleportation otherwise (it controls its own position).
        if (controller != null) controller.enabled = false;
        player.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        if (player.TryGetComponent<PlayerMovement>(out var move)) move.ResetMotion();
        Physics.SyncTransforms();
        if (controller != null) controller.enabled = true;
    }
}
