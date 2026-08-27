using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class DropSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] pipeSpawnPoints;

    [Header("Drop")]
    [SerializeField] private GameObject dropPrefab;

    [Header("Pool")]
    [SerializeField] private int defaultCapacity = 5;
    [SerializeField] private int maxSize = 20;

    [Header("Timing")]
    [SerializeField] private float spawnInterval = 2f;

    private IObjectPool<GameObject> dropPool;

    private void Awake()
    {
        dropPool = new ObjectPool<GameObject>(
            createFunc: CreateDrop,
            actionOnGet: obj =>
            {
                obj.SetActive(true);

                if (obj.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            },
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: Destroy,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    private GameObject CreateDrop()
    {
        GameObject instance = Instantiate(dropPrefab);
        instance.GetComponent<Drop>().Pool = dropPool;
        return instance;
    }

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            SpawnDrop();
        }
    }

    private void SpawnDrop()
    {
        Transform spawnPoint = pipeSpawnPoints[Random.Range(0, pipeSpawnPoints.Length)];
        GameObject drop = dropPool.Get();

        drop.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
    }
}