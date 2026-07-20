using UnityEngine;

/// <summary>
/// Pure data — describes a room's position and size on the tile grid.
/// No MonoBehaviour needed; this is just a data container.
/// </summary>
public class RoomData
{
    public int x;       // grid column of top-left corner
    public int y;       // grid row of top-left corner
    public int width;
    public int height;

    public RoomData(int x, int y, int width, int height)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }

    // Centre of the room in grid coordinates
    public Vector2Int Centre => new Vector2Int(x + width / 2, y + height / 2);

    // Returns true if this room overlaps another (with 1-tile padding)
    public bool Overlaps(RoomData other)
    {
        return x - 1 < other.x + other.width  &&
               x + width  + 1 > other.x       &&
               y - 1 < other.y + other.height &&
               y + height + 1 > other.y;
    }
}