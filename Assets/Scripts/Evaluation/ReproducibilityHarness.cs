using UnityEngine;
using System.Collections;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

/// <summary>
/// Drives the run matrix: N seeds across the available generators,
/// logging each result. Attach to an empty GameObject in a scene
/// used only for evaluation.
/// </summary>
public class ReproducibilityHarness : MonoBehaviour
{
    [Header("Session")]
    [Tooltip("Set differently for each run — e.g. editor_1, build_1, build_2.")]
    public string sessionId = "session_1";

    [Header("Matrix")]
    public int seedCount = 20;
    public int firstSeed = 1000;
    public int seedStride = 137;

    [Header("Grid Sizes")]
    public int[] gridSizes = { 40, 60, 80 };

    [Header("Generators")]
    public RandomWalkGenerator walkGenerator;
    public DungeonGenerator roomGenerator;
    public BSPDungeonGenerator bspGenerator;

    [Header("Shared")]
    public TileGrid tileGrid;

    private void Start()
    {
        ReproducibilityLogger.ReportPath();
        StartCoroutine(RunMatrix());
    }

    private IEnumerator RunMatrix()
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < seedCount; i++)
        {
            int seed = firstSeed + (i * seedStride);

            foreach (int size in gridSizes)
            {
                if (walkGenerator != null)
                    RunOne("random_walk", seed, size, () =>
                    {
                        walkGenerator.gridWidth  = size;
                        walkGenerator.gridHeight = size;
                        walkGenerator.GenerateForEvaluation(seed);
                        return walkGenerator.LastAttemptCount;
                    });

                if (roomGenerator != null)
                    RunOne("room_corridor", seed, size, () =>
                    {
                        roomGenerator.gridWidth  = size;
                        roomGenerator.gridHeight = size;
                        roomGenerator.Generate(seed);
                        return 1;
                    });

                if (bspGenerator != null)
                    RunOne("bsp", seed, size, () =>
                    {
                        bspGenerator.gridWidth  = size;
                        bspGenerator.gridHeight = size;
                        bspGenerator.Generate(seed);
                        return 1;
                    });

                yield return null;
            }
        }

        Debug.Log($"[Repro] Matrix complete for session '{sessionId}'.");
    }

    private void RunOne(string algorithm, int seed, int size, System.Func<int> generate)
    {
        Stopwatch sw = Stopwatch.StartNew();
        int attempts = generate();
        sw.Stop();

        ReproducibilityLogger.Log(
            sessionId, algorithm, seed, size, size,
            attempts, tileGrid, sw.ElapsedMilliseconds);
    }
}