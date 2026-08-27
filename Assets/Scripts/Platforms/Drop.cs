using UnityEngine;

public class Drop : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerRespawn>(out var playerRespawn))
        {
            playerRespawn.Respawn();
            Destroy(gameObject);
        }
    }
}