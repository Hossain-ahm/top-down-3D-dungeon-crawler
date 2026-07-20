using UnityEngine;

public enum TileType { Empty, Floor, Wall }

/// <summary>
/// Holds the 2D tile array and spawns GameObjects for each tile.
/// </summary>
public class TileGrid : MonoBehaviour
{
    [Header("Tile Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;

    [Header("Tile Size")]
    public float tileSize = 1f;

    private TileType[,] _grid;
    private int _width;
    private int _height;

    public void Initialise(int width, int height)
    {
        _width = width;
        _height = height;
        _grid = new TileType[width, height];
    }

    public void SetTile(int x, int y, TileType type)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height) return;
        _grid[x, y] = type;
    }

    public TileType GetTile(int x, int y)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height) return TileType.Empty;
        return _grid[x, y];
    }

    /// <summary>
    /// After all tiles are set, spawn the GameObjects.
    /// Walls are placed around every floor tile that borders an empty cell.
    /// </summary>
    public void SpawnTiles()
    {
        // Clear previous tiles (for re-generation)
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_grid[x, y] == TileType.Floor)
                {
                    SpawnTile(floorPrefab, x, y, "Floor");

                    // Check 4 neighbours — spawn walls on empty borders
                    TrySpawnWall(x + 1, y);
                    TrySpawnWall(x - 1, y);
                    TrySpawnWall(x, y + 1);
                    TrySpawnWall(x, y - 1);
                }
            }
        }
    }

    private void TrySpawnWall(int x, int y)
    {
        if (GetTile(x, y) == TileType.Empty)
        {
            SetTile(x, y, TileType.Wall);
            SpawnTile(wallPrefab, x, y, "Wall");
        }
    }

    private void SpawnTile(GameObject prefab, int x, int y, string label)
    {
        if (prefab == null) return;
        Vector3 pos = new Vector3(x * tileSize, 0f, y * tileSize);
        GameObject tile = Instantiate(prefab, pos, Quaternion.identity, transform);
        tile.name = $"{label}_{x}_{y}";

    if (label == "Floor")
        tile.layer = LayerMask.NameToLayer("Ground");
    }
}