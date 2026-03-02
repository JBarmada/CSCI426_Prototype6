using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Place on (or under) the player HP bar GameObject in the UI.
/// Assign a UI Image set to Image Type = Filled / Horizontal for the fill,
/// and an optional TextMeshProUGUI for the "85 / 100" label.
/// Visibility is tied to GameState — visible only during Playing.
/// </summary>
public class PlayerHealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI hpText;

    [SerializeField] private Color fullColor  = Color.green;
    [SerializeField] private Color emptyColor = Color.red;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(PlayerHealth.Instance.CurrentHealth, PlayerHealth.Instance.MaxHealth);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            HandleStateChanged(GameManager.Instance.CurrentState);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.OnHealthChanged -= HandleHealthChanged;

        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void HandleStateChanged(GameState state)
    {
        gameObject.SetActive(state == GameState.Playing);
    }

    private void HandleHealthChanged(int current, int max)
    {
        float percent = max > 0 ? (float)current / max : 0f;

        if (fillImage != null)
        {
            fillImage.fillAmount = percent;
            fillImage.color = Color.Lerp(emptyColor, fullColor, percent);
        }

        if (hpText != null)
            hpText.text = $"{current} / {max}";
    }
}
