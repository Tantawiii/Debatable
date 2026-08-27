using UnityEngine;

public class Drop : MonoBehaviour
{
    [Header("Return Settings")]
    [SerializeField] private float disableY = -10f;

    public UnityEngine.Pool.IObjectPool<GameObject> Pool { get; set; }

    private void Update()
    {
        if (transform.position.y <= disableY)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerRespawn>(out var playerRespawn))
        {
            playerRespawn.Respawn();
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        Pool.Release(gameObject);
    }
}