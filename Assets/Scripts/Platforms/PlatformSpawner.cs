using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PlatformSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints = new Transform[3];

    [Header("Platforms")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private GameObject platformWithLightPrefab;

    [Header("Pool")]
    [SerializeField] private int defaultCapacity = 5;
    [SerializeField] private int maxSize = 20;

    [Header("Spawn Probability")]
    [SerializeField] private int platformWeight = 4;
    [SerializeField] private int platformWithLightWeight = 1;

    [Header("Timing")]
    [SerializeField] private float spawnInterval = 1f;

    private IObjectPool<GameObject> platformPool;
    private IObjectPool<GameObject> platformWithLightPool;
    private int lastSpawnIndex = -1;

    private void Awake()
    {
        platformPool = new ObjectPool<GameObject>(
            createFunc: () => CreatePlatform(platformPrefab, PlatformType.Normal),
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: Destroy,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );

        platformWithLightPool = new ObjectPool<GameObject>(
            createFunc: () => CreatePlatform(platformWithLightPrefab, PlatformType.Light),
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: Destroy,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    private GameObject CreatePlatform(GameObject prefab, PlatformType type)
    {
        GameObject instance = Instantiate(prefab);
        instance.GetComponent<PooledPlatform>().Type = type;
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
            int spawnIndex = GetNextSpawnIndex();
            SpawnPlatform(spawnPoints[spawnIndex]);
            lastSpawnIndex = spawnIndex;

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private int GetNextSpawnIndex()
    {
        if (spawnPoints.Length <= 1)
        {
            return 0;
        }

        int index;
        do
        {
            index = Random.Range(0, spawnPoints.Length);
        }
        while (index == lastSpawnIndex);

        return index;
    }

    private void SpawnPlatform(Transform spawnPoint)
    {
        bool useNormal = Random.Range(0, platformWeight + platformWithLightWeight) < platformWeight;
        GameObject platform = useNormal ? platformPool.Get() : platformWithLightPool.Get();

        platform.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        platform.GetComponent<PlatformMover>().Initialize(this);
    }

    public void ReturnPlatform(GameObject platform)
    {
        PooledPlatform pooled = platform.GetComponent<PooledPlatform>();
        IObjectPool<GameObject> pool = pooled.Type == PlatformType.Normal ? platformPool : platformWithLightPool;

        pool.Release(platform);
    }
}