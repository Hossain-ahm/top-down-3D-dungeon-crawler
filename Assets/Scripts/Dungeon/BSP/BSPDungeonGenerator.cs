using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates a dungeon using Binary Space Partitioning.
/// Recursively splits the grid into sections, places one room per section,
/// then connects sibling rooms back up the tree hierarchy.
/// Room overlap is structurally impossible — each room lives in its own partition.
/// Seed-controlled and deterministic via System.Random.
/// </summary>
public class BSPDungeonGenerator : MonoBehaviour
{
    [Header("Seed")]
    public int seed = 12345;
    public bool randomSeedOnStart = false;

    [Header("Grid Settings")]
    public int gridWidth  = 80;
    public int gridHeight = 80;

    [Header("Room Settings")]
    public int minRoomSize = 5;
    public int maxRoomSize = 10;

    [Header("References")]
    public TileGrid tileGrid;
    public GameObject player;

[Header("Enemy Spawning")]
public float enemiesPerTile = 0.015f;
public float spawnDelay = 0.5f;

[Header("Enemy Prefabs")]
public GameObject meleePrefab;
public GameObject rangedPrefab;


    private System.Random _rng;
    private BSPNode _rootNode;
    private List<RoomData> _rooms = new List<RoomData>();

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

        // Build the BSP tree
        _rootNode = new BSPNode(0, 0, gridWidth, gridHeight);
        _rootNode.Split(_rng);

        // Place one room in each leaf
        _rootNode.CreateRoom(_rng, minRoomSize, maxRoomSize);

        // Collect all rooms
        _rootNode.GetAllRooms(_rooms);

        // Carve rooms and corridors into the grid
        CarveRooms();
        ConnectRooms(_rootNode);

        tileGrid.SpawnTiles();
        CreateRoomSpawners();
        SpawnPlayer();

        Debug.Log($"BSP dungeon generated with seed {seed} — {_rooms.Count} rooms");
    }

    private void CarveRooms()
    {
        foreach (var room in _rooms)
            for (int x = room.x; x < room.x + room.width; x++)
                for (int y = room.y; y < room.y + room.height; y++)
                    tileGrid.SetTile(x, y, TileType.Floor);
    }

    /// <summary>
    /// Recursively connects sibling rooms back up the BSP tree.
    /// Each node connects its left child's room to its right child's room.
    /// </summary>
    private void ConnectRooms(BSPNode node)
    {
        if (node.IsLeaf) return;

        // Recurse into children first
        ConnectRooms(node.leftChild);
        ConnectRooms(node.rightChild);

        // Connect this node's two children to each other
        RoomData leftRoom  = node.leftChild.GetRoom();
        RoomData rightRoom = node.rightChild.GetRoom();

        if (leftRoom != null && rightRoom != null)
            CorridorBuilder.Connect(tileGrid, leftRoom.Centre, rightRoom.Centre, _rng);
    }

    private void CreateRoomSpawners()
    {
        for (int i = 0; i < _rooms.Count; i++)
        {
            RoomData room = _rooms[i];

            GameObject zoneObj = new GameObject($"BSPRoom_{i}");
            zoneObj.transform.position = new Vector3(
                room.x + room.width  / 2f,
                1f,
                room.y + room.height / 2f
            );

            BoxCollider trigger = zoneObj.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(room.width - 0.5f, 2f, room.height - 0.5f);

            RoomSpawner spawner = zoneObj.AddComponent<RoomSpawner>();
            spawner.meleePrefab  = meleePrefab;
			spawner.rangedPrefab = rangedPrefab;
            spawner.spawnDelay   = spawnDelay;
            spawner.roomX        = room.x;
            spawner.roomY        = room.y;
            spawner.roomWidth    = room.width;
            spawner.roomHeight   = room.height;
            spawner.roomSeed     = seed + i * 1000;
            spawner.isFirstRoom  = (i == 0);
            spawner.enemyCount   = Mathf.Max(1,
                Mathf.RoundToInt(room.width * room.height * enemiesPerTile));
        }
    }

    private void SpawnPlayer()
    {
        if (_rooms.Count == 0 || player == null) return;
        Vector2Int centre = _rooms[0].Centre;
        player.transform.position = new Vector3(centre.x, 1f, centre.y);
    }
}