using System.Collections;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private int health;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = 100;

    }

    // Update is called once per frame
    void Update()
    {
        

    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        health -= 5;
        Debug.Log("Health " + health);
        if (collision.gameObject.CompareTag("PiercingFish"))
        {
            health -= 5;
            Debug.Log("Health " + health);

        }
        else if (collision.gameObject.CompareTag("JawFish"))
        {
            health -= 20;
            Debug.Log(health);

        }
        else if (collision.gameObject.CompareTag("EelFish"))
        {
            health -= 10;
            Debug.Log(health);

        }
        else if (collision.gameObject.CompareTag("UglyFish"))
        {
            Poison();

        }

    }

    IEnumerator Poison()
    {
        for (int i = 0; i < 20; i++)
        {
            health -= 2;
            Debug.Log(health);
            yield return new WaitForSeconds(0.5f);
        }
    }
}
