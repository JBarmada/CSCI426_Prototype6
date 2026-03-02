using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { MainMenu, Playing, Paused, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 60f;

    public GameState CurrentState { get; private set; } = GameState.MainMenu;
    public float TimeRemaining { get; private set; }
    public int CurrentScore { get; private set; }

    /// <summary>True when the round ended naturally (timer expired). False when the player died.</summary>
    public bool PlayerSurvived { get; private set; }

    public event Action<GameState> OnStateChanged;
    public event Action<float> OnTimerTick;
    public event Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetState(GameState.MainMenu);
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing)
            return;

        TimeRemaining -= Time.deltaTime;
        OnTimerTick?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            PlayerSurvived = true;
            EndGame();
        }
    }

    // ── State transitions ─────────────────────────────────────────────────────

    /// <summary>Transitions from MainMenu to Playing. Resets timer and score.</summary>
    public void StartGame()
    {
        CurrentScore = 0;
        TimeRemaining = gameDuration;
        PlayerSurvived = false;
        OnScoreChanged?.Invoke(CurrentScore);
        OnTimerTick?.Invoke(TimeRemaining);
        SetState(GameState.Playing);
    }

    /// <summary>Pauses the game.</summary>
    public void PauseGame()
    {
        if (CurrentState != GameState.Playing)
            return;

        SetState(GameState.Paused);
    }

    /// <summary>Resumes a paused game.</summary>
    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused)
            return;

        SetState(GameState.Playing);
    }

    /// <summary>Ends the round, saves the score, and transitions to GameOver. World freezes.</summary>
    public void EndGame()
    {
        if (CurrentState == GameState.GameOver)
            return;

        HighScoreManager.Instance.SaveScore(CurrentScore);
        SetState(GameState.GameOver);
    }

    /// <summary>Called when the player dies before the timer expires.</summary>
    public void NotifyPlayerDied()
    {
        PlayerSurvived = false;
        EndGame();
    }

    /// <summary>Reloads the active scene and returns to the frozen main menu.</summary>
    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        SetState(GameState.MainMenu);
    }

    /// <summary>Quits the application.</summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Score ─────────────────────────────────────────────────────────────────

    /// <summary>Adds points to the current score. Call when the player eats meat.</summary>
    public void AddScore(int amount)
    {
        if (CurrentState != GameState.Playing)
            return;

        CurrentScore += amount;
        OnScoreChanged?.Invoke(CurrentScore);
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void SetState(GameState newState)
    {
        CurrentState = newState;
        // Playing is the only state where time runs. Everything else — main menu,
        // pause, and game over — freezes the world.
        Time.timeScale = (newState == GameState.Playing) ? 1f : 0f;
        OnStateChanged?.Invoke(newState);
    }
}
