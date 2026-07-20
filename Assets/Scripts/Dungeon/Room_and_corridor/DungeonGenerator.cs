using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Main dungeon generator using room and corridor prefab placement.
/// Seed-based and fully deterministic — same seed always produces same dungeon.
/// This is the key networking insight: only the seed needs to be synced in M3.
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    [Header("Seed")]
    public int seed = 12345;
    public bool randomSeedOnStart = false;

    [Header("Grid Settings")]
    public int gridWidth = 80;
    public int gridHeight = 80;

    [Header("Room Settings")]
    public int maxRooms = 15;
    public int minRoomSize = 5;
    public int maxRoomSize = 12;

    [Header("References")]
    public TileGrid tileGrid;

    [Header("Player Spawn")]
    public GameObject player;
    
    [Header("Enemy Spawning")]
    public GameObject enemyPrefab;
    public float enemiesPerTile = 0.015f; // scales enemy count with room size

    private List<RoomData> _rooms = new List<RoomData>();
    private System.Random _rng;

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
        _rooms.Clear();

        tileGrid.Initialise(gridWidth, gridHeight);

        PlaceRooms();
        ConnectRooms();
        tileGrid.SpawnTiles();
        CreateRoomSpawners();
        SpawnPlayer();

        Debug.Log($"Dungeon generated with seed {seed} — {_rooms.Count} rooms");
    }

    private void PlaceRooms()
    {
        int attempts = 0;
        int maxAttempts = maxRooms * 5;

        while (_rooms.Count < maxRooms && attempts < maxAttempts)
        {
            attempts++;

            int w = _rng.Next(minRoomSize, maxRoomSize + 1);
            int h = _rng.Next(minRoomSize, maxRoomSize + 1);
            int x = _rng.Next(1, gridWidth  - w - 1);
            int y = _rng.Next(1, gridHeight - h - 1);

            RoomData candidate = new RoomData(x, y, w, h);

            bool overlaps = false;
            foreach (var existing in _rooms)
            {
                if (candidate.Overlaps(existing))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                CarveRoom(candidate);
                _rooms.Add(candidate);
            }
        }
    }

    private void CarveRoom(RoomData room)
    {
        for (int x = room.x; x < room.x + room.width; x++)
            for (int y = room.y; y < room.y + room.height; y++)
                tileGrid.SetTile(x, y, TileType.Floor);
    }

    private void ConnectRooms()
    {
        // Connect each room to the next one in the list
        // Simple chain — guarantees full connectivity
        for (int i = 0; i < _rooms.Count - 1; i++)
            CorridorBuilder.Connect(tileGrid, _rooms[i].Centre, _rooms[i + 1].Centre, _rng);
    }

    private void SpawnPlayer()
    {
        if (_rooms.Count == 0 || player == null) return;

        // Always spawn in the first room's centre
        Vector2Int centre = _rooms[0].Centre;
        player.transform.position = new Vector3(centre.x, 1f, centre.y);
    }
    
    private void CreateRoomSpawners()
    {
        for (int i = 0; i < _rooms.Count; i++)
        {
            RoomData room = _rooms[i];

            // Create an invisible trigger zone covering the room floor
            GameObject zoneObj = new GameObject($"RoomZone_{i}");
            zoneObj.transform.position = new Vector3(
                room.x + room.width  / 2f,
                1f,
                room.y + room.height / 2f
            );

            // BoxCollider sized to the room
            BoxCollider trigger = zoneObj.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(room.width - 0.5f, 2f, room.height - 0.5f);

            // Wire up the spawner
            RoomSpawner spawner = zoneObj.AddComponent<RoomSpawner>();
            spawner.enemyPrefab  = enemyPrefab;
            spawner.roomX        = room.x;
            spawner.roomY        = room.y;
            spawner.roomWidth    = room.width;
            spawner.roomHeight   = room.height;
            spawner.isFirstRoom  = (i == 0);

            // Scale enemy count with room area, minimum 1
            int area = room.width * room.height;
            spawner.enemyCount = Mathf.Max(1, Mathf.RoundToInt(area * enemiesPerTile));

            // Derive a unique seed for this room from the main seed
            // so enemy positions are deterministic per room
            spawner.roomSeed = seed + i * 1000;
        }
    }
}