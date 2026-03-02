using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private BiteAttackZone    _biteZone;
    private SpecialAttackZone _specialZone;

    private void Start()
    {
        _biteZone    = GetComponentInChildren<BiteAttackZone>(true);
        _specialZone = GetComponentInChildren<SpecialAttackZone>(true);
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

        if (selected.type == AttackType.Bite)
        {
            // Bite: AOE zone handles damage and meat registration for all enemies in range.
            _biteZone?.Activate();
        }
        else
        {
            // All other attacks: single-target direct hit.
            int damage = AttackSystem.Instance?.ExecuteAttack() ?? 0;

            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy == null)
                return;

            enemy.TakeDamage(damage);

            if (enemy.IsDead)
            {
                int meatCount = AttackSystem.Instance?.RollMeatDrop() ?? 1;
                AttackUpgradeSystem.Instance?.RegisterMeat(enemyAttackType.Value, meatCount);
            }
        }
    }

    // ── Public utility ────────────────────────────────────────────────────────

    /// <summary>
    /// Maps each enemy tag to the attack type that gains experience from killing it.
    /// Public so BiteAttackZone and SpecialAttackZone can register meat on AOE kills.
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

    /// <summary>
    /// Activates the 360° special zone if the currently selected attack's special is
    /// unlocked and off cooldown. Starts the cooldown timer on success.
    /// </summary>
    private void TryFireSpecial()
    {
        if (AttackSystem.Instance == null || _specialZone == null) return;

        AttackData selected = AttackSystem.Instance.SelectedAttack;
        if (selected == null) return;

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

        _specialZone.Activate();
        selected.specialCooldownRemaining = selected.specialMoveCooldown;
    }
}
