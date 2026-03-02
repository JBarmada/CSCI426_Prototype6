using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    public BuoyancyEffector2D ocean;
    


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
       
    }

    private void Update()
    {
        if (movement.x < 0){
            ocean.flowMagnitude = -30;
        }
        else if(movement.x > 0){
            ocean.flowMagnitude = 30;
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
