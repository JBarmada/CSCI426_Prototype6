using UnityEngine;

public class PoisonCloudEmitter : MonoBehaviour
{
    private LayerMask enemyLayer = ~0;
    private ContactFilter2D enemyFilter;
    private float cloudRadius = 1.5f;
    private float tickInterval = 0.5f;
    private int tickDamage = 1;

    private float remainingDuration;
    private float tickTimer;
    private GameObject owner;

    private static readonly Collider2D[] HitBuffer = new Collider2D[30];

    private void Awake()
    {
        enemyFilter = new ContactFilter2D();
        enemyFilter.useLayerMask = true;
        enemyFilter.useTriggers = true;
        enemyFilter.layerMask = enemyLayer;
    }

    private void Update()
    {
        if (remainingDuration <= 0f)
            return;

        remainingDuration -= Time.deltaTime;
        tickTimer -= Time.deltaTime;

        if (tickTimer > 0f)
            return;

        tickTimer = tickInterval;
        EmitTickDamage();
    }

    public void Configure(LayerMask layer, float radius, float interval, int damage)
    {
        enemyLayer = layer;
        cloudRadius = Mathf.Max(0.1f, radius);
        tickInterval = Mathf.Max(0.05f, interval);
        tickDamage = Mathf.Max(1, damage);

        enemyFilter.useLayerMask = true;
        enemyFilter.useTriggers = true;
        enemyFilter.layerMask = enemyLayer;
    }

    public void Activate(float duration, GameObject cloudOwner)
    {
        owner = cloudOwner;
        remainingDuration = Mathf.Max(remainingDuration, duration);
        if (tickTimer <= 0f)
            tickTimer = tickInterval;
    }

    private void EmitTickDamage()
    {
        int count = Physics2D.OverlapCircle(transform.position, cloudRadius, enemyFilter, HitBuffer);

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = HitBuffer[i];
            if (hit == null)
                continue;

            GameObject hitObject = hit.gameObject;
            if (owner != null && hitObject == owner)
                continue;

            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy == null || enemy.IsDead)
                continue;

            bool wasAlive = !enemy.IsDead;
            enemy.TakeDamage(tickDamage);

            if (wasAlive && enemy.IsDead)
            {
                AttackType? type = PlayerScript.GetEnemyAttackType(hit.tag);
                if (type.HasValue)
                {
                    int meatCount = AttackSystem.Instance?.RollMeatDrop() ?? 1;
                    AttackUpgradeSystem.Instance?.RegisterMeat(type.Value, meatCount);
                }
            }
        }
    }
}
