using System.Collections;
using UnityEngine;

public class PoisonAttackModule : PlayerAttackModuleBase
{
    [Header("Poison Normal")]
    [SerializeField] private int normalDiceSides = 3;

    [Header("Poison Status")]
    [SerializeField] private float stackDuration = 10f;
    [Tooltip("Tick interval for 1 / 2 / 3 poison stacks (seconds)")]
    [SerializeField] private float tickRate1Stack = 1.5f;
    [SerializeField] private float tickRate2Stacks = 1.0f;
    [SerializeField] private float tickRate3Stacks = 0.8f;
    [Tooltip("Poison damage per tick at each attack level (0-1 / 2 / 3)")]
    [SerializeField] private float poisonDamageLevel01 = 1.2f;
    [SerializeField] private float poisonDamageLevel2 = 1.6f;
    [SerializeField] private float poisonDamageLevel3 = 2.0f;

    [Header("Poison Special")]
    [SerializeField] private int specialDiceSides = 6;
    [SerializeField] private float specialRadius = 3f;
    [SerializeField] private float specialCooldown = 8f;
    [SerializeField] private float specialDamageMultiplier = 1.35f;
    [SerializeField] private float cloudDuration = 5f;
    [SerializeField] private float cloudRadius = 1.5f;
    [SerializeField] private float cloudTickInterval = 0.5f;
    [SerializeField] private float indicatorDuration = 0.2f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer zoneIndicator;
    [SerializeField] private GameObject cloudParticleEffectPrefab;
    [SerializeField] private Vector3 cloudParticleScale = Vector3.one;

    [Header("Audio")]
    [SerializeField] private string normalSoundClipName = "SpecialAttack";
    [SerializeField] private string specialSoundClipName = "SpecialAttack";

    public override AttackType AttackType => AttackType.Poison;

    protected override void Awake()
    {
        base.Awake();

        AttackData atkData = AttackSystem.Instance?.GetAttack(AttackType.Poison);
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

    private int RollPoisonDamage(int diceSides)
    {
        AttackData atkData = AttackSystem.Instance?.GetAttack(AttackType.Poison);
        if (atkData == null) return 0;

        int total = DiceRoller.Roll(
            atkData.diceCount,
            diceSides,
            atkData.flatBonus,
            out int[] individuals
        );

        AttackSystem.Instance.game.AddScore(total);
        AttackSystem.Instance.FireAttackRolledEvent(total, individuals);
        return total;
    }

    private float GetPoisonDamagePerTick()
    {
        AttackData atkData = AttackSystem.Instance?.GetAttack(AttackType.Poison);
        if (atkData == null) return poisonDamageLevel01;

        switch (atkData.level)
        {
            case 0:
            case 1:  return poisonDamageLevel01;
            case 2:  return poisonDamageLevel2;
            default: return poisonDamageLevel3;
        }
    }

    // ── Normal attack ─────────────────────────────────────────────────────────

    public override void ExecuteNormal(GameObject primaryTarget)
    {
        if (primaryTarget == null) return;

        int damage = RollPoisonDamage(normalDiceSides);
        DamageAndRegisterMeat(primaryTarget, damage);

        // Apply one non-refreshable poison stack (purple tint handled by PoisonStatus)
        ApplyPoisonStack(primaryTarget);

        if (!string.IsNullOrEmpty(normalSoundClipName))
            AudioManager.Instance?.Play(normalSoundClipName);
    }

    private void ApplyPoisonStack(GameObject target)
    {
        if (target == null) return;

        EnemyHealth enemy = target.GetComponent<EnemyHealth>();
        if (enemy == null || enemy.IsDead) return;

        PoisonStatus status = target.GetComponent<PoisonStatus>();
        if (status == null)
            status = target.AddComponent<PoisonStatus>();

        status.AddStack(stackDuration, GetPoisonDamagePerTick(),
            tickRate1Stack, tickRate2Stacks, tickRate3Stacks);
    }

    // ── Special attack ────────────────────────────────────────────────────────

    public override void ExecuteSpecial()
    {
        AttackData atkData = AttackSystem.Instance?.GetAttack(AttackType.Poison);
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
            float diameter = specialRadius * 2f;
            zoneIndicator.transform.localPosition = Vector3.zero;
            zoneIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
            zoneIndicator.enabled = true;
        }

        if (!string.IsNullOrEmpty(specialSoundClipName))
            AudioManager.Instance?.Play(specialSoundClipName);

        int damage = Mathf.Max(1, Mathf.CeilToInt(
            RollPoisonDamage(specialDiceSides) * specialDamageMultiplier));

        // Find all enemies in range
        int count = Physics2D.OverlapCircle(center, specialRadius, enemyFilter, HitBuffer);

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = HitBuffer[i];
            if (hit == null) continue;

            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy == null || enemy.IsDead) continue;

            // Deal damage to all enemies in range
            DamageAndRegisterMeat(hit, damage);

            // Spawn poison cloud from any currently poisoned enemy
            PoisonStatus status = hit.GetComponent<PoisonStatus>();
            if (status != null && status.IsActive)
            {
                SpawnPoisonCloud(hit.transform.position, status.Stacks);
            }
        }

        yield return new WaitForSeconds(indicatorDuration);

        if (debugMode && zoneIndicator != null)
            zoneIndicator.enabled = false;
    }

    private void SpawnPoisonCloud(Vector3 position, int stacks)
    {
        GameObject cloudGO;

        if (cloudParticleEffectPrefab != null)
        {
            cloudGO = Instantiate(cloudParticleEffectPrefab, position, Quaternion.identity);
            cloudGO.transform.localScale = cloudParticleScale;

            ParticleSystem ps = cloudGO.GetComponent<ParticleSystem>();
            if (ps != null)
                ps.Play();
        }
        else
        {
            cloudGO = new GameObject("PoisonCloud");
            cloudGO.transform.position = position;
        }

        // Add and configure the cloud emitter
        PoisonCloudEmitter emitter = cloudGO.GetComponent<PoisonCloudEmitter>();
        if (emitter == null)
            emitter = cloudGO.AddComponent<PoisonCloudEmitter>();

        emitter.Configure(enemyLayer, cloudRadius, cloudTickInterval,
            stacks, stackDuration, GetPoisonDamagePerTick(),
            tickRate1Stack, tickRate2Stacks, tickRate3Stacks);
        emitter.Activate(cloudDuration);

        // Auto-destroy after cloud duration + buffer for particles to finish
        Destroy(cloudGO, cloudDuration + 3f);
    }
}
