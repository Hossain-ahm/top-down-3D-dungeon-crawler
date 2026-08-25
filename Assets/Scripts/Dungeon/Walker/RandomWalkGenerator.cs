using UnityEngine;
using System.Collections;
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
    public int zoneSize = 20;
    public int minTilesPerZone = 10;
    public float spawnDelay = 0.5f;

    [Header("References")]
    public TileGrid tileGrid;
    public GameObject player;
    public float enemiesPerTile = 0.015f;
	
[Header("Enemy Prefabs")]
public GameObject meleePrefab;
public GameObject rangedPrefab;
	

    [Header("Entrance & Exit Rooms")]
    public int roomWidth  = 8;
    public int roomHeight = 6;

    [Header("Entrance & Exit Door")]
    public GameObject EntranceDoorPrefab;
    public GameObject ExitDoorPrefab;
    private Vector3 _entranceDoorPos;
    private Vector3 _exitDoorPos;

    [Header("Level Management")]
    public LevelManager levelManager;

    [Range(0f, 1f)]
    public float exitBias = 0.3f;

    private RoomData _entranceRoom;
    private RoomData _exitRoom;
    private Vector2Int _entranceCentre;
    private Vector2Int _exitCentre;
    private int _config;

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
    }

public void Generate(int dungeonSeed)
{
    int attempts = 0;
    const int maxAttempts = 10;

    do
    {
        seed = dungeonSeed + attempts;
        _rng = new System.Random(seed);
        _floorTiles.Clear();

        tileGrid.Initialise(gridWidth, gridHeight);

        PlaceEntranceAndExitRooms();
        Walk();

        attempts++;

        if (attempts >= maxAttempts)
        {
            Debug.LogWarning("Could not generate traversable cave after max attempts");
            break;
        }

    } while (!IsTraversable());

    // Only spawn tiles and objects once traversability is confirmed
    tileGrid.SpawnTiles();
    CreateZoneSpawners();
    SpawnPlayer();
    PlaceDoors();

    Debug.Log($"Cave generated with seed {seed} after {attempts} attempt(s)");
}

    private void PlaceEntranceAndExitRooms()
    {
        _config = _rng.Next(0, 4);

        switch (_config)
        {
            case 0:
                _entranceRoom = new RoomData(5, gridHeight / 2 - roomHeight / 2, roomWidth, roomHeight);
                _exitRoom     = new RoomData(gridWidth - roomWidth - 5, gridHeight / 2 - roomHeight / 2, roomWidth, roomHeight);
                break;
            case 1:
                _entranceRoom = new RoomData(gridWidth - roomWidth - 5, gridHeight / 2 - roomHeight / 2, roomWidth, roomHeight);
                _exitRoom     = new RoomData(5, gridHeight / 2 - roomHeight / 2, roomWidth, roomHeight);
                break;
            case 2:
                _entranceRoom = new RoomData(gridWidth / 2 - roomWidth / 2, 5, roomWidth, roomHeight);
                _exitRoom     = new RoomData(gridWidth / 2 - roomWidth / 2, gridHeight - roomHeight - 5, roomWidth, roomHeight);
                break;
            case 3:
                _entranceRoom = new RoomData(gridWidth / 2 - roomWidth / 2, gridHeight - roomHeight - 5, roomWidth, roomHeight);
                _exitRoom     = new RoomData(gridWidth / 2 - roomWidth / 2, 5, roomWidth, roomHeight);
                break;
        }

        _entranceCentre = _entranceRoom.Centre;
        _exitCentre     = _exitRoom.Centre;

        switch (_config)
        {
            case 0:
                _entranceDoorPos = new Vector3(_entranceRoom.x + _entranceRoom.width - 1, 1f, _entranceCentre.y);
                _exitDoorPos     = new Vector3(_exitRoom.x, 1f, _exitCentre.y);
                break;
            case 1:
                _entranceDoorPos = new Vector3(_entranceRoom.x, 1f, _entranceCentre.y);
                _exitDoorPos     = new Vector3(_exitRoom.x + _exitRoom.width - 1, 1f, _exitCentre.y);
                break;
            case 2:
                _entranceDoorPos = new Vector3(_entranceCentre.x, 1f, _entranceRoom.y + _entranceRoom.height - 1);
                _exitDoorPos     = new Vector3(_exitCentre.x, 1f, _exitRoom.y);
                break;
            case 3:
                _entranceDoorPos = new Vector3(_entranceCentre.x, 1f, _entranceRoom.y);
                _exitDoorPos     = new Vector3(_exitCentre.x, 1f, _exitRoom.y + _exitRoom.height - 1);
                break;
        }

        CarveRoom(_entranceRoom);
        CarveRoom(_exitRoom);
    }

    private void CarveRoom(RoomData room)
    {
        for (int x = room.x; x < room.x + room.width; x++)
        {
            for (int y = room.y; y < room.y + room.height; y++)
            {
                tileGrid.SetTile(x, y, TileType.Floor);
                _floorTiles.Add(new Vector2Int(x, y));
            }
        }
    }

    private void Walk()
    {
        Vector2Int startPos = _entranceCentre;

        for (int i = 0; i < iterations; i++)
        {
            Vector2Int pos = startPos;
            int steps = walkLength / iterations;

            for (int step = 0; step < steps; step++)
            {
                pos.x = Mathf.Clamp(pos.x, 1, gridWidth  - 3);
                pos.y = Mathf.Clamp(pos.y, 1, gridHeight - 3);

                tileGrid.SetTile(pos.x,     pos.y,     TileType.Floor);
                tileGrid.SetTile(pos.x + 1, pos.y,     TileType.Floor);
                tileGrid.SetTile(pos.x,     pos.y + 1, TileType.Floor);
                tileGrid.SetTile(pos.x + 1, pos.y + 1, TileType.Floor);

                if (!_floorTiles.Contains(pos))
                {
                    _floorTiles.Add(pos);
                    _floorTiles.Add(new Vector2Int(pos.x + 1, pos.y));
                    _floorTiles.Add(new Vector2Int(pos.x,     pos.y + 1));
                    _floorTiles.Add(new Vector2Int(pos.x + 1, pos.y + 1));
                }

                if (_rng.NextDouble() < exitBias)
                {
                    Vector2Int toExit = _exitCentre - pos;
                    if (Mathf.Abs(toExit.x) > Mathf.Abs(toExit.y))
                        pos.x += (int)Mathf.Sign(toExit.x);
                    else
                        pos.y += (int)Mathf.Sign(toExit.y);
                }
                else
                {
                    int dir = _rng.Next(0, 4);
					//int dir = UnityEngine.Random.Range(0, 4);
                    pos += Directions[dir];
                }
            }
        }
    }

    private void CreateZoneSpawners()
    {
        int chunksX = Mathf.CeilToInt((float)gridWidth  / zoneSize);
        int chunksY = Mathf.CeilToInt((float)gridHeight / zoneSize);

        Vector2Int playerTile = _entranceCentre;
        int playerChunkX = playerTile.x / zoneSize;
        int playerChunkY = playerTile.y / zoneSize;

        int zoneIndex = 0;

        for (int cx = 0; cx < chunksX; cx++)
        {
            for (int cy = 0; cy < chunksY; cy++)
            {
                List<Vector2Int> tilesInChunk = new List<Vector2Int>();
                foreach (var tile in _floorTiles)
                {
                    if (tile.x / zoneSize == cx && tile.y / zoneSize == cy)
                        tilesInChunk.Add(tile);
                }

                if (tilesInChunk.Count < minTilesPerZone) continue;

                float worldX = (cx * zoneSize + zoneSize / 2f);
                float worldZ = (cy * zoneSize + zoneSize / 2f);

                GameObject zoneObj = new GameObject($"CaveZone_{cx}_{cy}");
                zoneObj.tag = "Zone";
                zoneObj.transform.position = new Vector3(worldX, 1f, worldZ);

                BoxCollider trigger = zoneObj.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.size = new Vector3(zoneSize - 0.5f, 2f, zoneSize - 0.5f);

                RoomSpawner spawner = zoneObj.AddComponent<RoomSpawner>();
                spawner.meleePrefab = meleePrefab;
    			spawner.rangedPrefab = rangedPrefab;
				spawner.meleeRatio   = 0.6f;
                spawner.spawnDelay   = spawnDelay;
                spawner.roomSeed     = seed + zoneIndex * 1000;
                spawner.roomX        = cx * zoneSize;
                spawner.roomY        = cy * zoneSize;
                spawner.roomWidth    = zoneSize;
                spawner.roomHeight   = zoneSize;
                spawner.enemyCount   = Mathf.Max(1,
                    Mathf.RoundToInt(tilesInChunk.Count * enemiesPerTile));
                spawner.isFirstRoom  = (cx == playerChunkX && cy == playerChunkY);
                spawner.validFloorTiles = tilesInChunk;
                zoneIndex++;
            }
        }
    }

    private void SpawnPlayer()
    {
        if (player == null) return;

        Vector3 spawnPos = new Vector3(_entranceCentre.x, 1.5f, _entranceCentre.y);

        Rigidbody rb = player.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        player.transform.position = spawnPos;
        Physics.SyncTransforms();
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
    }



private void PlaceDoors()
{
    
    if (EntranceDoorPrefab == null || ExitDoorPrefab == null)
    {
        Debug.LogError("Door prefabs not assigned!");
        return;
    }

    GameObject entranceDoor = Instantiate(EntranceDoorPrefab, _entranceDoorPos, Quaternion.identity);
    entranceDoor.tag = "Door";
    DoorInteractable entranceInteract = entranceDoor.GetComponent<DoorInteractable>();
    entranceInteract.isEntrance   = true;
    entranceInteract.levelManager = levelManager;

GameObject exitDoor = Instantiate(ExitDoorPrefab, _exitDoorPos, Quaternion.identity);
exitDoor.tag = "Door";

DoorInteractable exitInteract = exitDoor.GetComponent<DoorInteractable>();
if (exitInteract == null)
{
    return;
}
exitInteract.isEntrance   = false;
exitInteract.levelManager = levelManager;
}

    public void ClearDungeon()
    {
        foreach (Transform child in tileGrid.transform)
            DestroyImmediate(child.gameObject);

        foreach (GameObject zone in GameObject.FindGameObjectsWithTag("Zone"))
            DestroyImmediate(zone);

        foreach (GameObject door in GameObject.FindGameObjectsWithTag("Door"))
            DestroyImmediate(door);

        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            DestroyImmediate(enemy);
		foreach (GameObject proj in GameObject.FindGameObjectsWithTag("EnemyProjectile"))
    		DestroyImmediate(proj);

        _floorTiles.Clear();
    }

private bool IsTraversable()
{
    // Flood fill from entrance centre
    // Check if exit centre is reachable
    HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
    Queue<Vector2Int> queue = new Queue<Vector2Int>();

    queue.Enqueue(_entranceCentre);
    visited.Add(_entranceCentre);

    while (queue.Count > 0)
    {
        Vector2Int current = queue.Dequeue();

        // If we reached the exit, cave is traversable
        if (current == _exitCentre)
            return true;

        // Check all 4 neighbours
        foreach (var dir in Directions)
        {
            Vector2Int neighbour = current + dir;
            if (!visited.Contains(neighbour) && 
                tileGrid.GetTile(neighbour.x, neighbour.y) == TileType.Floor)
            {
                visited.Add(neighbour);
                queue.Enqueue(neighbour);
            }
        }
    }

    return false;
}

public int LastAttemptCount { get; private set; }

/// <summary>
/// Generation without instantiation, spawning or player placement.
/// Produces the grid only — used by the evaluation harness.
/// </summary>
public void GenerateForEvaluation(int dungeonSeed)
{
    int attempts = 0;
    const int maxAttempts = 10;

    do
    {
        seed = dungeonSeed + attempts;
        _rng = new System.Random(seed);
        _floorTiles.Clear();

        tileGrid.Initialise(gridWidth, gridHeight);

        PlaceEntranceAndExitRooms();
        Walk();

        attempts++;

        if (attempts >= maxAttempts)
            break;

    } while (!IsTraversable());

    LastAttemptCount = attempts;
}
}