using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    private int currentLevel = 0;
    public RandomWalkGenerator walkGenerator;
    public BossArenaGenerator bossArenaGenerator;

    private void Update()
    {
        // DEBUG — press B to jump straight to boss arena
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            Debug.Log("DEBUG: Jumping to boss arena");
            currentLevel = 4;
            if (bossArenaGenerator != null)
                bossArenaGenerator.Generate();
        }
    }

    public void LoadNextLevel()
    {
        currentLevel++;

        if (currentLevel >= 4)
        {
            if (bossArenaGenerator != null)
                bossArenaGenerator.Generate();
            else
                Debug.LogError("BossArenaGenerator not assigned!");
        }
        else
        {
            walkGenerator.ClearDungeon();
            walkGenerator.Generate(walkGenerator.seed * (currentLevel + 1));
        }
    }
}