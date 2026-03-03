using UnityEngine;

/// <summary>
/// Spawned at a poisoned enemy's position during Poison Special.
/// Periodically checks for enemies in radius and applies/refreshes poison stacks.
/// The spawning code handles GO cleanup via timed Destroy.
/// </summary>
public class PoisonCloudEmitter : MonoBehaviour
{
    private ContactFilter2D _enemyFilter;
    private float _cloudRadius = 1.5f;
    private float _tickInterval = 0.5f;
    private int _stacksToApply = 1;
    private float _stackDuration;
    private float _damagePerTick;
    private float _tick1, _tick2, _tick3;

    private float _remainingDuration;
    private float _tickTimer;

    private static readonly Collider2D[] CloudHitBuffer = new Collider2D[30];

    public void Configure(LayerMask layer, float radius, float tickInterval,
        int stacksToApply, float stackDuration, float damagePerTick,
        float tick1, float tick2, float tick3)
    {
        _cloudRadius = Mathf.Max(0.1f, radius);
        _tickInterval = Mathf.Max(0.05f, tickInterval);
        _stacksToApply = Mathf.Clamp(stacksToApply, 1, PoisonStatus.MaxStacks);
        _stackDuration = stackDuration;
        _damagePerTick = damagePerTick;
        _tick1 = tick1; _tick2 = tick2; _tick3 = tick3;

        _enemyFilter = new ContactFilter2D();
        _enemyFilter.useLayerMask = true;
        _enemyFilter.useTriggers = true;
        _enemyFilter.layerMask = layer;
    }

    public void Activate(float duration)
    {
        _remainingDuration = duration;
        _tickTimer = 0f; // Tick immediately on first frame
    }

    private void Update()
    {
        if (_remainingDuration <= 0f)
        {
            enabled = false; // Stop ticking; timed Destroy handles cleanup
            return;
        }

        _remainingDuration -= Time.deltaTime;
        _tickTimer -= Time.deltaTime;

        if (_tickTimer <= 0f)
        {
            _tickTimer = _tickInterval;
            SpreadPoison();
        }
    }

    private void SpreadPoison()
    {
        int count = Physics2D.OverlapCircle(transform.position, _cloudRadius, _enemyFilter, CloudHitBuffer);

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = CloudHitBuffer[i];
            if (hit == null) continue;

            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy == null || enemy.IsDead) continue;

            PoisonStatus status = hit.GetComponent<PoisonStatus>();
            if (status == null)
                status = hit.gameObject.AddComponent<PoisonStatus>();

            // Cloud applies stacks with refresh
            status.ApplyStacksWithRefresh(_stacksToApply, _stackDuration, _damagePerTick,
                _tick1, _tick2, _tick3);
        }
    }
}
