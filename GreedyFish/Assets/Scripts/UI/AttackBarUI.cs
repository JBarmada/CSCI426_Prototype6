using UnityEngine;

/// <summary>
/// Root controller for the bottom AttackBarPanel.
/// Finds and wires the three sub-section components, subscribes to AttackSystem
/// and AttackUpgradeSystem events, and shows/hides the bar with game state.
/// </summary>
public class AttackBarUI : MonoBehaviour
{
    private AttackSlotUI[] _slots;
    private DiceSectionUI _diceSection;
    private ModifierSectionUI _modSection;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        FindSections();
        SubscribeToEvents();

        // Sync visibility immediately based on the current game state.
        // This hides the bar at MainMenu and shows it if somehow entering Playing first.
        if (GameManager.Instance != null)
            HandleStateChanged(GameManager.Instance.CurrentState);
        else
            gameObject.SetActive(false);

        if (AttackSystem.Instance?.SelectedAttack != null)
            HandleAttackSelected(AttackSystem.Instance.SelectedAttack.type);
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    // ── Event subscription ────────────────────────────────────────────────────

    private void SubscribeToEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            GameManager.Instance.OnBuffedAttackChosen += HandleBuffedAttackChosen;
        }

        if (AttackSystem.Instance != null)
        {
            AttackSystem.Instance.OnAttackSelected += HandleAttackSelected;
            AttackSystem.Instance.OnAttackRolled   += HandleAttackRolled;
            AttackSystem.Instance.OnCooldownChanged += HandleCooldownChanged;
        }

        if (AttackUpgradeSystem.Instance != null)
        {
            AttackUpgradeSystem.Instance.OnMeatChanged      += HandleMeatChanged;
            AttackUpgradeSystem.Instance.OnLevelUp          += HandleLevelUp;
            AttackUpgradeSystem.Instance.OnSpecialUnlocked  += HandleSpecialUnlocked;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
            GameManager.Instance.OnBuffedAttackChosen -= HandleBuffedAttackChosen;
        }

        if (AttackSystem.Instance != null)
        {
            AttackSystem.Instance.OnAttackSelected -= HandleAttackSelected;
            AttackSystem.Instance.OnAttackRolled   -= HandleAttackRolled;
            AttackSystem.Instance.OnCooldownChanged -= HandleCooldownChanged;
        }

        if (AttackUpgradeSystem.Instance != null)
        {
            AttackUpgradeSystem.Instance.OnMeatChanged      -= HandleMeatChanged;
            AttackUpgradeSystem.Instance.OnLevelUp          -= HandleLevelUp;
            AttackUpgradeSystem.Instance.OnSpecialUnlocked  -= HandleSpecialUnlocked;
        }
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void HandleStateChanged(GameState state)
    {
        gameObject.SetActive(state == GameState.Playing);
    }

    private void HandleBuffedAttackChosen(AttackType buffedType)
    {
        if (_slots == null) return;
        foreach (AttackSlotUI slot in _slots)
            slot.SetBuffed(slot.AttackType == buffedType);
    }

    private void HandleAttackSelected(AttackType type)
    {
        if (_slots == null) return;

        foreach (AttackSlotUI slot in _slots)
            slot.SetSelected(slot.AttackType == type);

        RefreshModifierSection();
    }

    private void HandleAttackRolled(int total, int[] dice)
    {
        if (_diceSection == null || AttackSystem.Instance?.SelectedAttack == null) return;
        _diceSection.ShowResult(total, dice, AttackSystem.Instance.SelectedAttack.flatBonus);
    }

    private void HandleCooldownChanged(AttackType type, float remaining)
    {
        if (_modSection == null || AttackSystem.Instance?.SelectedAttack == null) return;
        if (AttackSystem.Instance.SelectedAttack.type != type) return;

        AttackData data = AttackSystem.Instance.SelectedAttack;
        string formula = GetSpecialDiceFormula(data);
        _modSection.SetSpecialMove(data.specialMoveName, remaining, data.specialMoveCooldown, data.specialUnlocked, formula);
    }

    private void HandleMeatChanged(AttackType type, int newCount, int threshold)
    {
        if (_modSection == null || AttackSystem.Instance?.SelectedAttack == null) return;
        if (AttackSystem.Instance.SelectedAttack.type != type) return;

        bool maxed = newCount >= AttackData.SpecialThreshold;
        float progress = maxed ? 1f : (float)newCount / threshold;
        _modSection.SetProgress(progress, newCount, threshold, maxed);
    }

    private void HandleLevelUp(AttackType type, int newLevel)
    {
        if (_slots != null)
        {
            foreach (AttackSlotUI slot in _slots)
            {
                if (slot.AttackType == type)
                    slot.RefreshLevel(newLevel);
            }
        }

        if (_modSection != null && AttackSystem.Instance != null)
        {
            AttackData data = AttackSystem.Instance.GetAttack(type);
            if (data != null && AttackSystem.Instance.SelectedAttack?.type == type)
            {
                int displayBonus = data.flatBonus + AttackSystem.Instance.GetBuffDamageBonus(type);
                _modSection.SetFormula(data.diceCount, data.diceSides, displayBonus);
            }
        }
    }

    private void HandleSpecialUnlocked(AttackType type)
    {
        if (_modSection == null || AttackSystem.Instance?.SelectedAttack?.type != type) return;

        AttackData data = AttackSystem.Instance.GetAttack(type);
        if (data != null)
        {
            string formula = GetSpecialDiceFormula(data);
            _modSection.SetSpecialMove(data.specialMoveName, data.specialCooldownRemaining, data.specialMoveCooldown, true, formula);
        }
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void FindSections()
    {
        _slots       = GetComponentsInChildren<AttackSlotUI>(true);
        _diceSection = GetComponentInChildren<DiceSectionUI>(true);
        _modSection  = GetComponentInChildren<ModifierSectionUI>(true);
    }

    private void RefreshModifierSection()
    {
        if (_modSection == null || AttackSystem.Instance?.SelectedAttack == null) return;

        AttackData data = AttackSystem.Instance.SelectedAttack;
        bool maxed = data.meatCollected >= AttackData.SpecialThreshold;
        int threshold = maxed ? AttackData.SpecialThreshold : GetNextThreshold(data);
        float progress = maxed ? 1f : (float)data.meatCollected / threshold;

        _modSection.SetProgress(progress, data.meatCollected, threshold, maxed);
        int displayBonus = data.flatBonus + AttackSystem.Instance.GetBuffDamageBonus(data.type);
        _modSection.SetFormula(data.diceCount, data.diceSides, displayBonus);
        string specialFormula = GetSpecialDiceFormula(data);
        _modSection.SetSpecialMove(data.specialMoveName, data.specialCooldownRemaining, data.specialMoveCooldown, data.specialUnlocked, specialFormula);
    }

    private static int GetNextThreshold(AttackData data)
    {
        if (data.meatCollected < AttackData.Level2Threshold) return AttackData.Level2Threshold;
        if (data.meatCollected < AttackData.Level3Threshold) return AttackData.Level3Threshold;
        return AttackData.SpecialThreshold;
    }

    private static string GetSpecialDiceFormula(AttackData data)
    {
        int sides = data.specialDiceSides > 0 ? data.specialDiceSides : data.diceSides;
        int buffBonus = AttackSystem.Instance != null ? AttackSystem.Instance.GetBuffDamageBonus(data.type) : 0;
        return $"{data.diceCount}d{sides}+{data.flatBonus + buffBonus}";
    }
}
