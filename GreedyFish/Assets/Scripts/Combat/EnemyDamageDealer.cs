using UnityEngine;

/// <summary>
/// Attach to any enemy. Deals damage to the player on collision contact,
/// with a configurable cooldown to prevent frame-spam.
/// </summary>
public class EnemyDamageDealer : MonoBehaviour
{
    [SerializeField] private int damagePerHit = 10;

    [Tooltip("Minimum seconds between successive hits to the same player.")]
    [SerializeField] private float hitCooldown = 1f;


    private float _nextHitTime;

    // ── Collision ─────────────────────────────────────────────────────────────
   
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            TryDealDamage();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            TryDealDamage();
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void TryDealDamage()
    {


        if (Time.time < _nextHitTime) return;

        _nextHitTime = Time.time + hitCooldown;
        PlayerHealth.Instance?.TakeDamage(damagePerHit);
    }
}
