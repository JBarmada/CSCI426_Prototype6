using UnityEngine;

public class PoisonStatus : MonoBehaviour
{
    [SerializeField] private float remainingDuration;

    public bool IsActive => remainingDuration > 0f;

    private void Update()
    {
        if (remainingDuration <= 0f)
            return;

        remainingDuration -= Time.deltaTime;
        if (remainingDuration < 0f)
            remainingDuration = 0f;
    }

    public void Apply(float duration)
    {
        if (duration <= 0f)
            return;

        if (duration > remainingDuration)
            remainingDuration = duration;
    }
}
