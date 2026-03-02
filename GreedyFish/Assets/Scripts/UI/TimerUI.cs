using UnityEngine;
using TMPro;

/// <summary>
/// Displays the game timer on screen.
/// Attach this to a TextMeshProUGUI element in your Canvas.
/// </summary>
public class TimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    private bool _isSubscribed;

    private void Awake()
    {
        if (timerText == null)
            timerText = GetComponent<TextMeshProUGUI>();

        SetTimerText(0f);
    }

    private void OnEnable()
    {
        TrySubscribe();

        if (GameManager.Instance != null)
            SetTimerText(GameManager.Instance.TimeRemaining);
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private void Update()
    {
        // Handles cases where this UI is enabled before GameManager.Instance exists.
        if (!_isSubscribed)
            TrySubscribe();
    }

    private void TrySubscribe()
    {
        if (_isSubscribed || GameManager.Instance == null)
            return;

        GameManager.Instance.OnTimerTick += UpdateTimer;
        _isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!_isSubscribed || GameManager.Instance == null)
            return;

        GameManager.Instance.OnTimerTick -= UpdateTimer;
        _isSubscribed = false;
    }

    private void UpdateTimer(float timeRemaining)
    {
        SetTimerText(timeRemaining);
    }

    private void SetTimerText(float timeRemaining)
    {
        if (timerText == null)
            return;

        float clampedTime = Mathf.Max(0f, timeRemaining);
        int minutes = Mathf.FloorToInt(clampedTime / 60f);
        int seconds = Mathf.FloorToInt(clampedTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
