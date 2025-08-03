using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1.5f;
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool IsGameRunning { get; private set; } = false;

    [Header("References")]
    [SerializeField] private GameTimer gameTimer;
    
    private int herdableCounter = 0;

    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip[] levelMusic;
    [SerializeField] private bool[] levelMusicLoops; // MUST be same length as levelMusic

    [SerializeField] private bool mainMenuMusicLoop = true;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        PlayMusicForCurrentScene();
        StartGame();
    }

    public void StartGame()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            return;
        }
        IsGameRunning = true;
        if (gameTimer != null)
        {
            gameTimer.StartTimer();
        }

        // TODO: Reset score, spawn player, etc.
        Debug.Log("Game Started");
    }

    public void EndGame()
    {
        IsGameRunning = false;

        if (gameTimer != null)
        {
            gameTimer.StopTimer();
            gameTimer.ResetTimer();
        }

        // TODO: Show game over UI, final time, etc.
        Debug.Log("Game Ended");
    }

    public void RestartGame()
    {
        EndGame();
        StartGame();
    }
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void LoadNextLevel()
    {
        StartCoroutine(FadeAndLoadNext());
    }

    private IEnumerator FadeAndLoadNext()
    {
        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, 1f); // Ensure full black at end
        // Load next scene at the end
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    }

    public void RegisterHerdable()
    {
        herdableCounter++;
    }

    public void UnregisterHerdable()
    {
        herdableCounter--;
        if (herdableCounter <= 0)
        {
            // Win condition triggers fade and level load
            LoadNextLevel();
        }
    }

    private void PlayMusicForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "MainMenu")
        {
            AudioManager.Instance.PlayMusic(mainMenuMusic, mainMenuMusicLoop);
        }
        else
        {
            int levelIndex = SceneManager.GetActiveScene().buildIndex - 1; // MainMenu is buildIndex 0
            if (levelIndex >= 0 && levelIndex < levelMusic.Length)
            {
                bool loop = levelIndex < levelMusicLoops.Length ? levelMusicLoops[levelIndex] : true;
                AudioManager.Instance.PlayMusic(levelMusic[levelIndex], loop);
            }
        }
    }
}