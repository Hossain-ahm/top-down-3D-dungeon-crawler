using UnityEngine;

public class DoorInteractable : MonoBehaviour
{
    public bool isEntrance;
    public LevelManager levelManager;

    public void Interact()
    {
        if (isEntrance) return;
        levelManager.LoadNextLevel();
    }
}