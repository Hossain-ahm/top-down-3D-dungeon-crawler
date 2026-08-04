using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomSpawner : MonoBehaviour
{
    [Header("Spawning")]
public GameObject meleePrefab;
public GameObject rangedPrefab;
[Range(0f, 1f)]
public float meleeRatio = 0.6f;
[Tooltip("Tiles closer than this distance to the room center spawn melee enemies; further tiles spawn ranged enemies.")]
public float splitDistance = 4f; 
public float spawnDelay = 0.5f;


    [Header("Room Data")]
    public int roomX;
    public int roomY;
    public int roomWidth;
    public int roomHeight;
    public int enemyCount;
    public int roomSeed;

    [Header("State")]
    public bool hasSpawned = false;
    public bool isFirstRoom = false;

    // Valid floor tile positions — set by the generator
    [HideInInspector]
    public List<Vector2Int> validFloorTiles = new List<Vector2Int>();

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (hasSpawned || isFirstRoom) return;

        StartCoroutine(SpawnAfterDelay());
    }

    private IEnumerator SpawnAfterDelay()
    {
        hasSpawned = true;
        yield return new WaitForSeconds(spawnDelay);
        SpawnEnemies();
    }

    private void SpawnEnemies()
{
    if (meleePrefab == null && rangedPrefab == null) return;

    System.Random rng = new System.Random(roomSeed);

    if (validFloorTiles.Count > 0)
    {
        List<Vector2Int> shuffled = new List<Vector2Int>(validFloorTiles);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            Vector2Int temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }

        int count = Mathf.Min(enemyCount, shuffled.Count);
        for (int i = 0; i < count; i++)
        {
            GameObject prefabToSpawn = GetEnemyPrefab(rng);
            if (prefabToSpawn != null)
            {
                Vector3 spawnPos = new Vector3(shuffled[i].x, 1.5f, shuffled[i].y);
                GameObject enemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
                enemy.tag = "Enemy";
            }
        }
    }
    else
    {
        for (int i = 0; i < enemyCount; i++)
        {
            float x = roomX + 1 + (float)(rng.NextDouble() * (roomWidth - 2));
            float z = roomY + 1 + (float)(rng.NextDouble() * (roomHeight - 2));

            GameObject prefabToSpawn = GetEnemyPrefab(rng);
            if (prefabToSpawn != null)
            {
                GameObject enemy = Instantiate(prefabToSpawn, new Vector3(x, 1.5f, z), Quaternion.identity);
                enemy.tag = "Enemy";
            }
        }
    }
}

private GameObject GetEnemyPrefab(System.Random rng)
{
    float roll = (float)rng.NextDouble();

    if (roll < meleeRatio && meleePrefab != null)
        return meleePrefab;
    else if (rangedPrefab != null)
        return rangedPrefab;
    else
        return meleePrefab;
}
}