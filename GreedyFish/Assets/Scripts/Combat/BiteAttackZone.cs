using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to a child of the Player named "BiteZone".
/// Triggers a configurable box-shaped AOE in front of the player when Activate() is called.
/// All fields are exposed so you can tune the shape, effect, and sound from the Inspector.
/// </summary>
public class BiteAttackZone : MonoBehaviour
{
    [Header("Zone Shape")]
    [Tooltip("Distance in front of the player centre where the box is placed.")]
    [SerializeField] private Vector2 offset = new Vector2(1f, 0f);

    [Tooltip("Width and height of the attack box in world units.")]
    [SerializeField] private Vector2 boxSize = new Vector2(1.5f, 1.2f);

    [Tooltip("Layers considered as enemies. Set to Everything if you have no dedicated Enemy layer.")]
    [SerializeField] private LayerMask enemyLayer = ~0;

    [Header("Timing")]
    [Tooltip("How long the visual indicator stays visible after the hit.")]
    [SerializeField] private float indicatorDuration = 0.12f;

    [Tooltip("Minimum seconds between successive activations (prevents instant re-triggers).")]
    [SerializeField] private float activationCooldown = 0.3f;

    [Header("Visual")]
    [Tooltip("Optional ParticleSystem that plays at the zone centre on each activation.")]
    [SerializeField] private ParticleSystem hitEffect;

    [Tooltip("Optional SpriteRenderer that briefly flashes to show the bite area.")]
    [SerializeField] private SpriteRenderer zoneIndicator;

    [Header("Audio")]
    [Tooltip("Name of the clip registered in AudioManager to play on each bite.")]
    [SerializeField] private string biteSoundClipName = "BiteAttack";

    // ── Runtime ───────────────────────────────────────────────────────────────

    private PlayerMovement _movement;
    private float _nextActivationTime;
    private static readonly Collider2D[] HitBuffer = new Collider2D[20];

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        _movement = GetComponentInParent<PlayerMovement>();

        if (zoneIndicator != null)
            zoneIndicator.enabled = false;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Triggers the bite AOE. Called by PlayerScript on collision while Bite is selected.</summary>
    public void Activate()
    {
        if (Time.time < _nextActivationTime) return;
        _nextActivationTime = Time.time + activationCooldown;
        StartCoroutine(ExecuteBite());
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private IEnumerator ExecuteBite()
    {
        // Derive worldCenter from the Player (BiteZone's parent) so this is always
        // drift-safe even if BiteZone's own position was accidentally mutated.
        Transform playerTransform = transform.parent != null ? transform.parent : transform;
        Vector2 worldCenter = (Vector2)playerTransform.position + GetWorldOffset();
        Vector2 visualOffset = GetWorldOffset();

        // Position visuals using localPosition (relative to BiteZone) instead of world
        // position. BiteZone inherits Player scale, so localPos = (offset.x, offset.y)
        // correctly resolves to the right world position for both facing directions.
        if (zoneIndicator != null)
        {
            zoneIndicator.transform.localPosition = new Vector3(
                visualOffset.x, visualOffset.y, zoneIndicator.transform.localPosition.z);
            zoneIndicator.enabled = true;
        }

        if (hitEffect != null)
        {
            hitEffect.transform.localPosition = new Vector3(
                visualOffset.x, visualOffset.y, hitEffect.transform.localPosition.z);
            hitEffect.Play();
        }

        AudioManager.Instance?.Play(biteSoundClipName);

        // Roll damage once — all enemies in the zone take the same roll.
        int damage = AttackSystem.Instance?.ExecuteAttack() ?? 0;

        // Find all colliders in the box and damage enemies.
        int count = Physics2D.OverlapBoxNonAlloc(worldCenter, boxSize, 0f, HitBuffer, enemyLayer);

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

    /// <summary>Returns the bite centre offset flipped to match the player's facing direction.</summary>
    private Vector2 GetWorldOffset()
    {
        // PlayerMovement.direction: 0 = left, 1 = right
        float xSign = (_movement != null && _movement.direction == 0) ? -1f : 1f;
        return new Vector2(offset.x * xSign, offset.y);
    }

    // ── Gizmo ─────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (_movement == null) _movement = GetComponentInParent<PlayerMovement>();

        Vector2 worldOffset = GetWorldOffset();
        Vector2 centre = (Vector2)transform.position + worldOffset;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.45f);
        Gizmos.DrawWireCube(centre, boxSize);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.15f);
        Gizmos.DrawCube(centre, boxSize);
    }
}
