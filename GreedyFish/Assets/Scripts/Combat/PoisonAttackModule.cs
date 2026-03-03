using System.Collections;
using UnityEngine;

public class PoisonAttackModule : PlayerAttackModuleBase
{
    [Header("Poison Status")]
    [SerializeField] private float normalPoisonDuration = 4f;
    [SerializeField] private float specialPoisonDuration = 6f;

    [Header("Poison Special")]
    [SerializeField] private float specialRadius = 3f;
    [SerializeField] private float cloudDuration = 5f;
    [SerializeField] private float cloudRadius = 1.5f;
    [SerializeField] private float cloudTickInterval = 0.5f;
    [SerializeField] private int cloudTickDamage = 1;
    [SerializeField] private float indicatorDuration = 0.2f;

    [Header("Visuals")]
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private SpriteRenderer zoneIndicator;

    [Header("Audio")]
    [SerializeField] private string normalSoundClipName = "SpecialAttack";
    [SerializeField] private string specialSoundClipName = "SpecialAttack";

    public override AttackType AttackType => AttackType.Poison;

    protected override void Awake()
    {
        base.Awake();

        if (zoneIndicator != null)
            zoneIndicator.enabled = false;
    }

    public override void ExecuteNormal(GameObject primaryTarget)
    {
        int damage = RollDamage();
        DamageAndRegisterMeat(primaryTarget, damage);
        ApplyPoison(primaryTarget, normalPoisonDuration);

        if (!string.IsNullOrEmpty(normalSoundClipName))
            AudioManager.Instance?.Play(normalSoundClipName);
    }

    public override void ExecuteSpecial()
    {
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

        if (hitEffect != null)
            hitEffect.Play();

        if (!string.IsNullOrEmpty(specialSoundClipName))
            AudioManager.Instance?.Play(specialSoundClipName);

        int damage = RollDamage();
        int count = Physics2D.OverlapCircle(center, specialRadius, enemyFilter, HitBuffer);

        for (int i = 0; i < count; i++)
        {
            DamageAndRegisterMeat(HitBuffer[i], damage);
            TriggerPoisonCloud(HitBuffer[i]);
        }

        yield return new WaitForSeconds(indicatorDuration);

        if (debugMode && zoneIndicator != null)
            zoneIndicator.enabled = false;
    }

    private void ApplyPoison(GameObject target, float duration)
    {
        if (target == null)
            return;

        PoisonStatus status = target.GetComponent<PoisonStatus>();
        if (status == null)
            status = target.AddComponent<PoisonStatus>();

        status.Apply(duration);
    }

    private void TriggerPoisonCloud(Collider2D hit)
    {
        if (hit == null)
            return;

        EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
        if (enemy == null || enemy.IsDead)
            return;

        PoisonStatus status = hit.GetComponent<PoisonStatus>();
        if (status == null)
            status = hit.gameObject.AddComponent<PoisonStatus>();

        status.Apply(specialPoisonDuration);

        if (!status.IsActive)
            return;

        PoisonCloudEmitter emitter = hit.GetComponent<PoisonCloudEmitter>();
        if (emitter == null)
            emitter = hit.gameObject.AddComponent<PoisonCloudEmitter>();

        emitter.Configure(enemyLayer, cloudRadius, cloudTickInterval, cloudTickDamage);
        emitter.Activate(cloudDuration, hit.gameObject);
    }
}
