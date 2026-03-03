
using GLTFast.Schema;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

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

        float old = transform.position.x;

        transform.position = Vector2.MoveTowards(this.transform.position, player.transform.position, speed * Time.deltaTime);

        float newPos = transform.position.x;
         float scaleVal = transform.localScale.x;

        if (old > newPos)
        {

            if (scaleVal > 0)
            {
                transform.localScale = new Vector3(-1 * transform.localScale.x, transform.localScale.y, transform.localScale.z);

            }



        }
        else if (newPos > old)
        {
            if (scaleVal < 0)
           {
                transform.localScale = new Vector3(-1 * transform.localScale.x, transform.localScale.y, transform.localScale.z);

            }

        }
        
    }

    public void ApplyStun(float duration)
    {
        if (duration <= 0f)
            return;

        if (duration > _stunRemaining)
            _stunRemaining = duration;
    }
}
