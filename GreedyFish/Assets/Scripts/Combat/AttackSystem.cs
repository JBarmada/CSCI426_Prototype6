using System;
using UnityEngine;

/// <summary>
/// Scene-local singleton on the Player GameObject.
/// Owns the four AttackData entries, handles selection, executes attacks on contact,
/// and ticks special-move cooldowns. Resets each round (no DontDestroyOnLoad).
/// </summary>
public class AttackSystem : MonoBehaviour
{
    public static AttackSystem Instance { get; private set; }

    [Header("Attacks")]
    [SerializeField] private AttackData[] attacks;   // 4 entries, configured in Inspector

    [Header("Meat Drop Dice")]
    [SerializeField] private int meatDropDiceCount = 1;
    [SerializeField] private int meatDropDiceSides = 3;
    [SerializeField] private int meatDropFlatBonus = 1;

    public AttackData SelectedAttack { get; private set; }

    // ── Events ────────────────────────────────────────────────────────────────

    public event Action<AttackType> OnAttackSelected;
    public event Action<int, int[]> OnAttackRolled;    // (total, individualDice)
    public event Action<AttackType, float> OnCooldownChanged; // (type, remaining)

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        if (attacks != null && attacks.Length > 0)
            SelectedAttack = attacks[0];
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        TickCooldowns();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Returns the AttackData for the given type, or null if not found.</summary>
    public AttackData GetAttack(AttackType type)
    {
        if (attacks == null) return null;

        foreach (AttackData data in attacks)
        {
            if (data.type == type)
                return data;
        }

        return null;
    }

    /// <summary>Selects the attack of the given type and fires OnAttackSelected.</summary>
    public void SelectAttack(AttackType type)
    {
        AttackData data = GetAttack(type);
        if (data == null) return;

        SelectedAttack = data;
        OnAttackSelected?.Invoke(type);
    }

    /// <summary>
    /// Called by PlayerScript.OnCollisionEnter2D when touching an enemy.
    /// Rolls dice using the selected attack, fires events, and returns damage dealt.
    /// </summary>
    public int ExecuteAttack()
    {
        if (SelectedAttack == null) return 0;

        int total = DiceRoller.Roll(
            SelectedAttack.diceCount,
            SelectedAttack.diceSides,
            SelectedAttack.flatBonus,
            out int[] individuals
        );

        OnAttackRolled?.Invoke(total, individuals);
        return total;
    }

    /// <summary>Called when an enemy dies — rolls for the number of meat pieces to drop.</summary>
    public int RollMeatDrop()
    {
        return DiceRoller.Roll(meatDropDiceCount, meatDropDiceSides, meatDropFlatBonus, out _);
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void TickCooldowns()
    {
        if (attacks == null) return;

        foreach (AttackData data in attacks)
        {
            if (data.specialCooldownRemaining > 0f)
            {
                data.specialCooldownRemaining -= Time.deltaTime;

                if (data.specialCooldownRemaining < 0f)
                    data.specialCooldownRemaining = 0f;

                OnCooldownChanged?.Invoke(data.type, data.specialCooldownRemaining);
            }
        }
    }
}
