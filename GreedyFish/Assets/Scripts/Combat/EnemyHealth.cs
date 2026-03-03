using System;
using UnityEngine;
using System.Collections;


/// <summary>
/// Attached to every enemy. Tracks HP and fires OnHealthChanged so health bars can react.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 20;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }

    /// <summary>True once the enemy's HP reaches zero.</summary>
    public bool IsDead => CurrentHealth <= 0;

    /// <summary>Fired whenever HP changes. (currentHP, maxHP)</summary>
    public event Action<int, int> OnHealthChanged;
    public FishSpawner spawner;
    



 
    public GameObject[] projectilePrefabs;

    [SerializeField] private float spawnInterval = 1.0f;
    [Tooltip("Number of fish to spawn per group")]
    [SerializeField] private int groupSize = 1;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        spawner = GameObject.FindAnyObjectByType<FishSpawner>();
       
    }

    /// <summary>Reduces HP by <paramref name="amount"/>. Destroys the GameObject on death.</summary>
    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (IsDead)
        {
            //if (this.CompareTag("EelFish"))
            //{
            GameManager.Instance?.AddScore(4, ScoreCategory.EnemyKill);
            groupSize = 1;
            StartCoroutine(SpawnProj());
            

            //}
            spawner.deadCount++;
            Destroy(gameObject);

        }

    }


    private IEnumerator SpawnProj()
    {


        int randProj = UnityEngine.Random.Range(0, projectilePrefabs.Length);
        



        for (int i = 0; i < groupSize; i++)
        {
            Instantiate(projectilePrefabs[randProj], transform.position, transform.rotation);

        }
        StopCoroutine(SpawnProj());
            
        yield return new WaitForSeconds(spawnInterval);
        


    }
}
