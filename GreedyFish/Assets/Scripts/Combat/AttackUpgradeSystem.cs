using System;
using UnityEngine;

/// <summary>
/// Scene-local singleton on the Player GameObject.
/// Listens for meat collection and upgrades the corresponding AttackData inside AttackSystem.
/// Resets each round (no DontDestroyOnLoad).
/// </summary>
public class AttackUpgradeSystem : MonoBehaviour
{
    public static AttackUpgradeSystem Instance { get; private set; }

    // ── Events ────────────────────────────────────────────────────────────────

    public event Action<AttackType, int, int> OnMeatChanged;   // (type, newCount, currentThreshold)
    public event Action<AttackType, int> OnLevelUp;            // (type, newLevel)
    public event Action<AttackType> OnSpecialUnlocked;         // (type)

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Call when a meat piece of the given type is collected.
    /// Adds <paramref name="amount"/> to the corresponding AttackData and checks level thresholds.
    /// </summary>
    public void RegisterMeat(AttackType type, int amount)
    {
        if (AttackSystem.Instance == null) return;

        AttackData data = AttackSystem.Instance.GetAttack(type);
        if (data == null) return;

        data.meatCollected += amount;

        int threshold = GetCurrentThreshold(data);
        OnMeatChanged?.Invoke(type, data.meatCollected, threshold);

        CheckLevelThresholds(data);
    }

    /// <summary>Returns the total meat collected for the given attack type.</summary>
    public int GetMeatCount(AttackType type)
    {
        if (AttackSystem.Instance == null) return 0;
        AttackData data = AttackSystem.Instance.GetAttack(type);
        return data?.meatCollected ?? 0;
    }

    /// <summary>Returns the current level (1–3) for the given attack type.</summary>
    public int GetLevel(AttackType type)
    {
        if (AttackSystem.Instance == null) return 1;
        AttackData data = AttackSystem.Instance.GetAttack(type);
        return data?.level ?? 1;
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void CheckLevelThresholds(AttackData data)
    {
        // Level 2: 15 meat → 1d6+1
        if (data.meatCollected >= AttackData.Level2Threshold && data.level < 2)
        {
            data.level = 2;
            data.diceCount = 1;
            data.flatBonus = 1;
            OnLevelUp?.Invoke(data.type, data.level);
        }

        // Level 3: 30 meat → 2d6+1
        if (data.meatCollected >= AttackData.Level3Threshold && data.level < 3)
        {
            data.level = 3;
            data.diceCount = 2;
            data.flatBonus = 1;
            OnLevelUp?.Invoke(data.type, data.level);
        }

        // Special: 45 meat → 2d6+2, special unlocked
        if (data.meatCollected >= AttackData.SpecialThreshold && !data.specialUnlocked)
        {
            data.flatBonus = 2;
            data.specialUnlocked = true;
            OnSpecialUnlocked?.Invoke(data.type);
        }
    }

    private static int GetCurrentThreshold(AttackData data)
    {
        if (data.meatCollected < AttackData.Level2Threshold)
            return AttackData.Level2Threshold;
        if (data.meatCollected < AttackData.Level3Threshold)
            return AttackData.Level3Threshold;
        if (data.meatCollected < AttackData.SpecialThreshold)
            return AttackData.SpecialThreshold;

        return AttackData.SpecialThreshold; // maxed
    }
}
