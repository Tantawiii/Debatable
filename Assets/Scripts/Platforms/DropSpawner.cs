using System.Collections;
using UnityEngine;

public class DropSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] pipeSpawnPoints;

    [Header("Drop")]
    [SerializeField] private GameObject dropPrefab;

    [Header("Timing")]
    [SerializeField] private float spawnInterval = 2f;

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

        Instantiate(dropPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}