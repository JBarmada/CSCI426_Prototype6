using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StabAttackModule : PlayerAttackModuleBase
{
    [Header("Normal Stab")]
    [SerializeField] private Vector2 stabOffset = new Vector2(1.2f, 0f);
    [SerializeField] private Vector2 stabBoxSize = new Vector2(2.8f, 0.7f);
    [SerializeField] private float hitPauseDuration = 0.1f;
    [SerializeField] private float indicatorDuration = 0.15f;

    [Header("Stab Special")]
    [SerializeField] private Vector2 specialOffset = new Vector2(1.4f, 0f);
    [SerializeField] private Vector2 specialBoxSize = new Vector2(3.2f, 0.8f);
    [SerializeField] private float tridentLaneSpacing = 0.8f;
    [SerializeField] private float specialDamageMultiplier = 1.5f;
    [SerializeField] private float specialIndicatorDuration = 0.2f;
    [SerializeField] private float specialCooldown = 8f;

    [Header("Visuals")]
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private SpriteRenderer zoneIndicator;
    [SerializeField] private GameObject specialParticleEffectPrefab;

    [Header("Audio")]
    [SerializeField] private string attackSoundClipName = "SpecialAttack";

    public override AttackType AttackType => AttackType.Stab;

    protected override void Awake()
    {
        base.Awake();

        // Set the special cooldown duration on the attack data
        AttackData atkData = AttackSystem.Instance?.GetAttack(AttackType.Stab);
        if (atkData != null)
        {
            atkData.specialMoveCooldown = specialCooldown;
            atkData.diceSides = 4; // Stab uses d4 instead of d6
        }

        if (zoneIndicator != null)
            zoneIndicator.enabled = false;
    }

    public override void ExecuteNormal(GameObject primaryTarget)
    {
        StartCoroutine(DoNormalStab());
    }

    public override void ExecuteSpecial()
    {
        AttackData atkData = AttackSystem.Instance?.GetAttack(AttackType.Stab);
        if (atkData == null || atkData.specialCooldownRemaining > 0)
            return;

        atkData.specialCooldownRemaining = specialCooldown;
        StartCoroutine(DoSpecial());
    }

    private int RollStabDamage()
    {
        AttackData atkData = AttackSystem.Instance?.GetAttack(AttackType.Stab);
        if (atkData == null) return 0;

        // Stab uses d4 dice
        int total = DiceRoller.Roll(
            atkData.diceCount,
            4,
            atkData.flatBonus,
            out int[] individuals
        );

        AttackSystem.Instance.game.AddScore(total);
        AttackSystem.Instance.FireAttackRolledEvent(total, individuals);
        return total;
    }

    private IEnumerator DoNormalStab()
    {
        Vector2 signedOffset = GetFacingOffset(stabOffset);
        Vector2 center = (Vector2)transform.position + signedOffset;

        // Show zone indicator - use original offset for localPosition
        if (zoneIndicator != null)
        {
            zoneIndicator.transform.localPosition = new Vector3(stabOffset.x, stabOffset.y, zoneIndicator.transform.localPosition.z);
            zoneIndicator.transform.localScale = new Vector3(stabBoxSize.x, stabBoxSize.y, 1f);
            zoneIndicator.enabled = true;
        }

        // Play hit effect at attack location
        if (hitEffect != null)
        {
            hitEffect.transform.position = center;
            hitEffect.Play();
        }

        AudioManager.Instance?.Play(attackSoundClipName);

        // First hit
        int damage1 = RollStabDamage();
        int count1 = Physics2D.OverlapBox(center, stabBoxSize, 0f, enemyFilter, HitBuffer);
        for (int i = 0; i < count1; i++)
            DamageAndRegisterMeat(HitBuffer[i], damage1);

        // Tiny pause
        yield return new WaitForSeconds(hitPauseDuration);

        // Second hit
        int damage2 = RollStabDamage();
        int count2 = Physics2D.OverlapBox(center, stabBoxSize, 0f, enemyFilter, HitBuffer);
        for (int i = 0; i < count2; i++)
            DamageAndRegisterMeat(HitBuffer[i], damage2);

        yield return new WaitForSeconds(indicatorDuration);

        if (zoneIndicator != null)
            zoneIndicator.enabled = false;
    }

    private IEnumerator DoSpecial()
    {
        Vector2 signedOffset = GetFacingOffset(specialOffset);
        Vector2 baseCenter = (Vector2)transform.position + signedOffset;

        // Show zone indicators for all 3 trident lanes
        if (zoneIndicator != null)
        {
            // Use original offset for localPosition
            zoneIndicator.transform.localPosition = new Vector3(specialOffset.x, specialOffset.y, zoneIndicator.transform.localPosition.z);
            // Make indicator large enough to show all 3 lanes
            float totalHeight = specialBoxSize.y + (tridentLaneSpacing * 2);
            zoneIndicator.transform.localScale = new Vector3(specialBoxSize.x, totalHeight, 1f);
            zoneIndicator.enabled = true;
        }

        // Spawn particle effects for each lane
        if (specialParticleEffectPrefab != null)
        {
            for (int lane = -1; lane <= 1; lane++)
            {
                Vector2 laneCenter = baseCenter + new Vector2(0f, lane * tridentLaneSpacing);
                GameObject particleInstance = Instantiate(specialParticleEffectPrefab, laneCenter, Quaternion.identity);
                ParticleSystem ps = particleInstance.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Play();
                    // Destroy after particle system finishes
                    Destroy(particleInstance, ps.main.duration + ps.main.startLifetime.constantMax);
                }
                else
                {
                    // If no particle system, destroy after indicator duration
                    Destroy(particleInstance, specialIndicatorDuration);
                }
            }
        }

        // Play hit effect at base attack location
        if (hitEffect != null)
        {
            hitEffect.transform.position = baseCenter;
            hitEffect.Play();
        }

        AudioManager.Instance?.Play(attackSoundClipName);

        int damage = Mathf.Max(1, Mathf.CeilToInt(RollStabDamage() * specialDamageMultiplier));
        var hitEnemies = new HashSet<EnemyHealth>();

        // Trident: three parallel stronger pierces (top / center / bottom lanes).
        for (int lane = -1; lane <= 1; lane++)
        {
            Vector2 laneCenter = baseCenter + new Vector2(0f, lane * tridentLaneSpacing);
            int count = Physics2D.OverlapBox(laneCenter, specialBoxSize, 0f, enemyFilter, HitBuffer);

            for (int i = 0; i < count; i++)
            {
                Collider2D hit = HitBuffer[i];
                if (hit == null)
                    continue;

                EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                if (enemy == null || enemy.IsDead || hitEnemies.Contains(enemy))
                    continue;

                hitEnemies.Add(enemy);
                DamageAndRegisterMeat(hit, damage);
            }
        }

        yield return new WaitForSeconds(specialIndicatorDuration);

        if (zoneIndicator != null)
            zoneIndicator.enabled = false;
    }
}
