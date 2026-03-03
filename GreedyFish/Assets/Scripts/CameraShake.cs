using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float duration = 1f;
    public bool start = false;
    public AnimationCurve curve;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
       

    }

    // Update is called once per frame
    void Update()
    {
        SecondShake();


    }

    IEnumerator Shaking()
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / duration);
            transform.position = startPosition + Random.insideUnitSphere * strength;
            yield return null;
        }

        transform.position = startPosition;
    }

    public void SecondShake()
    {
        if (start == true)
        {
            start = false;
             StartCoroutine(Shaking());
            
        }
        
           
    
        
    }

   
}

