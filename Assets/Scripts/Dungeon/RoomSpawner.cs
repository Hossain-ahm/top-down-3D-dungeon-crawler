using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomSpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject enemyPrefab;
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
    // If empty, falls back to random position within bounds (Room & Corridor)
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
    if (enemyPrefab == null) return;

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
            Vector3 spawnPos = new Vector3(shuffled[i].x, 1f, shuffled[i].y);
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemy.tag = "Enemy";
        }
    }
    else
    {
        for (int i = 0; i < enemyCount; i++)
        {
            float x = roomX + 1 + (float)(rng.NextDouble() * (roomWidth  - 2));
            float z = roomY + 1 + (float)(rng.NextDouble() * (roomHeight - 2));
            GameObject enemy = Instantiate(enemyPrefab, new Vector3(x, 1f, z), Quaternion.identity);
            enemy.tag = "Enemy";
        }
    }
}
}