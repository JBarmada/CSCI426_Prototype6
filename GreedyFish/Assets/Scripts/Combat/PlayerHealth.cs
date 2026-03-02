using System;
using UnityEngine;

/// <summary>
/// Scene-local singleton that tracks the player's HP.
/// Fires events so PlayerHealthBarUI and other systems stay in sync.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [SerializeField] private int maxHealth = 100;

    public int MaxHealth     => maxHealth;
    public int CurrentHealth { get; private set; }
    public float HealthPercent => maxHealth > 0 ? (float)CurrentHealth / maxHealth : 0f;
    public bool IsDead => CurrentHealth <= 0;

    /// <summary>Fired whenever HP changes. (currentHP, maxHP)</summary>
    public event Action<int, int> OnHealthChanged;

    /// <summary>Fired once when HP reaches zero.</summary>
    public event Action OnDied;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;
        CurrentHealth = maxHealth;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Deals damage to the player. Triggers death and GameOver when HP hits zero.</summary>
    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (IsDead)
        {
            OnDied?.Invoke();
            GameManager.Instance?.NotifyPlayerDied();
        }
    }

    /// <summary>Restores HP up to the maximum.</summary>
    public void Heal(int amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
