using UnityEngine;

public class FigureCapture : MonoBehaviour
{
    public int seed = 1000;
    public int gridSize = 80;

    public TileGrid tileGrid;
    public RandomWalkGenerator walk;
    public DungeonGenerator rooms;
    public BSPDungeonGenerator bsp;
[ContextMenu("Generate Random Walk")]
public void GenWalk()
{
    Debug.Log($"walk null: {walk == null}, tileGrid null: {tileGrid == null}");
    if (walk == null || tileGrid == null) return;
    
    walk.gridWidth = walk.gridHeight = gridSize;
    walk.GenerateForEvaluation(seed);
    tileGrid.SpawnTiles();
}
    [ContextMenu("Generate Room and Corridor")]
    public void GenRooms()
    {
        rooms.gridWidth = rooms.gridHeight = gridSize;
        rooms.Generate(seed);
    }

    [ContextMenu("Generate BSP")]
    public void GenBSP()
    {
        bsp.gridWidth = bsp.gridHeight = gridSize;
        bsp.Generate(seed);
    }
}