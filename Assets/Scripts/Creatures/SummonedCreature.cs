using UnityEngine;

public abstract class SummonedCreature : MonoBehaviour
{
    public enum CreatureType { Angel, Demon }

    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int attackDamage = 25;
    [SerializeField] protected float attackRange = 6f;
    [SerializeField] protected float moveSpeed = 3f;

    [SerializeField] protected Animator anim;

    protected int currentHealth;
    protected CreatureType creatureType;
    protected Vector2 lastMoveDir = Vector2.down;
    protected bool isMoving = false;
    protected bool isAttacking = false;

    protected readonly string PARAM_H = "MoveX";
    protected readonly string PARAM_V = "MoveY";
    protected readonly string PARAM_MOV = "IsMoving";
    protected readonly string PARAM_ATK = "IsAttacking";

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        if (anim == null) anim = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        if (anim != null)
        {
            anim.SetFloat(PARAM_H, 0f);
            anim.SetFloat(PARAM_V, -1f);
            anim.SetBool(PARAM_MOV, false);
            anim.SetBool(PARAM_ATK, false);
        }
    }

    protected virtual void Update()
    {
        HandleCombat();
        UpdateBaseAnimations();
    }

    protected virtual void UpdateBaseAnimations()
    {
        if (anim == null) return;

        if (lastMoveDir.sqrMagnitude > 0.01f)
        {
            anim.SetFloat(PARAM_H, lastMoveDir.x);
            anim.SetFloat(PARAM_V, lastMoveDir.y);
        }

        anim.SetBool(PARAM_MOV, isMoving);
        anim.SetBool(PARAM_ATK, isAttacking);
    }

    protected void SetMoveDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.01f)
        {
            lastMoveDir = direction.normalized;
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    protected void StartAttackAnimation() => isAttacking = true;
    protected void StopAttackAnimation() => isAttacking = false;

    public void OnAttackAnimationEnd() => StopAttackAnimation();

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die() => Destroy(gameObject);

    protected abstract void HandleCombat();

    protected virtual void Attack(GameObject target) { }

    public void SetCreatureType(CreatureType type) => creatureType = type;

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
