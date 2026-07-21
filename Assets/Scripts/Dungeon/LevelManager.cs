using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private int currentLevel = 0;
    public RandomWalkGenerator walkGenerator;

    public void LoadNextLevel()
    {
        currentLevel++;

        if (currentLevel >= 4)
        {
            Debug.Log("Boss Level — TODO: load boss scene");
        }
        else
        {
            walkGenerator.ClearDungeon();
            walkGenerator.Generate(walkGenerator.seed * (currentLevel + 1));
        }
    }
}