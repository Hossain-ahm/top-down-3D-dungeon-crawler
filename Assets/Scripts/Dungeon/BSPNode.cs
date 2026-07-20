using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents one node in the BSP tree.
/// Each node is either split into two children, or is a leaf containing one room.
/// </summary>
public class BSPNode
{
    // Bounds of this node in grid coordinates
    public int x, y, width, height;

    // Child nodes — null if this is a leaf
    public BSPNode leftChild;
    public BSPNode rightChild;

    // The room carved inside this leaf — null for non-leaf nodes
    public RoomData room;

    // Minimum size a node must be before we stop splitting
    private const int MinSize = 10;

    public BSPNode(int x, int y, int width, int height)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }

    public bool IsLeaf => leftChild == null && rightChild == null;

    /// <summary>
    /// Recursively splits this node. Returns false if too small to split.
    /// rng is passed down so all splits are seed-controlled.
    /// </summary>
    public bool Split(System.Random rng, int depth = 0)
    {
        if (!IsLeaf) return false;

        // Stop splitting if this node is too small
        if (width < MinSize * 2 || height < MinSize * 2) return false;

        // Decide split direction
        // If significantly wider than tall, split vertically
        // If significantly taller than wide, split horizontally
        // Otherwise pick randomly
        bool splitHorizontally;
        if (width > height && (float)width / height >= 1.25f)
            splitHorizontally = false;
        else if (height > width && (float)height / width >= 1.25f)
            splitHorizontally = true;
        else
            splitHorizontally = rng.Next(0, 2) == 0;

        // Maximum split point — ensures both children are at least MinSize
        int max = (splitHorizontally ? height : width) - MinSize;
        if (max <= MinSize) return false;

        int splitPoint = rng.Next(MinSize, max);

        if (splitHorizontally)
        {
            leftChild  = new BSPNode(x, y,              width, splitPoint);
            rightChild = new BSPNode(x, y + splitPoint, width, height - splitPoint);
        }
        else
        {
            leftChild  = new BSPNode(x,              y, splitPoint,         height);
            rightChild = new BSPNode(x + splitPoint, y, width - splitPoint, height);
        }

        // Recurse into children
        leftChild.Split(rng,  depth + 1);
        rightChild.Split(rng, depth + 1);

        return true;
    }

    /// <summary>
    /// Carves a room inside this leaf node.
    /// Room is randomly sized but guaranteed to fit within this node's bounds.
    /// </summary>
    public void CreateRoom(System.Random rng, int minRoomSize, int maxRoomSize)
    {
        if (!IsLeaf)
        {
            // Not a leaf — recurse into children
            leftChild?.CreateRoom(rng, minRoomSize, maxRoomSize);
            rightChild?.CreateRoom(rng, minRoomSize, maxRoomSize);
            return;
        }

        // Room must fit inside this node with at least 1 tile margin on each side
        int roomW = rng.Next(minRoomSize, Mathf.Min(maxRoomSize, width  - 2) + 1);
        int roomH = rng.Next(minRoomSize, Mathf.Min(maxRoomSize, height - 2) + 1);

        // Random offset within the node so room isn't always top-left aligned
        int roomX = x + rng.Next(1, width  - roomW);
        int roomY = y + rng.Next(1, height - roomH);

        room = new RoomData(roomX, roomY, roomW, roomH);
    }

    /// <summary>
    /// Returns the room in this node — or a child's room if this isn't a leaf.
    /// Used when connecting siblings so we always get a valid room to connect to.
    /// </summary>
    public RoomData GetRoom()
    {
        if (room != null) return room;

        RoomData leftRoom  = leftChild?.GetRoom();
        RoomData rightRoom = rightChild?.GetRoom();

        if (leftRoom == null)  return rightRoom;
        if (rightRoom == null) return leftRoom;

        return leftRoom;
    }

    /// <summary>
    /// Collects all leaf rooms into a list.
    /// </summary>
    public void GetAllRooms(List<RoomData> result)
    {
        if (IsLeaf && room != null)
        {
            result.Add(room);
            return;
        }
        leftChild?.GetAllRooms(result);
        rightChild?.GetAllRooms(result);
    }
}