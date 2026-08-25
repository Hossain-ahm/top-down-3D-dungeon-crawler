using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

/// <summary>
/// Captures a canonical hash of a generated grid plus metadata,
/// appended to a CSV for cross-session comparison.
/// </summary>
public static class ReproducibilityLogger
{
    private static string LogPath =>
        Path.Combine(Application.persistentDataPath, "reproducibility_log.csv");

    /// <summary>
    /// Serialises the grid row-major into a canonical string.
    /// Fixed ordering, single character per cell, no separators —
    /// any formatting variance would produce false negatives.
    /// </summary>
    public static string Serialise(TileGrid grid, int width, int height)
    {
        StringBuilder sb = new StringBuilder(width * height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                switch (grid.GetTile(x, y))
                {
                    case TileType.Floor: sb.Append('F'); break;
                    case TileType.Wall:  sb.Append('W'); break;
                    default:             sb.Append('.'); break;
                }
            }
        }

        return sb.ToString();
    }

    public static string Hash(string serialised)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(serialised));
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    /// <summary>
    /// Counts floor cells — a sanity check independent of the hash.
    /// If hashes differ but floor counts match, suspect serialisation.
    /// </summary>
    public static int CountFloor(string serialised)
    {
        int count = 0;
        foreach (char ch in serialised)
            if (ch == 'F') count++;
        return count;
    }

    public static void Log(
        string sessionId,
        string algorithm,
        int seed,
        int width,
        int height,
        int attempts,
        TileGrid grid,
        long generationMs)
    {
        string serialised = Serialise(grid, width, height);
        string hash = Hash(serialised);
        int floorCount = CountFloor(serialised);

        bool exists = File.Exists(LogPath);

        using (StreamWriter w = new StreamWriter(LogPath, append: true))
        {
            if (!exists)
                w.WriteLine("session,algorithm,seed,width,height,attempts,floor_cells,gen_ms,hash");

            w.WriteLine($"{sessionId},{algorithm},{seed},{width},{height}," +
                        $"{attempts},{floorCount},{generationMs},{hash}");
        }

        Debug.Log($"[Repro] {algorithm} seed={seed} floor={floorCount} " +
                  $"attempts={attempts} {generationMs}ms hash={hash.Substring(0, 12)}…");
    }

    public static void ReportPath()
    {
        Debug.Log($"[Repro] Log file: {LogPath}");
    }
}