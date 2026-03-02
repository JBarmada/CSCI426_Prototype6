using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private int health;

    private void Start()
    {
        health = 100;
    }

    private void Update()
    {
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;

        // Determine which attack type corresponds to this enemy tag.
        AttackType? enemyAttackType = GetEnemyAttackType(other.tag);
        if (enemyAttackType == null)
            return;

        // Execute the currently selected attack and apply damage to the enemy.
        int damage = 0;
        if (AttackSystem.Instance != null)
            damage = AttackSystem.Instance.ExecuteAttack();

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy == null)
            return;

        enemy.TakeDamage(damage);

        // If the enemy died, roll meat and register with the upgrade system.
        if (enemy.IsDead)
        {
            int meatCount = AttackSystem.Instance != null
                ? AttackSystem.Instance.RollMeatDrop()
                : 1;

            AttackUpgradeSystem.Instance?.RegisterMeat(enemyAttackType.Value, meatCount);
        }
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Maps each enemy tag to the attack type that gains experience from killing it.
    /// Returns null when the collider is not a recognised enemy.
    /// </summary>
    private static AttackType? GetEnemyAttackType(string tag)
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
}
