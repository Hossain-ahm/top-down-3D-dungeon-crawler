using UnityEngine;
using System.Collections.Generic;

public class RandomWalkGenerator : MonoBehaviour
{
    [Header("Seed")]
    public int seed = 12345;
    public bool randomSeedOnStart = false;

    [Header("Grid Settings")]
    public int gridWidth  = 80;
    public int gridHeight = 80;

    [Header("Walk Settings")]
    public int walkLength = 1200;
    public int iterations = 6;

    [Header("Zone Spawning")]
    [Tooltip("How many tiles wide/tall each spawn zone is.")]
    public int zoneSize = 20;

    [Tooltip("Minimum floor tiles in a chunk before it becomes a zone.")]
    public int minTilesPerZone = 10;

    public float spawnDelay = 0.5f;

    [Header("References")]
    public TileGrid tileGrid;
    public GameObject player;
    public GameObject enemyPrefab;
    public float enemiesPerTile = 0.015f;

    private System.Random _rng;
    private List<Vector2Int> _floorTiles = new List<Vector2Int>();

    private static readonly Vector2Int[] Directions = {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    private void Start()
    {
        if (randomSeedOnStart)
            seed = Random.Range(0, int.MaxValue);

        Generate(seed);
    }

    public void Generate(int dungeonSeed)
    {
        seed = dungeonSeed;
        _rng = new System.Random(seed);
        _floorTiles.Clear();

        tileGrid.Initialise(gridWidth, gridHeight);

        Walk();
        tileGrid.SpawnTiles();
        CreateZoneSpawners();
        SpawnPlayer();

        Debug.Log($"Random Walk dungeon generated with seed {seed} — {_floorTiles.Count} floor tiles");
    }

    private void Walk()
    {
        Vector2Int centre = new Vector2Int(gridWidth / 2, gridHeight / 2);

        for (int i = 0; i < iterations; i++)
        {
            Vector2Int pos = centre;
            int steps = walkLength / iterations;

            for (int step = 0; step < steps; step++)
            {
                pos.x = Mathf.Clamp(pos.x, 1, gridWidth  - 2);
                pos.y = Mathf.Clamp(pos.y, 1, gridHeight - 2);

                if (tileGrid.GetTile(pos.x, pos.y) != TileType.Floor)
                {
                    tileGrid.SetTile(pos.x, pos.y, TileType.Floor);
                    _floorTiles.Add(pos);
                }

                int dir = _rng.Next(0, 4);
                pos += Directions[dir];
            }
        }
    }

    private void CreateZoneSpawners()
    {
        // Figure out how many chunks fit in the grid
        int chunksX = Mathf.CeilToInt((float)gridWidth  / zoneSize);
        int chunksY = Mathf.CeilToInt((float)gridHeight / zoneSize);

        // Player spawn is always at grid centre
        Vector2Int playerTile = new Vector2Int(gridWidth / 2, gridHeight / 2);
        int playerChunkX = playerTile.x / zoneSize;
        int playerChunkY = playerTile.y / zoneSize;

        int zoneIndex = 0;

        for (int cx = 0; cx < chunksX; cx++)
        {
            for (int cy = 0; cy < chunksY; cy++)
            {
                // Collect floor tiles that fall inside this chunk
                List<Vector2Int> tilesInChunk = new List<Vector2Int>();
                foreach (var tile in _floorTiles)
                {
                    if (tile.x / zoneSize == cx && tile.y / zoneSize == cy)
                        tilesInChunk.Add(tile);
                }

                // Skip chunks with too few floor tiles — not worth a zone
                if (tilesInChunk.Count < minTilesPerZone) continue;

                // Calculate zone centre in world space
                float worldX = (cx * zoneSize + zoneSize / 2f);
                float worldZ = (cy * zoneSize + zoneSize / 2f);

                // Create trigger zone
                GameObject zoneObj = new GameObject($"CaveZone_{cx}_{cy}");
                zoneObj.transform.position = new Vector3(worldX, 1f, worldZ);

                BoxCollider trigger = zoneObj.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.size = new Vector3(zoneSize - 0.5f, 2f, zoneSize - 0.5f);

                // Wire up spawner
                RoomSpawner spawner = zoneObj.AddComponent<RoomSpawner>();
                spawner.enemyPrefab  = enemyPrefab;
                spawner.spawnDelay   = spawnDelay;
                spawner.roomSeed     = seed + zoneIndex * 1000;

                // Use chunk bounds as the spawn area
                spawner.roomX      = cx * zoneSize;
                spawner.roomY      = cy * zoneSize;
                spawner.roomWidth  = zoneSize;
                spawner.roomHeight = zoneSize;

                // Scale enemy count with how many floor tiles are in this zone
                spawner.enemyCount = Mathf.Max(1,
                    Mathf.RoundToInt(tilesInChunk.Count * enemiesPerTile));

                // Mark the zone containing the player spawn as safe
                spawner.isFirstRoom = (cx == playerChunkX && cy == playerChunkY);

                // Pass confirmed floor tiles so enemies only spawn on walkable ground
                spawner.validFloorTiles = tilesInChunk;
                zoneIndex++;
            }
        }
    }

    private void SpawnPlayer()
    {
        if (player == null) return;
        Vector2Int centre = new Vector2Int(gridWidth / 2, gridHeight / 2);
        player.transform.position = new Vector3(centre.x, 1f, centre.y);
    }
}