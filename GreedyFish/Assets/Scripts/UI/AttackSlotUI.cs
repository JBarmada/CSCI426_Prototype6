using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attached to each of the four attack slot GameObjects.
/// Displays the attack name, level, and selection highlight.
/// Calls AttackSystem.SelectAttack on button click.
/// </summary>
public class AttackSlotUI : MonoBehaviour
{
    [SerializeField] private AttackType attackType;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;       // "LVL. 1"
    [SerializeField] private Image selectionBorder;           // bright colored outline image
    [SerializeField] private Color selectedColor   = Color.yellow;
    [SerializeField] private Color deselectedColor = new Color(1f, 1f, 1f, 0f);

    /// <summary>Exposes the configured attack type so AttackBarUI can route events correctly.</summary>
    public AttackType AttackType => attackType;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(() => AttackSystem.Instance?.SelectAttack(attackType));

        SetSelected(false);
        RefreshLevel(1);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Called by AttackBarUI to sync the highlight state of this slot.</summary>
    public void SetSelected(bool selected)
    {
        if (selectionBorder != null)
            selectionBorder.color = selected ? selectedColor : deselectedColor;
    }

    /// <summary>Called by AttackBarUI when this attack's level changes.</summary>
    public void RefreshLevel(int level)
    {
        if (levelText != null)
            levelText.text = $"LVL. {level}";
    }
}
