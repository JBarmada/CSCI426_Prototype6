using UnityEngine;

public abstract class PlayerAttackModuleBase : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] protected LayerMask enemyLayer = ~0;

    [Header("Debug")]
    [SerializeField] protected bool debugMode = false;

    protected PlayerMovement movement;
    protected ContactFilter2D enemyFilter;

    protected static readonly Collider2D[] HitBuffer = new Collider2D[40];

    public abstract AttackType AttackType { get; }
   

    protected virtual void Awake()
    {
        movement = GetComponentInParent<PlayerMovement>();

        enemyFilter = new ContactFilter2D();
        enemyFilter.useLayerMask = true;
        enemyFilter.layerMask = enemyLayer;
        enemyFilter.useTriggers = true;
     
    }

    public abstract void ExecuteNormal(GameObject primaryTarget);
    public abstract void ExecuteSpecial();

    protected int RollDamage()
    {
        return AttackSystem.Instance?.ExecuteAttack() ?? 0;
    }

    protected int RollMeatDrop()
    {
        return AttackSystem.Instance?.RollMeatDrop() ?? 1;
    }

    protected Vector2 GetFacingOffset(Vector2 localOffset)
    {
        float xSign = (movement != null && movement.direction == 0) ? -1f : 1f;
        return new Vector2(localOffset.x * xSign, localOffset.y);
    }

    protected void DamageAndRegisterMeat(Collider2D hit, int damage)
    {
        if (hit == null)
            return;

        EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
        if (enemy == null || enemy.IsDead)
            return;

        bool wasAlive = !enemy.IsDead;
        enemy.TakeDamage(damage);

        if (wasAlive && enemy.IsDead)
            RegisterMeatByTag(hit.tag);
    }

    protected void DamageAndRegisterMeat(GameObject target, int damage)
    {
        if (target == null)
            return;

        EnemyHealth enemy = target.GetComponent<EnemyHealth>();
        if (enemy == null || enemy.IsDead)
            return;

        bool wasAlive = !enemy.IsDead;
        enemy.TakeDamage(damage);

        if (wasAlive && enemy.IsDead)
            RegisterMeatByTag(target.tag);
    }

    protected void RegisterMeatByTag(string enemyTag)
    {
        AttackType? attackType = PlayerScript.GetEnemyAttackType(enemyTag);
        if (!attackType.HasValue)
            return;

        AttackUpgradeSystem.Instance?.RegisterMeat(attackType.Value, RollMeatDrop());
    }
}
