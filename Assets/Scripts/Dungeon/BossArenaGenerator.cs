using UnityEngine;
using System.Collections;
public class BossArenaGenerator : MonoBehaviour
{
    [Header("References")]
    public TileGrid tileGrid;
    public RandomWalkGenerator walkGenerator;
    public GameObject player;

    [Header("Prefabs")]
    public GameObject bossPrefab;
    public GameObject rangedPrefab;

    [Header("Grid & Arena Settings")]
    public int gridWidth = 80;
    public int gridHeight = 80;
    public float arenaRadius = 15f;
    
    [Header("UI")]
    public BossHealthUI bossHealthUI;

    /// <summary>
    /// Clears existing level, carves a circular boss arena, and spawns the player, boss, and minions.
    /// </summary>
    
    private GameObject _bossInstance;

    private void SpawnBoss(float centreX, float centreY)
    {
        if (bossPrefab == null) return;

        Vector3 bossPos = new Vector3(centreX, 3.5f, centreY);
        _bossInstance = Instantiate(bossPrefab, bossPos, Quaternion.identity);
        _bossInstance.tag = "Enemy";

        BossEnemy boss = _bossInstance.GetComponent<BossEnemy>();
        if (boss != null)
        {
            boss.SetArenaBounds(new Vector3(centreX, 3.5f, centreY), arenaRadius);

            if (bossHealthUI != null)
                bossHealthUI.SetBoss(boss);
        }
    }

    public GameObject GetBoss() => _bossInstance;
    
    public void Generate()
    {
        // Step 3a: Clear previous level objects
        if (walkGenerator != null)
        {
            walkGenerator.ClearDungeon();
        }

        // Step 3b: Initialise tile grid bounds
        if (tileGrid != null)
        {
            tileGrid.Initialise(gridWidth, gridHeight);
        }

        // Calculate arena centre coordinates
        float centreX = gridWidth / 2f;
        float centreY = gridHeight / 2f;
        Vector2 arenaCentre = new Vector2(centreX, centreY);

        // Step 2: Carve the circular arena floor tiles
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), arenaCentre);
                if (distance <= arenaRadius)
                {
                    tileGrid.SetTile(x, y, TileType.Floor);
                }
            }
        }

        // Step 3c: Instantiate tile meshes & boundary walls
        if (tileGrid != null)
        {
            tileGrid.SpawnTiles();
            StartCoroutine(SpawnEntitiesAfterDelay(centreX, centreY));
        }
    }
    
    
    private IEnumerator SpawnEntitiesAfterDelay(float centreX, float centreY)
    {
        yield return new WaitForFixedUpdate();
        SpawnPlayer(centreX, centreY);
        SpawnBoss(centreX, centreY);
        SpawnMinions(centreX, centreY);
    }

    private void SpawnPlayer(float centreX, float centreY)
    {
        if (player == null) return;

        // Place player near the south edge (2 tiles inside the south boundary wall)
        Vector3 playerPos = new Vector3(centreX, 1.5f, centreY - arenaRadius + 2f);

        // Safely move physics object without getting stuck in geometry
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            player.transform.position = playerPos;
            Physics.SyncTransforms();
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
        }
        else
        {
            player.transform.position = playerPos;
        }
    }
    

    private void SpawnMinions(float centreX, float centreY)
    {
        if (rangedPrefab == null) return;

        float offset = arenaRadius / 2f;

        // Ranged Minion 1: East side
        Vector3 eastMinionPos = new Vector3(centreX + offset, 1.5f, centreY);
        GameObject minionEast = Instantiate(rangedPrefab, eastMinionPos, Quaternion.identity);
        minionEast.tag = "Enemy";

        // Ranged Minion 2: West side
        Vector3 westMinionPos = new Vector3(centreX - offset, 1.5f, centreY);
        GameObject minionWest = Instantiate(rangedPrefab, westMinionPos, Quaternion.identity);
        minionWest.tag = "Enemy";
    }
}