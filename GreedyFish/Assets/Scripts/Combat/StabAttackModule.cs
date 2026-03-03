using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StabAttackModule : PlayerAttackModuleBase
{
    [Header("Normal Stab")]
    [SerializeField] private Vector2 stabOffset = new Vector2(1.2f, 0f);
    [SerializeField] private Vector2 stabBoxSize = new Vector2(2.8f, 0.7f);

    [Header("Stab Special")]
    [SerializeField] private Vector2 specialOffset = new Vector2(1.4f, 0f);
    [SerializeField] private Vector2 specialBoxSize = new Vector2(3.2f, 0.8f);
    [SerializeField] private float tridentLaneSpacing = 0.8f;
    [SerializeField] private float specialDamageMultiplier = 1.5f;
    [SerializeField] private float indicatorDuration = 0.2f;

    [Header("Visuals")]
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private SpriteRenderer zoneIndicator;

    [Header("Audio")]
    [SerializeField] private string attackSoundClipName = "SpecialAttack";

    public override AttackType AttackType => AttackType.Stab;

    protected override void Awake()
    {
        base.Awake();

        if (zoneIndicator != null)
            zoneIndicator.enabled = false;
    }

    public override void ExecuteNormal(GameObject primaryTarget)
    {
        Vector2 center = (Vector2)transform.position + GetFacingOffset(stabOffset);
        int damage = RollDamage();
        int count = Physics2D.OverlapBox(center, stabBoxSize, 0f, enemyFilter, HitBuffer);

        for (int i = 0; i < count; i++)
            DamageAndRegisterMeat(HitBuffer[i], damage);
    }

    public override void ExecuteSpecial()
    {
        StartCoroutine(DoSpecial());
    }

    private IEnumerator DoSpecial()
    {
        Vector2 signedOffset = GetFacingOffset(specialOffset);
        Vector2 baseCenter = (Vector2)transform.position + signedOffset;

        if (debugMode && zoneIndicator != null)
        {
            zoneIndicator.transform.localPosition = new Vector3(signedOffset.x, signedOffset.y, zoneIndicator.transform.localPosition.z);
            zoneIndicator.transform.localScale = new Vector3(specialBoxSize.x, specialBoxSize.y, 1f);
            zoneIndicator.enabled = true;
        }

        if (hitEffect != null)
            hitEffect.Play();

        AudioManager.Instance?.Play(attackSoundClipName);

        int damage = Mathf.Max(1, Mathf.CeilToInt(RollDamage() * specialDamageMultiplier));
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

        yield return new WaitForSeconds(indicatorDuration);

        if (debugMode && zoneIndicator != null)
            zoneIndicator.enabled = false;
    }
}
