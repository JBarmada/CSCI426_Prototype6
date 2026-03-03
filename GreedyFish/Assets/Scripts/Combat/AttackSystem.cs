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

        // Buffed attack gets +1 flat damage bonus
        int bonusDamage = 0;
        if (GameManager.Instance != null && SelectedAttack.type == GameManager.Instance.BuffedAttackType)
            bonusDamage = 1;

        int total = DiceRoller.Roll(
            SelectedAttack.diceCount,
            SelectedAttack.diceSides,
            SelectedAttack.flatBonus + bonusDamage,
            out int[] individuals
        );
        GameManager.Instance?.AddScore(total);
        OnAttackRolled?.Invoke(total, individuals);

        // Grant 1 XP per hit to the selected attack (2 XP if this attack is buffed)
        if (AttackUpgradeSystem.Instance != null)
        {
            bool isBuff = GameManager.Instance != null && SelectedAttack.type == GameManager.Instance.BuffedAttackType;
            AttackUpgradeSystem.Instance.RegisterMeat(SelectedAttack.type, isBuff ? 2 : 1, false);
        }

        return total;
    }

    /// <summary>Called when an enemy dies — rolls for the number of meat pieces to drop.</summary>
    public int RollMeatDrop()
    {
        return DiceRoller.Roll(meatDropDiceCount, meatDropDiceSides, meatDropFlatBonus, out _);
    }

    /// <summary>Public method to fire the OnAttackRolled event from external attack modules.</summary>
    public void FireAttackRolledEvent(int total, int[] individuals)
    {
        OnAttackRolled?.Invoke(total, individuals);
    }

    /// <summary>Returns +1 if the given attack type is this round's buffed type, 0 otherwise.
    /// Call this from custom roll methods so the buff damage bonus is always applied.</summary>
    public int GetBuffDamageBonus(AttackType type)
    {
        if (GameManager.Instance != null && type == GameManager.Instance.BuffedAttackType)
            return 1;
        return 0;
    }

    /// <summary>Grants XP to the given attack type for landing a hit.
    /// 1 XP normally, 2 XP if this attack is the round's buffed type.</summary>
    public void GrantHitXP(AttackType type)
    {
        if (AttackUpgradeSystem.Instance == null) return;
        bool isBuff = GameManager.Instance != null && type == GameManager.Instance.BuffedAttackType;
        AttackUpgradeSystem.Instance.RegisterMeat(type, isBuff ? 2 : 1, false);
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
