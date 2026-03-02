using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls visibility of Start, Pause, and End menu panels.
/// All UI references are re-discovered after every scene load so that
/// DontDestroyOnLoad cross-scene reference staleness is never an issue.
/// Panels are found via Transform.Find on UICanvas (works for inactive objects).
/// Buttons are wired with AddListener in code — not via serialized persistent calls.
/// </summary>
public class MenuManager : MonoBehaviour
{
    // Re-discovered after every scene load — never serialized.
    private GameObject startMenuPanel;
    private GameObject pauseMenuPanel;
    private GameObject endMenuPanel;
    private TextMeshProUGUI highScoresText;
    private TextMeshProUGUI controlsText;
    private TextMeshProUGUI finalScoreText;
    private TextMeshProUGUI resultText;

    private const string ControlsContent =
        "Move:  WASD / Arrow Keys\n" +
        "Select Attack:  Mouse Click\n" +
        "Pause:  Escape\n" +
        "Reset:  R";

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Initial scene load: sceneLoaded doesn't fire for the first load,
        // so we connect manually here.
        FindAndConnectUI();

        if (GameManager.Instance == null)
            return;

        GameManager.Instance.OnStateChanged += HandleStateChanged;
        HandleStateChanged(GameManager.Instance.CurrentState);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    // ── Scene reload ──────────────────────────────────────────────────────────

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // On scene reload, Unity creates a new Managers object from the scene file.
        // GameManager.Awake immediately destroys it (singleton guard), but before that
        // happens its MenuManager.Awake also subscribes to sceneLoaded, so this fires
        // twice. Skip the call from the duplicate — only the persistent instance acts.
        if (GameManager.Instance == null || gameObject != GameManager.Instance.gameObject)
            return;

        FindAndConnectUI();

        HandleStateChanged(GameManager.Instance.CurrentState);
    }

    /// <summary>
    /// Locates UICanvas in the active scene, then uses Transform.Find to reach
    /// all panels — including inactive ones that GameObject.Find would miss.
    /// Buttons are wired with AddListener so no serialized persistent call is needed.
    /// </summary>
    private void FindAndConnectUI()
    {
        GameObject canvas = GameObject.Find("UICanvas");
        if (canvas == null)
        {
            Debug.LogWarning("[MenuManager] UICanvas not found in scene.");
            return;
        }

        Transform ct = canvas.transform;
        startMenuPanel = ct.Find("StartMenuPanel")?.gameObject;
        pauseMenuPanel = ct.Find("PauseMenuPanel")?.gameObject;
        endMenuPanel   = ct.Find("EndMenuPanel")?.gameObject;

        if (startMenuPanel != null)
        {
            highScoresText = FindText(startMenuPanel, "HighScoresText");
            controlsText   = FindText(startMenuPanel, "ControlsText");
            if (controlsText != null) controlsText.text = ControlsContent;
            RefreshHighScores();
            WireButton(startMenuPanel, "StartButton", OnStartPressed);
            WireButton(startMenuPanel, "QuitButton",  OnQuitPressed);
        }

        if (pauseMenuPanel != null)
        {
            WireButton(pauseMenuPanel, "ResumeButton", OnResumePressed);
            WireButton(pauseMenuPanel, "ResetButton",  OnResetPressed);
            WireButton(pauseMenuPanel, "QuitButton",   OnQuitPressed);
        }

        if (endMenuPanel != null)
        {
            finalScoreText = FindText(endMenuPanel, "FinalScoreText");
            resultText     = FindText(endMenuPanel, "ResultText");
            WireButton(endMenuPanel, "RetryButton", OnResetPressed);
            WireButton(endMenuPanel, "QuitButton",  OnQuitPressed);
        }
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

    private static TextMeshProUGUI FindText(GameObject parent, string childName)
    {
        Transform child = parent.transform.Find(childName);
        return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
    }

    private static void WireButton(GameObject parent, string childName, UnityAction action)
    {
        Transform child = parent.transform.Find(childName);
        if (child == null) return;
        Button btn = child.GetComponent<Button>();
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    private static void SetPanelVisible(GameObject panel, bool visible)
    {
        if (panel != null)
            panel.SetActive(visible);
    }
}
