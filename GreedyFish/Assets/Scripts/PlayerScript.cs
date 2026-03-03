using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [Header("Attack Modules")]
    [SerializeField] private BiteAttackModule biteAttack;
    [SerializeField] private StabAttackModule stabAttack;
    [SerializeField] private ZapAttackModule zapAttack;
    [SerializeField] private PoisonAttackModule poisonAttack;

    private readonly Dictionary<AttackType, PlayerAttackModuleBase> attackModules =
        new Dictionary<AttackType, PlayerAttackModuleBase>();

    private void Awake()
    {
        EnsureModulesExist();
        RebuildModuleLookup();
    }

    private void Update()
    {
        if (GameManager.Instance?.CurrentState != GameState.Playing) return;

        // Space bar fires the special attack of the currently selected type.
        if (Input.GetKeyDown(KeyCode.Space))
            TryFireSpecial();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance?.CurrentState != GameState.Playing) return;

        GameObject other = collision.gameObject;

        AttackType? enemyAttackType = GetEnemyAttackType(other.tag);
        if (enemyAttackType == null)
            return;

        AttackData selected = AttackSystem.Instance?.SelectedAttack;
        if (selected == null)
            return;

        if (!attackModules.TryGetValue(selected.type, out PlayerAttackModuleBase module) || module == null)
            return;

        module.ExecuteNormal(other);
    }

    // ── Public utility ────────────────────────────────────────────────────────

    /// <summary>
    /// Maps each enemy tag to the attack type that gains experience from killing it.
    /// Public so attack modules can register meat from kill events.
    /// Returns null when the tag is not a recognised enemy.
    /// </summary>
    public static AttackType? GetEnemyAttackType(string tag)
    {
        switch (tag)
        {
            case "PiercingFish": return AttackType.Stab;
            case "JawFish":      return AttackType.Bite;
            case "EelFish":      return AttackType.Zap;
            case "UglyFish":     return AttackType.Poison;
            default:             return null;
        }
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void TryFireSpecial()
    {
        if (AttackSystem.Instance == null) return;

        AttackData selected = AttackSystem.Instance.SelectedAttack;
        if (selected == null) return;

        if (!attackModules.TryGetValue(selected.type, out PlayerAttackModuleBase module) || module == null)
            return;

        if (!selected.specialUnlocked)
        {
            Debug.Log($"[PlayerScript] Special not yet unlocked for {selected.type}.");
            return;
        }

        if (selected.specialCooldownRemaining > 0f)
        {
            Debug.Log($"[PlayerScript] Special on cooldown: {selected.specialCooldownRemaining:F1}s remaining.");
            return;
        }

        module.ExecuteSpecial();
        selected.specialCooldownRemaining = selected.specialMoveCooldown;
    }

    private void EnsureModulesExist()
    {
        if (biteAttack == null)
            biteAttack = GetComponentInChildren<BiteAttackModule>(true) ?? gameObject.AddComponent<BiteAttackModule>();

        if (stabAttack == null)
            stabAttack = GetComponentInChildren<StabAttackModule>(true) ?? gameObject.AddComponent<StabAttackModule>();

        if (zapAttack == null)
            zapAttack = GetComponentInChildren<ZapAttackModule>(true) ?? gameObject.AddComponent<ZapAttackModule>();

        if (poisonAttack == null)
            poisonAttack = GetComponentInChildren<PoisonAttackModule>(true) ?? gameObject.AddComponent<PoisonAttackModule>();
    }

    private void RebuildModuleLookup()
    {
        attackModules.Clear();

        if (biteAttack != null)
            attackModules[AttackType.Bite] = biteAttack;

        if (stabAttack != null)
            attackModules[AttackType.Stab] = stabAttack;

        if (zapAttack != null)
            attackModules[AttackType.Zap] = zapAttack;

        if (poisonAttack != null)
            attackModules[AttackType.Poison] = poisonAttack;
    }
}
