using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls visibility of Start, Pause, and End menu panels.
/// Listens to GameManager state changes and handles keyboard shortcuts.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject startMenuPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject endMenuPanel;

    [Header("Start Menu")]
    [SerializeField] private TextMeshProUGUI highScoresText;
    [SerializeField] private TextMeshProUGUI controlsText;

    [Header("End Menu")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI resultText;

    private const string ControlsContent =
        "Move:  WASD / Arrow Keys\n" +
        "Select Attack:  Mouse Click\n" +
        "Pause:  Escape\n" +
        "Reset:  R";

    private void Start()
    {
        if (controlsText != null)
            controlsText.text = ControlsContent;

        if (GameManager.Instance == null)
            return;

        // Subscribe here — Start is guaranteed to run after every Awake in the scene,
        // so GameManager.Instance is always set. OnEnable fires too early to be safe.
        GameManager.Instance.OnStateChanged += HandleStateChanged;

        // Sync panels with whatever state the GameManager is already in.
        HandleStateChanged(GameManager.Instance.CurrentState);
    }   

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void Update()
    {
        if (GameManager.Instance == null)
            return;

        // Escape — toggle pause while playing
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
                GameManager.Instance.PauseGame();
            else if (GameManager.Instance.CurrentState == GameState.Paused)
                GameManager.Instance.ResumeGame();
        }

        // R — reset from anywhere except main menu
        if (Input.GetKeyDown(KeyCode.R) && GameManager.Instance.CurrentState != GameState.MainMenu)
            GameManager.Instance.ResetGame();
    }

    // ── State handling ────────────────────────────────────────────────────────

    private void HandleStateChanged(GameState state)
    {
        SetPanelVisible(startMenuPanel, state == GameState.MainMenu);
        SetPanelVisible(pauseMenuPanel, state == GameState.Paused);
        SetPanelVisible(endMenuPanel,   state == GameState.GameOver);

        if (state == GameState.GameOver)
            PopulateEndMenu();

        if (state == GameState.MainMenu)
            RefreshHighScores();
    }

    // ── Button callbacks ──────────────────────────────────────────────────────

    /// <summary>Called by Start button.</summary>
    public void OnStartPressed()     => GameManager.Instance.StartGame();

    /// <summary>Called by Resume button.</summary>
    public void OnResumePressed()    => GameManager.Instance.ResumeGame();

    /// <summary>Called by Reset / Retry buttons.</summary>
    public void OnResetPressed()     => GameManager.Instance.ResetGame();

    /// <summary>Called by any Quit button.</summary>
    public void OnQuitPressed()      => GameManager.Instance.QuitGame();

    // ── Internal ──────────────────────────────────────────────────────────────

    private void PopulateEndMenu()
    {
        if (GameManager.Instance == null)
            return;

        if (finalScoreText != null)
            finalScoreText.text = $"Score\n{GameManager.Instance.CurrentScore}";

        if (resultText != null)
            resultText.text = GameManager.Instance.PlayerSurvived ? "You Survived!" : "You Were Eaten...";
    }

    private void RefreshHighScores()
    {
        if (highScoresText == null || HighScoreManager.Instance == null)
            return;

        List<HighScoreEntry> scores = HighScoreManager.Instance.GetTopScores(5);

        if (scores.Count == 0)
        {
            highScoresText.text = "No scores yet.";
            return;
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("── High Scores ──");
        for (int i = 0; i < scores.Count; i++)
            sb.AppendLine($"{i + 1}.  {scores[i].score,6}   {scores[i].date}");

        highScoresText.text = sb.ToString();
    }

    private static void SetPanelVisible(GameObject panel, bool visible)
    {
        if (panel != null)
            panel.SetActive(visible);
    }
}
