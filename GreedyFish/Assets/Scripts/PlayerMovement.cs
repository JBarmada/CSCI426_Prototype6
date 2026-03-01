using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;



    

    private Rigidbody2D rb;
    private Vector2 movement;
    


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
       
    }

    private void Update()
    {

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
