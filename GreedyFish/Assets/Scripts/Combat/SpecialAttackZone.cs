using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to a child of the Player named "SpecialZone".
/// Executes a 360° circle AOE around the player when Activate() is called.
/// All fields are exposed for Inspector tuning.
/// </summary>
public class SpecialAttackZone : MonoBehaviour
{
    [Header("Zone Shape")]
    [Tooltip("Radius of the 360° explosion in world units.")]
    [SerializeField] private float radius = 3f;

    [Tooltip("Layers considered as enemies.")]
    [SerializeField] private LayerMask enemyLayer = ~0;

    [Header("Timing")]
    [Tooltip("How long the visual indicator stays visible.")]
    [SerializeField] private float indicatorDuration = 0.2f;

    [Header("Visual")]
    [Tooltip("Optional ParticleSystem that plays at the player position on activation.")]
    [SerializeField] private ParticleSystem hitEffect;

    [Tooltip("Optional SpriteRenderer (circle sprite) that briefly shows the blast radius.")]
    [SerializeField] private SpriteRenderer zoneIndicator;

    [Header("Audio")]
    [Tooltip("Name of the clip registered in AudioManager to play on activation.")]
    [SerializeField] private string specialSoundClipName = "SpecialAttack";

    // ── Runtime ───────────────────────────────────────────────────────────────

    private static readonly Collider2D[] HitBuffer = new Collider2D[30];

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (zoneIndicator != null)
            zoneIndicator.enabled = false;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Triggers the 360° special AOE. Wire this up to your input / UI button.</summary>
    public void Activate()
    {
        StartCoroutine(ExecuteSpecial());
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private IEnumerator ExecuteSpecial()
    {
        Vector2 centre = transform.position;

        // Show visual indicator scaled to the blast radius.
        if (zoneIndicator != null)
        {
            float diameter = radius * 2f;
            zoneIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
            zoneIndicator.enabled = true;
        }

        if (hitEffect != null)
            hitEffect.Play();

        AudioManager.Instance?.Play(specialSoundClipName);

        // Roll damage once for the entire blast.
        int damage = AttackSystem.Instance?.ExecuteAttack() ?? 0;

        // Hit every enemy in the circle.
        int count = Physics2D.OverlapCircleNonAlloc(centre, radius, HitBuffer, enemyLayer);

        for (int i = 0; i < count; i++)
        {
            EnemyHealth enemy = HitBuffer[i].GetComponent<EnemyHealth>();
            if (enemy == null || enemy.IsDead) continue;

            bool wasAlive = !enemy.IsDead;
            enemy.TakeDamage(damage);

            if (wasAlive && enemy.IsDead)
            {
                AttackType? type = PlayerScript.GetEnemyAttackType(HitBuffer[i].tag);
                if (type.HasValue)
                {
                    int meatCount = AttackSystem.Instance?.RollMeatDrop() ?? 1;
                    AttackUpgradeSystem.Instance?.RegisterMeat(type.Value, meatCount);
                }
            }
        }

        yield return new WaitForSeconds(indicatorDuration);

        if (zoneIndicator != null)
            zoneIndicator.enabled = false;
    }

    // ── Gizmo ─────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.color = new Color(0.5f, 0f, 1f, 0.1f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
