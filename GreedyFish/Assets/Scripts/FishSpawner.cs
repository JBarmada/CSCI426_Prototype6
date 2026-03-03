using UnityEngine;
using System.Collections;



public class FishSpawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject[] projectilePrefabs;

    public GameObject poisonEffect;

    [SerializeField] private float spawnInterval = 1.0f;
    [Tooltip("Number of fish to spawn per group")]
    [SerializeField] private int groupSize = 1;

    [Header("Delayed Activation")]
    [Tooltip("If > 0, this spawner waits until TimeRemaining drops to this value before starting. Leave at 0 for immediate start.")]
    [SerializeField] private float activateAtTimeRemaining = 0f;

    public int deadCount = 0;
    public int spawnCount = 0;
    private bool activated = false;

    void Start()
    {
        if (activateAtTimeRemaining <= 0f)
        {
            activated = true;
            StartCoroutine(SpawnProj());
        }
    }

    void Update()
    {
        // Delayed activation: wait until timer reaches the threshold
        if (!activated && activateAtTimeRemaining > 0f)
        {
            if (GameManager.Instance != null
                && GameManager.Instance.CurrentState == GameState.Playing
                && GameManager.Instance.TimeRemaining <= activateAtTimeRemaining)
            {
                activated = true;
                StartCoroutine(SpawnProj());
            }
        }

        if (!activated) return;

        if ((spawnCount == deadCount) && (spawnCount != 0))
        {
            spawnCount = 0;
            deadCount = 0;
            StartCoroutine(SpawnProj());
            
        }
    }
    private IEnumerator SpawnProj()
    {

        yield return new WaitForSeconds(spawnInterval);
        int randProj = Random.Range(0, projectilePrefabs.Length);
        int randSpawPoint = Random.Range(0, spawnPoints.Length);

        if (randProj == 1)
        {
            GameObject fish = Instantiate(projectilePrefabs[randProj], spawnPoints[randSpawPoint].position, transform.rotation);
            AssignSpawner(fish);
            StopCoroutine(SpawnProj());
            spawnCount++;

        }
        else
        {
            for (int i = 0; i < groupSize; i++)
            {
                yield return new WaitForSeconds(spawnInterval);
                GameObject fish = Instantiate(projectilePrefabs[randProj], spawnPoints[randSpawPoint].position, transform.rotation);
                AssignSpawner(fish);
                spawnCount++;
                
            }

            /*if (randProj == 3)
                {

                    Instantiate(poisonEffect, spawnPoints[randSpawPoint].position, transform.rotation);
                poisonEffect.SetActive(false);
                    
                }*/

            StopCoroutine(SpawnProj());

        }


    }

    private void AssignSpawner(GameObject fish)
    {
        EnemyHealth eh = fish.GetComponent<EnemyHealth>();
        if (eh != null)
            eh.spawner = this;
    }
}




