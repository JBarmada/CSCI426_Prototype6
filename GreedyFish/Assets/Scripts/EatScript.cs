using UnityEngine;

public class EatScript : MonoBehaviour
{
    public PlayerHealth player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Tail").GetComponent<PlayerHealth>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Food"))
        {
            if (player.CurrentHealth < 100)
            {
                player.Heal(5);

            }

            Destroy(collision.gameObject);
        }
    }
}
