using System;

/// <summary>
/// Serializable data class holding one attack's full runtime state.
/// Owned and mutated by AttackSystem and AttackUpgradeSystem.
/// </summary>
[Serializable]
public class AttackData
{
    public AttackType type;
    public string displayName;           // e.g. "JawBite", "Piercing Stab"
    public string specialMoveName;       // e.g. "Jaw Storm", "Piercing Trident"
    public float specialMoveCooldown;    // seconds until special can be reused
    public int diceCount;               // X in XdY+Z, starts at 1
    public int diceSides;               // Y, always 6
    public int flatBonus;               // Z, starts at 0
    public int specialDiceSides;         // dice sides used by special attack (0 = use diceSides)
    public int level;                   // 1, 2, or 3
    public int meatCollected;           // running total for this attack type
    public bool specialUnlocked;
    public float specialCooldownRemaining; // countdown in seconds; 0 = ready

    // ── Level thresholds ─────────────────────────────────────────────────────

    public const int Level2Threshold   = 30;  // → 1d6+1
    public const int Level3Threshold   = 60;  // → 2d6+1
    public const int SpecialThreshold  = 90;  // → 2d6+2, special unlocked
}
