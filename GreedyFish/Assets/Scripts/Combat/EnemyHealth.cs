using System;
using UnityEngine;

/// <summary>
/// Attached to every enemy. Tracks HP and fires OnHealthChanged so health bars can react.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 20;

    public int MaxHealth     => maxHealth;
    public int CurrentHealth { get; private set; }

    /// <summary>True once the enemy's HP reaches zero.</summary>
    public bool IsDead => CurrentHealth <= 0;

    /// <summary>Fired whenever HP changes. (currentHP, maxHP)</summary>
    public event Action<int, int> OnHealthChanged;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    /// <summary>Reduces HP by <paramref name="amount"/>. Destroys the GameObject on death.</summary>
    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (IsDead)
            Destroy(gameObject);
    }
}
