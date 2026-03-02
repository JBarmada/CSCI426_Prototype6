using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    public BuoyancyEffector2D ocean;
    //0 is left, 1 is right;
    public int direction = 0;
    public int oldDirection = 0;
    


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

    }

    private void Update()
    {
        if (movement.x < 0)
        {
            oldDirection = direction;
            direction = 0;
            ocean.flowMagnitude = -60;
            
        }
        else if (movement.x > 0)
        {
            oldDirection = direction;
            direction = 1;
            ocean.flowMagnitude = 60;
            
        }

        if (oldDirection != direction)
        {
            //flip image
            if (oldDirection == 1)
            {
                transform.localScale = new Vector3(1, 1, 1);
                

            }
            else
            {
                transform.localScale = new Vector3(-1, 1, 1);


            }
            
        }
        
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.Normalize();
        


    }
    

    private void FixedUpdate()
    {
        Vector2 targetVelocity = movement * moveSpeed;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, 0.2f);
    }
}
