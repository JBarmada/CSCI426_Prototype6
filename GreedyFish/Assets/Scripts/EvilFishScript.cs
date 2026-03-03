
using GLTFast.Schema;
using Unity.VisualScripting;
using UnityEngine;

public class EvilFishScript : MonoBehaviour
{
    public GameObject player;
    public float speed;
    public float distance;

    private float _stunRemaining;

    public bool IsStunned => _stunRemaining > 0f;

     private Vector2 movement;
   
    //0 is left, 1 is right;
    public int direction = 0;
    public int oldDirection = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player");
        
    }

    // Update is called once per frame
    void Update()
    {
        if (movement.x < 0)
        {
            oldDirection = direction;
            direction = 0;
         
            
        }
        else if (movement.x > 0)
        {
            oldDirection = direction;
            direction = 1;
           
            
        }

        if (oldDirection != direction)
        {
            //flip image
           /* if (oldDirection == 1)
            {
                Vector3 dir = new Vector3(-1, 1, 1);
                  
                
                

            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);



            }*/
            
        }
        
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.Normalize();









        if (_stunRemaining > 0f)
        {
            _stunRemaining -= Time.deltaTime;
            if (_stunRemaining < 0f)
                _stunRemaining = 0f;
            return;
        }

        if (player == null)
            return;

        distance = Vector2.Distance(transform.position, player.transform.position);
       // Vector2 direction = player.transform.position - transform.position;

        transform.position = Vector2.MoveTowards(this.transform.position, player.transform.position, speed * Time.deltaTime);
        
    }

    public void ApplyStun(float duration)
    {
        if (duration <= 0f)
            return;

        if (duration > _stunRemaining)
            _stunRemaining = duration;
    }
}
