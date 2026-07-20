using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Connects rooms with L-shaped corridors on the tile grid.
/// </summary>
public static class CorridorBuilder
{
    /// <summary>
    /// Carves an L-shaped corridor between two grid points.
    /// The bend direction is chosen by the Random instance (seed-controlled).
    /// </summary>
    public static void Connect(TileGrid grid, Vector2Int a, Vector2Int b, System.Random rng)
    {
        // Randomly decide whether to go horizontal-first or vertical-first
        if (rng.Next(0, 2) == 0)
        {
            CarveHorizontal(grid, a.x, b.x, a.y);
            CarveVertical(grid, a.y, b.y, b.x);
        }
        else
        {
            CarveVertical(grid, a.y, b.y, a.x);
            CarveHorizontal(grid, a.x, b.x, b.y);
        }
    }

    private static void CarveHorizontal(TileGrid grid, int x1, int x2, int y)
    {
        int minX = Mathf.Min(x1, x2);
        int maxX = Mathf.Max(x1, x2);
        for (int x = minX; x <= maxX; x++)
            grid.SetTile(x, y, TileType.Floor);
    }

    private static void CarveVertical(TileGrid grid, int y1, int y2, int x)
    {
        int minY = Mathf.Min(y1, y2);
        int maxY = Mathf.Max(y1, y2);
        for (int y = minY; y <= maxY; y++)
            grid.SetTile(x, y, TileType.Floor);
    }
}