using UnityEngine;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Text;
using CompressionLevel = System.IO.Compression.CompressionLevel;
/// <summary>
/// Measures the payload cost of transmitting a generated level as
/// grid data versus as a seed, across a range of grid sizes.
/// Answers RQ2 analytically; requires no network layer.
/// </summary>
public class NetworkCostHarness : MonoBehaviour
{
    [Header("Grid Sizes")]
    public int[] gridSizes = { 40, 60, 80, 120, 160, 200 };

    [Header("Seeds")]
    [Tooltip("Averaged over this many seeds per size — grid content affects compression.")]
    public int seedsPerSize = 10;
    public int firstSeed = 1000;
    public int seedStride = 137;

    [Header("References")]
    public RandomWalkGenerator walkGenerator;
    public TileGrid tileGrid;

    private string LogPath =>
        Path.Combine(Application.persistentDataPath, "network_cost_log.csv");
    
    

    private void Start()
    {
        Debug.Log($"[NetCost] Log file: {LogPath}");
        StartCoroutine(RunMatrix());
    }

    private IEnumerator RunMatrix()
    {
        yield return new WaitForSeconds(0.5f);

        bool exists = File.Exists(LogPath);
        using (StreamWriter w = new StreamWriter(LogPath, append: true))
        {
            if (!exists)
                w.WriteLine("width,height,cells,seed,seed_bytes,ascii_bytes," +
                            "packed_bytes,deflate_bytes,floor_cells");

            foreach (int size in gridSizes)
            {
                for (int i = 0; i < seedsPerSize; i++)
                {
                    int seed = firstSeed + (i * seedStride);

                    walkGenerator.gridWidth  = size;
                    walkGenerator.gridHeight = size;
                    walkGenerator.GenerateForEvaluation(seed);

                    string ascii = ReproducibilityLogger.Serialise(tileGrid, size, size);

                    int cells        = size * size;
                    int seedBytes    = sizeof(int);
                    int asciiBytes   = Encoding.UTF8.GetByteCount(ascii);
                    byte[] packed    = PackTwoBitsPerCell(tileGrid, size, size);
                    int deflateBytes = DeflateSize(packed);
                    int floorCells   = ReproducibilityLogger.CountFloor(ascii);

                    w.WriteLine($"{size},{size},{cells},{seed},{seedBytes}," +
                                $"{asciiBytes},{packed.Length},{deflateBytes},{floorCells}");
                }

                Debug.Log($"[NetCost] {size}x{size} complete");
                yield return null;
            }
        }

        Debug.Log("[NetCost] Matrix complete.");
    }

    /// <summary>
    /// Three tile states fit in two bits, so four cells pack per byte.
    /// This is the fair comparison — nobody transmits ASCII over a network.
    /// </summary>
    private byte[] PackTwoBitsPerCell(TileGrid grid, int width, int height)
    {
        int cells = width * height;
        byte[] buffer = new byte[(cells + 3) / 4];

        int index = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte code;
                switch (grid.GetTile(x, y))
                {
                    case TileType.Floor: code = 1; break;
                    case TileType.Wall:  code = 2; break;
                    default:             code = 0; break;
                }

                int byteIndex = index / 4;
                int shift = (index % 4) * 2;
                buffer[byteIndex] |= (byte)(code << shift);
                index++;
            }
        }

        return buffer;
    }

    private int DeflateSize(byte[] data)
    {
        using (MemoryStream output = new MemoryStream())
        {
            using (DeflateStream deflate =
                   new DeflateStream(output, CompressionLevel.Optimal, leaveOpen: true))
            {
                deflate.Write(data, 0, data.Length);
            }
            return (int)output.Length;
        }
    }
}