
using GLTFast.Schema;
using Unity.VisualScripting;
using UnityEngine;

public class EvilFishScript : MonoBehaviour
{
    public GameObject player;
    public float speed;
     public float distance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player");
        
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector2.Distance(transform.position, player.transform.position);
       // Vector2 direction = player.transform.position - transform.position;

        transform.position = Vector2.MoveTowards(this.transform.position, player.transform.position, speed * Time.deltaTime);
        
    }
}
