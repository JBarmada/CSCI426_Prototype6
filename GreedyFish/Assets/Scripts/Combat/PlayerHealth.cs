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
    public int CurrentHealth { get; set; }
    public float HealthPercent => maxHealth > 0 ? (float)CurrentHealth / maxHealth : 0f;
    public bool IsDead => CurrentHealth <= 0;

    /// <summary>Fired whenever HP changes. (currentHP, maxHP)</summary>
    public event Action<int, int> OnHealthChanged;

    /// <summary>Fired once when HP reaches zero.</summary>
    public event Action OnDied;


    
    public CameraShake cam;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;
        CurrentHealth = maxHealth;
        cam = GameObject.FindAnyObjectByType<CameraShake>();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PiercingFish"))
        {
            TakeDamage(5);

        }
        else if (collision.gameObject.CompareTag("EelFish"))
        {
            TakeDamage(10);

        }
        else if (collision.gameObject.CompareTag("UglyFish"))
        {
            TakeDamage(5);

        }
        else if (collision.gameObject.CompareTag("JawFish"))
        {
            TakeDamage(15);

        }
    }
    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Deals damage to the player. Triggers death and GameOver when HP hits zero.</summary>
    public void TakeDamage(int amount)
    {
        cam.start = true;
        if (IsDead) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (IsDead)
        {
            cam.start = false;
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
