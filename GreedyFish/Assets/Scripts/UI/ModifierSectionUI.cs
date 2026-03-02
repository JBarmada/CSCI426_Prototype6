using TMPro;
using UnityEngine;

/// <summary>
/// Section 3 of the attack bar — level progress bar, XdY+Z formula text, and special move cooldown line.
/// </summary>
public class ModifierSectionUI : MonoBehaviour
{
    [SerializeField] private RectTransform progressFill;  // green inner fill bar
    [SerializeField] private TextMeshProUGUI formulaText;      // "2d6+1"
    [SerializeField] private TextMeshProUGUI specialMoveText;  // "Blood Frenzy (CD: 8s)"
    [SerializeField] private TextMeshProUGUI progressLabel;    // "XP: 12 / 15"

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        SetProgress(0f, 0, AttackData.Level2Threshold, false);
        SetFormula(1, 6, 0);
        SetSpecialMove("???", 0f, 0f, false);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Updates the progress bar fill and label.
    /// <paramref name="progress"/> must be in 0..1 range.
    /// </summary>
    public void SetProgress(float progress, int currentMeat, int threshold, bool maxed)
    {
        progress = Mathf.Clamp01(progress);

        if (progressFill != null)
        {
            Vector2 max = progressFill.anchorMax;
            max.x = progress;
            progressFill.anchorMax = max;
        }

        if (progressLabel != null)
        {
            progressLabel.text = maxed
                ? "MAX"
                : $"XP: {currentMeat} / {threshold}";
        }
    }

    /// <summary>Updates the XdY+Z formula display.</summary>
    public void SetFormula(int diceCount, int diceSides, int flatBonus)
    {
        if (formulaText != null)
            formulaText.text = $"{diceCount}d{diceSides}+{flatBonus}";
    }

    /// <summary>Updates the special move cooldown line.</summary>
    public void SetSpecialMove(string moveName, float cooldownRemaining, float cooldownMax, bool unlocked)
    {
        if (specialMoveText == null) return;

        if (!unlocked)
        {
            specialMoveText.text = "??? (Locked)";
            return;
        }

        if (cooldownRemaining > 0f)
        {
            int cdSeconds = Mathf.CeilToInt(cooldownRemaining);
            specialMoveText.text = $"{moveName} (CD: {cdSeconds}s)";
        }
        else
        {
            specialMoveText.text = $"{moveName} (Ready!)";
        }
    }
}
