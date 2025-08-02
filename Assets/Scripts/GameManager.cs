using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool IsGameRunning { get; private set; } = false;

    [Header("References")]
    [SerializeField] private GameTimer gameTimer;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    private void Start()
    {
         StartGame();
    }

    public void StartGame()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            return;
        }
        IsGameRunning = true;
        gameTimer.StartTimer();

        // TODO: Reset score, spawn player, etc.
        Debug.Log("Game Started");
    }

    public void EndGame()
    {
        IsGameRunning = false;
        gameTimer.StopTimer();
        gameTimer.ResetTimer();

        // TODO: Show game over UI, final time, etc.
        Debug.Log("Game Ended");
    }

    public void RestartGame()
    {
        EndGame();
        StartGame();
    }

    public void LoadLevel(string LevelName)
    {
        SceneManager.LoadScene(LevelName);
    }

    public void LoadNextLevel()
    {
        print("HERE!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
