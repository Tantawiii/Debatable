using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

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

    private Scene _homeScene;
    private Coroutine _spawnRoutine;

    private void Awake()
    {
        _homeScene = gameObject.scene;

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

    private void OnEnable()  => LevelStreamer.LevelBecameCurrent += OnLevelChanged;
    private void OnDisable()
    {
        LevelStreamer.LevelBecameCurrent -= OnLevelChanged;
        StopSpawning();
    }

    private void OnDestroy()
    {
        StopSpawning();
        dropPool?.Clear();
    }

    private void OnLevelChanged(LevelInfo now)
    {
        if (now == null || now.Scene != _homeScene) StopSpawning();
    }

    private void StopSpawning()
    {
        if (_spawnRoutine != null) { StopCoroutine(_spawnRoutine); _spawnRoutine = null; }
    }

    private GameObject CreateDrop()
    {
        GameObject instance = Instantiate(dropPrefab);
        if (_homeScene.IsValid())
            SceneManager.MoveGameObjectToScene(instance, _homeScene);
        instance.GetComponent<Drop>().Pool = dropPool;
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
            yield return new WaitForSeconds(spawnInterval);

            SpawnDrop();
        }
    }

    private void SpawnDrop()
    {
        if (!_homeScene.isLoaded || pipeSpawnPoints == null || pipeSpawnPoints.Length == 0) return;

        Transform spawnPoint = pipeSpawnPoints[Random.Range(0, pipeSpawnPoints.Length)];
        GameObject drop = dropPool.Get();

        if (_homeScene.IsValid() && drop.scene != _homeScene)
            SceneManager.MoveGameObjectToScene(drop, _homeScene);

        drop.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
    }
}
