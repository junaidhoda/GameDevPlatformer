
using UnityEngine;
using System.Collections;

public class HazardSpawner : MonoBehaviour
{
    public GameObject seaMinePrefab;
    public GameObject barrelPrefab;

    public int objectsPerWave = 10;
    public float waveInterval = 5f;
    public float barrelDelay = 2f;
    public float spawnHeight = 8f;
    public float spawnRange = 35f;

    void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        while (true)
        {
            SpawnWave(seaMinePrefab);

            yield return new WaitForSeconds(barrelDelay);

            SpawnWave(barrelPrefab);

            yield return new WaitForSeconds(
                waveInterval - barrelDelay
            );
        }
    }

    void SpawnWave(GameObject prefab)
    {
        if (prefab == null) return;

        for (int i = 0; i < objectsPerWave; i++)
        {
            float x = Random.Range(-spawnRange, spawnRange);
            float z = Random.Range(-spawnRange, spawnRange);

            Vector3 spawnPosition = new Vector3(
                x, spawnHeight, z
            );

            GameObject hazard = Instantiate(
                prefab,
                spawnPosition,
                Quaternion.identity
            );

            Destroy(hazard, 8f);
        }
    }
}
