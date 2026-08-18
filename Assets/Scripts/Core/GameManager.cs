using UnityEngine;

public enum GameState { StartScreen, Playing, GameOver, Victory }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Screens")]
    public GameObject startScreen;
    public GameObject gameOverScreen;
    public GameObject victoryScreen;
    public GameObject hudRoot;

    [Header("References")]
    public RandomWalkGenerator walkGenerator;
    public LevelManager levelManager;
    public GameObject player;

    public GameState CurrentState { get; private set; } = GameState.StartScreen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        ShowStartScreen();
    }

    // ─── State changes ──────────────────────────────────────────────────────

    public void ShowStartScreen()
    {
        CurrentState = GameState.StartScreen;
        Time.timeScale = 0f;

        SetActive(startScreen, true);
        SetActive(gameOverScreen, false);
        SetActive(victoryScreen, false);
        SetActive(hudRoot, false);

        if (player != null)
            player.SetActive(false);
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;

        SetActive(startScreen, false);
        SetActive(gameOverScreen, false);
        SetActive(victoryScreen, false);
        SetActive(hudRoot, true);

        if (player != null)
            player.SetActive(true);

        if (walkGenerator != null)
            walkGenerator.Generate(walkGenerator.seed);
    }

    public void GameOver()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;

        SetActive(gameOverScreen, true);
        SetActive(hudRoot, false);
    }

    public void Victory()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Victory;
        Time.timeScale = 0f;

        SetActive(victoryScreen, true);
        SetActive(hudRoot, false);
    }

    // ─── Button hooks ───────────────────────────────────────────────────────

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void SetActive(GameObject obj, bool state)
    {
        if (obj != null) obj.SetActive(state);
    }
}