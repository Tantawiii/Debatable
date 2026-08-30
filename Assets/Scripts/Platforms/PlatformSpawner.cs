using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

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

    private Scene _homeScene;
    private Coroutine _spawnRoutine;

    private void Awake()
    {
        _homeScene = gameObject.scene;

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

    private void OnEnable()  => LevelStreamer.LevelBecameCurrent += OnLevelChanged;
    private void OnDisable()
    {
        LevelStreamer.LevelBecameCurrent -= OnLevelChanged;
        StopSpawning();
    }

    private void OnDestroy()
    {
        StopSpawning();
        platformPool?.Clear();
        platformWithLightPool?.Clear();
    }

    private void OnLevelChanged(LevelInfo now)
    {
        // once another scene is the current level, this spawner's level is on its way out
        if (now == null || now.Scene != _homeScene) StopSpawning();
    }

    private void StopSpawning()
    {
        if (_spawnRoutine != null) { StopCoroutine(_spawnRoutine); _spawnRoutine = null; }
    }

    private GameObject CreatePlatform(GameObject prefab, PlatformType type)
    {
        GameObject instance = Instantiate(prefab);
        if (_homeScene.IsValid())
            SceneManager.MoveGameObjectToScene(instance, _homeScene);
        instance.GetComponent<PooledPlatform>().Type = type;
        return instance;
    }

    private void Start()
    {
        _spawnRoutine = StartCoroutine(SpawnLoop());
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
        if (!_homeScene.isLoaded) return;

        bool useNormal = Random.Range(0, platformWeight + platformWithLightWeight) < platformWeight;
        GameObject platform = useNormal ? platformPool.Get() : platformWithLightPool.Get();

        if (_homeScene.IsValid() && platform.scene != _homeScene)
            SceneManager.MoveGameObjectToScene(platform, _homeScene);

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
