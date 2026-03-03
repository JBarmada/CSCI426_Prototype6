
using GLTFast.Schema;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
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

    public Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player");
        rb = GetComponent<Rigidbody2D>();
        
    }

    // Update is called once per frame
    void Update()
    {
        /*if (_stunRemaining > 0f)
        {
            /*transform.position = transform.position;
            _stunRemaining -= Time.deltaTime;
            if (_stunRemaining < 0f)
                _stunRemaining = 0f;
            return;
        }*/

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

    public void OnCollision2D(Collision2D collision)
    {
        //if(collision.gameObject.CompareTag("Player"))
        //StartCoroutine(ApplyStun(4));
        
    }

    public IEnumerator ApplyStun(float duration)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            //float strength = curve.Evaluate(elapsedTime / duration);
            rb.constraints = RigidbodyConstraints2D.FreezeAll;

            yield return null;
        }

       // transform.position = startPosition;




       /* if (duration <= 0f)
            return;

        if (duration > _stunRemaining)
            _stunRemaining = duration;*/
    }
}
