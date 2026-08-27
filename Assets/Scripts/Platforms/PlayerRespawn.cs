using UnityEngine;

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
        controller = player.GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (player.position.y <= fallThresholdY)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        // Disable the CharacterController briefly — it blocks direct
        // transform teleportation otherwise (it controls its own position).
        controller.enabled = false;
        player.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        controller.enabled = true;
    }
}