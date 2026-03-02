using UnityEngine;
using System.Collections;



public class FishSpawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject[] projectilePrefabs;

    [SerializeField] private float spawnInterval = 1.0f;
    [Tooltip("Number of fish to spawn per group")]
    [SerializeField] private int groupSize = 1;

    void Start()
    {
        StartCoroutine(SpawnProj());
    }

    private IEnumerator SpawnProj()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            for (int i = 0; i < groupSize; i++)
            {
                int randProj = Random.Range(0, projectilePrefabs.Length);
                int randSpawPoint = Random.Range(0, spawnPoints.Length);
                Instantiate(projectilePrefabs[randProj], spawnPoints[randSpawPoint].position, transform.rotation);
            }
        }
    }
}




