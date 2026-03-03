using System.Collections;
using UnityEngine;

[ExecuteAlways]
public class BiteAttackModule : PlayerAttackModuleBase
{
    [Header("Normal Bite")]
    [SerializeField] private Vector2 biteOffset = new Vector2(1f, 0f);
    [SerializeField] private Vector2 biteBoxSize = new Vector2(1.5f, 1.2f);
    [SerializeField] private float biteIndicatorDuration = 0.12f;
    [SerializeField] private float biteCooldown = 0.3f;

    [Header("Bite Special")]
    [SerializeField] private float specialRadius = 3f;
    [SerializeField] private float specialIndicatorDuration = 0.2f;
    [SerializeField] private float specialCooldown = 5f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer biteSprite;
    [SerializeField] private SpriteRenderer zoneIndicator;
    [SerializeField] private GameObject jawStormParticlePrefab;

    [Header("Audio")]
    [SerializeField] private string biteSoundClipName = "BiteAttack";
    [SerializeField] private string specialSoundClipName = "SpecialAttack";

    private float nextNormalTime;

    public override AttackType AttackType => AttackType.Bite;

    protected override void Awake()
    {
        base.Awake();

        // Set the special cooldown duration on the attack data
        AttackData atkData = AttackSystem.Instance?.GetAttack(AttackType.Bite);
        if (atkData != null)
            atkData.specialMoveCooldown = specialCooldown;

        if (zoneIndicator != null)
        {
            if (debugMode)
            {
                // Show normal bite zone permanently in debug mode
                UpdateDebugZoneIndicator(biteOffset, biteBoxSize);
            }
            else
            {
                zoneIndicator.enabled = false;
            }
        }
    }

    private void Update()
    {
        if (debugMode)
        {
            UpdateDebugZoneIndicator(biteOffset, biteBoxSize);
        }
        else if (zoneIndicator != null)
        {
            zoneIndicator.enabled = false;
        }
    }

    public override void ExecuteNormal(GameObject primaryTarget)
    {
        if (Time.time < nextNormalTime)
            return;

        nextNormalTime = Time.time + biteCooldown;
        StartCoroutine(DoBiteNormal());
    }

    public override void ExecuteSpecial()
    {
        AttackData atkData = AttackSystem.Instance?.GetAttack(AttackType.Bite);
        if (atkData == null || atkData.specialCooldownRemaining > 0)
            return;

        atkData.specialCooldownRemaining = specialCooldown;
        StartCoroutine(DoBiteSpecial());
    }

    private void UpdateDebugZoneIndicator(Vector2 offset, Vector2 size)
    {
        if (!debugMode || zoneIndicator == null)
            return;

        Vector2 signedOffset = GetFacingOffset(offset);
        zoneIndicator.transform.localPosition = new Vector3(signedOffset.x, signedOffset.y, zoneIndicator.transform.localPosition.z);
        zoneIndicator.transform.localScale = new Vector3(size.x, size.y, 1f);
        zoneIndicator.enabled = true;
    }

    private int RollSpecialDamage()
    {
        AttackData atkData = AttackSystem.Instance?.GetAttack(AttackType.Bite);
        if (atkData == null) return 0;

        // Roll with d12 (12-sided dice) instead of normal d6
        int total = DiceRoller.Roll(
            atkData.diceCount,
            12,
            atkData.flatBonus,
            out int[] individuals
        );

        AttackSystem.Instance.game.AddScore(total);
        AttackSystem.Instance.FireAttackRolledEvent(total, individuals);
        return total;
    }

    private IEnumerator DoBiteNormal()
    {
        Vector2 signedOffset = GetFacingOffset(biteOffset);
        Vector2 worldCenter = (Vector2)transform.position + signedOffset;

        UpdateDebugZoneIndicator(biteOffset, biteBoxSize);

        if (biteSprite != null)
        {
            // Use original offset for localPosition - parent scale handles the flip
            biteSprite.transform.localPosition = new Vector3(biteOffset.x, biteOffset.y, biteSprite.transform.localPosition.z);
            biteSprite.gameObject.SetActive(true);
        }

        AudioManager.Instance?.Play(biteSoundClipName);

        int damage = RollDamage();
        int count = Physics2D.OverlapBox(worldCenter, biteBoxSize, 0f, enemyFilter, HitBuffer);

        for (int i = 0; i < count; i++)
            DamageAndRegisterMeat(HitBuffer[i], damage);

        yield return new WaitForSeconds(biteIndicatorDuration);

        if (biteSprite != null)
            biteSprite.gameObject.SetActive(false);

        if (!debugMode && zoneIndicator != null)
            zoneIndicator.enabled = false;
    }

    private IEnumerator DoBiteSpecial()
    {
        Vector2 center = transform.position;

        UpdateDebugZoneIndicator(Vector2.zero, Vector2.one * specialRadius * 2f);

        if (biteSprite != null)
            biteSprite.gameObject.SetActive(true);

        // Spawn particle effect
        if (jawStormParticlePrefab != null)
        {
            GameObject particleInstance = Instantiate(jawStormParticlePrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = particleInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }
        }

        AudioManager.Instance?.Play(specialSoundClipName);

        int damage = RollSpecialDamage();
        int count = Physics2D.OverlapCircle(center, specialRadius, enemyFilter, HitBuffer);

        for (int i = 0; i < count; i++)
            DamageAndRegisterMeat(HitBuffer[i], damage);

        yield return new WaitForSeconds(specialIndicatorDuration);

        if (biteSprite != null)
            biteSprite.gameObject.SetActive(false);

        if (!debugMode && zoneIndicator != null)
            zoneIndicator.enabled = false;
    }
}
