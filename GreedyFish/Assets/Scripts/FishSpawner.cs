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
    public int deadCount = 0;
    public int spawnCount = 0;

    void Start()
    {
        StartCoroutine(SpawnProj());
    }
    void Update()
    {
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

            Instantiate(projectilePrefabs[randProj], spawnPoints[randSpawPoint].position, transform.rotation);
            StopCoroutine(SpawnProj());
            spawnCount++;

        }
        else
        {
            for (int i = 0; i < groupSize; i++)
            {
                yield return new WaitForSeconds(spawnInterval);
                Instantiate(projectilePrefabs[randProj], spawnPoints[randSpawPoint].position, transform.rotation);
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
}




