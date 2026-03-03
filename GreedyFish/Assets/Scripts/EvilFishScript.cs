
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
