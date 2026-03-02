using UnityEngine;
using System.Collections;



public class FishSpawner : MonoBehaviour
{


   public Transform[] spawnPoints;
    public GameObject [] projectilePrefabs;
   // private float spawnInterval = 1.0f;
    void Start()
    {
        int randProj = Random.Range(0, projectilePrefabs.Length);
        int randSpawPoint = Random.Range(0, spawnPoints.Length);
        Instantiate(projectilePrefabs[randProj], spawnPoints[randSpawPoint].position, transform.rotation);
      // StartCoroutine(spawnProj(spawnInterval));
    }

    /*private IEnumerator spawnProj(float interval)
    {
        yield return new WaitForSeconds(interval);
       
        StartCoroutine(spawnProj(spawnInterval));
        
    }*/


    // Update is called once per frame
    void Update()
    {

        
            
        
    }
}




