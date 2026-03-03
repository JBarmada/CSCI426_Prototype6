using UnityEngine;

/// <summary>
/// Stacking poison debuff on enemies. Max 3 stacks (non-refreshable from basic attacks).
/// Tick rate scales with stacks: configurable (default 1.5 / 1.0 / 0.8).
/// Applies a dark purple tint to the enemy sprite and health bar while active.
/// </summary>
public class PoisonStatus : MonoBehaviour
{
    public const int MaxStacks = 3;

    private int _stacks;
    private float _remainingDuration;
    private float _tickTimer;
    private float _damagePerTick = 1.2f;
    private float _tick1 = 1.5f, _tick2 = 1.0f, _tick3 = 0.8f;

    // Cached references for tinting
    private SpriteRenderer _spriteRenderer;
    private EnemyHealthBarUI _healthBar;
    private Color _originalSpriteColor;
    private bool _isTinted;
    private bool _referencesCached;

    private static readonly Color PoisonTintColor = new Color(0.45f, 0.1f, 0.55f, 1f);

    public int Stacks => _stacks;
    public bool IsActive => _stacks > 0 && _remainingDuration > 0f;

    private void Update()
    {
        if (_stacks <= 0 || _remainingDuration <= 0f)
        {
            if (_isTinted) ClearPoison();
            return;
        }

        _remainingDuration -= Time.deltaTime;
        if (_remainingDuration <= 0f)
        {
            ClearPoison();
            return;
        }

        _tickTimer -= Time.deltaTime;
        if (_tickTimer <= 0f)
        {
            _tickTimer = GetTickInterval();
            ApplyTickDamage();
        }
    }

    /// <summary>
    /// Adds one poison stack (non-refreshable — does not reset duration of existing stacks).
    /// </summary>
    public void AddStack(float duration, float damagePerTick,
        float tick1 = 1.5f, float tick2 = 1.0f, float tick3 = 0.8f)
    {
        CacheReferences();
        _damagePerTick = damagePerTick;
        _tick1 = tick1; _tick2 = tick2; _tick3 = tick3;

        if (_stacks < MaxStacks)
        {
            _stacks++;
            // Only set duration if no active poison (non-refreshable)
            if (_remainingDuration <= 0f)
                _remainingDuration = duration;
        }
        else if (_remainingDuration <= 0f)
        {
            // Re-apply if fully expired at max stacks
            _remainingDuration = duration;
        }

        _tickTimer = GetTickInterval();
        ApplyTint();
    }

    /// <summary>
    /// Sets stacks and refreshes duration (used by poison cloud — DOES refresh).
    /// </summary>
    public void ApplyStacksWithRefresh(int stackCount, float duration, float damagePerTick,
        float tick1 = 1.5f, float tick2 = 1.0f, float tick3 = 0.8f)
    {
        CacheReferences();
        _damagePerTick = damagePerTick;
        _tick1 = tick1; _tick2 = tick2; _tick3 = tick3;

        int newStacks = Mathf.Clamp(stackCount, 1, MaxStacks);
        if (newStacks > _stacks)
            _stacks = newStacks;

        _remainingDuration = duration; // Refresh!
        _tickTimer = GetTickInterval();
        ApplyTint();
    }

    private void ClearPoison()
    {
        _stacks = 0;
        _remainingDuration = 0f;
        _tickTimer = 0f;
        RemoveTint();
    }

    private float GetTickInterval()
    {
        switch (_stacks)
        {
            case 1:  return _tick1;
            case 2:  return _tick2;
            default: return _tick3; // 3+
        }
    }

    private void ApplyTickDamage()
    {
        EnemyHealth enemy = GetComponent<EnemyHealth>();
        if (enemy == null || enemy.IsDead) return;

        int damage = Mathf.Max(1, Mathf.RoundToInt(_damagePerTick));
        enemy.TakeDamage(damage);
    }

    // ── Tinting ───────────────────────────────────────────────────────────────

    private void CacheReferences()
    {
        if (_referencesCached) return;
        _referencesCached = true;

        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (_spriteRenderer != null)
            _originalSpriteColor = _spriteRenderer.color;

        _healthBar = GetComponentInChildren<EnemyHealthBarUI>();
    }

    private void ApplyTint()
    {
        if (_isTinted) return;
        _isTinted = true;

        if (_spriteRenderer != null)
            _spriteRenderer.color = Color.Lerp(_originalSpriteColor, PoisonTintColor, 0.6f);

        if (_healthBar != null)
            _healthBar.SetPoisonTint(true);
    }

    private void RemoveTint()
    {
        if (!_isTinted) return;
        _isTinted = false;

        if (_spriteRenderer != null)
            _spriteRenderer.color = _originalSpriteColor;

        if (_healthBar != null)
            _healthBar.SetPoisonTint(false);
    }
}
