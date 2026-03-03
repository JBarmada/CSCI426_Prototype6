using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZapAttackModule : PlayerAttackModuleBase
{
    [Header("Zap Normal")]
    [SerializeField] private float normalStunDuration = 1.25f;
    [SerializeField] private int normalDiceSides = 5;

    [Header("Zap Special")]
    [SerializeField] private float specialAcquireRadius = 3.5f;
    [SerializeField] private float chainJumpRadius = 3f;
    [SerializeField] private int minChainTargets = 2;
    [SerializeField] private int maxChainTargets = 5;
    [SerializeField] private float specialDamageMultiplier = 1.35f;
    [SerializeField] private float specialStunDuration = 1.75f;
    [SerializeField] private float indicatorDuration = 0.2f;
    [SerializeField] private int specialDiceSides = 6;
    [SerializeField] private float specialCooldown = 8f;

    [Header("Visuals")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private SpriteRenderer zoneIndicator;

    [Header("Audio")]
    [SerializeField] private string attackSoundClipName = "SpecialAttack";

    public override AttackType AttackType => AttackType.Zap;

    protected override void Awake()
    {
        base.Awake();

        // Set the dice sides and special cooldown on the attack data
        AttackData atkData = AttackSystem.Instance?.GetAttack(AttackType.Zap);
        if (atkData != null)
        {
            atkData.diceSides = normalDiceSides;
            atkData.specialDiceSides = specialDiceSides;
            atkData.specialMoveCooldown = specialCooldown;
        }

        if (zoneIndicator != null)
            zoneIndicator.enabled = false;
    }

    // ── Custom dice rolls ─────────────────────────────────────────────────────

    private int RollZapDamage(int diceSides)
    {
        AttackData atkData = AttackSystem.Instance?.GetAttack(AttackType.Zap);
        if (atkData == null) return 0;

        int buffBonus = AttackSystem.Instance.GetBuffDamageBonus(AttackType.Zap);

        int total = DiceRoller.Roll(
            atkData.diceCount,
            diceSides,
            atkData.flatBonus + buffBonus,
            out int[] individuals
        );

        GameManager.Instance?.AddScore(total);
        AttackSystem.Instance.FireAttackRolledEvent(total, individuals);
        AttackSystem.Instance.GrantHitXP(AttackType.Zap);
        return total;
    }

    // ── Particle helper ───────────────────────────────────────────────────────

    private void SpawnHitEffectOnTarget(Transform target)
    {
        if (hitEffectPrefab == null || target == null) return;

        GameObject fx = Instantiate(hitEffectPrefab, target.position, Quaternion.identity, target);
        ParticleSystem ps = fx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            Destroy(fx, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Destroy(fx, 2f);
        }
    }

    // ── Normal attack ─────────────────────────────────────────────────────────

    public override void ExecuteNormal(GameObject primaryTarget)
    {
        if (primaryTarget == null)
            return;

        AudioManager.Instance?.Play(attackSoundClipName);

        int damage = RollZapDamage(normalDiceSides);
        DamageAndRegisterMeat(primaryTarget, damage);

        // Spawn particle effect on the enemy
        SpawnHitEffectOnTarget(primaryTarget.transform);

        EnemyHealth enemy = primaryTarget.GetComponent<EnemyHealth>();
        EvilFishScript fish = primaryTarget.GetComponent<EvilFishScript>();
        if (enemy != null && fish != null && !enemy.IsDead)
            fish.ApplyStun(normalStunDuration);
    }

    // ── Special attack ────────────────────────────────────────────────────────

    public override void ExecuteSpecial()
    {
        AttackData atkData = AttackSystem.Instance?.GetAttack(AttackType.Zap);
        if (atkData == null || atkData.specialCooldownRemaining > 0)
            return;

        atkData.specialCooldownRemaining = specialCooldown;
        StartCoroutine(DoSpecial());
    }

    private IEnumerator DoSpecial()
    {
        Vector2 center = transform.position;

        if (debugMode && zoneIndicator != null)
        {
            float diameter = specialAcquireRadius * 2f;
            zoneIndicator.transform.localPosition = Vector3.zero;
            zoneIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
            zoneIndicator.enabled = true;
        }

        AudioManager.Instance?.Play(attackSoundClipName);

        int chainTargetCount = Random.Range(Mathf.Max(1, minChainTargets), Mathf.Max(1, maxChainTargets) + 1);
        int damage = Mathf.Max(1, Mathf.CeilToInt(RollZapDamage(specialDiceSides) * specialDamageMultiplier));

        var visited = new HashSet<EnemyHealth>();
        Collider2D currentLink = FindNearestEnemy(center, specialAcquireRadius, visited);

        for (int i = 0; i < chainTargetCount && currentLink != null; i++)
        {
            EnemyHealth enemy = currentLink.GetComponent<EnemyHealth>();
            if (enemy == null || enemy.IsDead)
                break;

            visited.Add(enemy);
            DamageAndRegisterMeat(currentLink, damage);

            // Spawn particle effect on each chained enemy
            SpawnHitEffectOnTarget(currentLink.transform);

            if (!enemy.IsDead)
            {
                EvilFishScript fish = currentLink.GetComponent<EvilFishScript>();
                if (fish != null)
                    fish.ApplyStun(specialStunDuration);
            }

            currentLink = FindNearestEnemy(currentLink.transform.position, chainJumpRadius, visited);
        }

        yield return new WaitForSeconds(indicatorDuration);

        if (debugMode && zoneIndicator != null)
            zoneIndicator.enabled = false;
    }

    private Collider2D FindNearestEnemy(Vector2 origin, float searchRadius, HashSet<EnemyHealth> ignoreSet)
    {
        int count = Physics2D.OverlapCircle(origin, searchRadius, enemyFilter, HitBuffer);
        Collider2D nearestHit = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = HitBuffer[i];
            if (hit == null)
                continue;

            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy == null || enemy.IsDead || ignoreSet.Contains(enemy))
                continue;

            float distance = ((Vector2)hit.transform.position - origin).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearestHit = hit;
            }
        }

        return nearestHit;
    }
}
