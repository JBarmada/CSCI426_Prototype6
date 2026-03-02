using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the dice result panel (large number) and history log in section 2 of the attack bar.
/// </summary>
public class DiceSectionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultText;     // large number, default "--"
    [SerializeField] private TextMeshProUGUI breakdownText;  // "[4, 2] + 1"
    [SerializeField] private TextMeshProUGUI historyText;    // small scrolling log

    private const int MaxHistoryEntries = 5;

    private readonly Queue<string> _history = new Queue<string>();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        if (resultText    != null) resultText.text    = "--";
        if (breakdownText != null) breakdownText.text = "";
        if (historyText   != null) historyText.text   = "";
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Called by AttackBarUI when OnAttackRolled fires.</summary>
    public void ShowResult(int total, int[] dice, int bonus)
    {
        if (resultText != null)
            resultText.text = total.ToString();

        string breakdown = $"[{string.Join(", ", dice)}] +{bonus}";

        if (breakdownText != null)
            breakdownText.text = breakdown;

        // Update rolling history.
        string entry = $"{total} ({breakdown})";
        _history.Enqueue(entry);

        if (_history.Count > MaxHistoryEntries)
            _history.Dequeue();

        if (historyText != null)
            historyText.text = string.Join("\n", _history);
    }
}
