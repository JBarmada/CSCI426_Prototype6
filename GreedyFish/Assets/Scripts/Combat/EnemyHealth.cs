using UnityEngine;

/// <summary>
/// Stub component to be attached to every enemy GameObject.
/// Tracks hit points and exposes TakeDamage / IsDead for the attack system.
/// Expand with animations, drops, and pooling in a future pass.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 20;

    public int CurrentHealth { get; private set; }

    /// <summary>True once the enemy's HP reaches zero.</summary>
    public bool IsDead => CurrentHealth <= 0;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    /// <summary>Reduces the enemy's HP by <paramref name="amount"/>. Destroys the GameObject on death.</summary>
    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        CurrentHealth -= amount;
        Debug.Log($"[EnemyHealth] {gameObject.name} took {amount} damage → {CurrentHealth} HP");

        if (IsDead)
        {
            Debug.Log($"[EnemyHealth] {gameObject.name} died.");
            Destroy(gameObject);
        }
    }
}
