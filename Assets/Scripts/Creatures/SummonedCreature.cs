using UnityEngine;

public abstract class SummonedCreature : MonoBehaviour
{
    public enum CreatureType { Angel, Demon }

    [Header("Stats")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int attackDamage = 25;
    [SerializeField] protected float attackRange = 6f;
    [SerializeField] protected float moveSpeed = 3f;

    [Header("Animation")]
    [SerializeField] protected Animator animator;

    protected int currentHealth;
    protected CreatureType creatureType;
    protected Vector2 lastMovementDirection = Vector2.down;
    protected bool isMoving = false;
    protected bool isAttacking = false;

    // Animation parameter names
    protected readonly string ANIM_HORIZONTAL = "Horizontal";
    protected readonly string ANIM_VERTICAL = "Vertical";
    protected readonly string ANIM_IS_MOVING = "IsMoving";
    protected readonly string ANIM_IS_ATTACKING = "IsAttacking";

    protected virtual void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    protected virtual void Start()
    {
        InitializeAnimator();
    }

    protected virtual void Update()
    {
        HandleCombat();
        UpdateAnimationParameters();
    }

    protected virtual void InitializeAnimator()
    {
        if (animator != null)
        {
            animator.SetFloat(ANIM_HORIZONTAL, 0f);
            animator.SetFloat(ANIM_VERTICAL, -1f);
            animator.SetBool(ANIM_IS_MOVING, false);
            animator.SetBool(ANIM_IS_ATTACKING, false);
        }
    }

    protected virtual void UpdateAnimationParameters()
    {
        if (animator == null) return;

        // Actualizar dirección
        if (lastMovementDirection.sqrMagnitude > 0.01f)
        {
            animator.SetFloat(ANIM_HORIZONTAL, lastMovementDirection.x);
            animator.SetFloat(ANIM_VERTICAL, lastMovementDirection.y);
        }

        // Actualizar estado de movimiento
        animator.SetBool(ANIM_IS_MOVING, isMoving);

        // Actualizar estado de ataque
        animator.SetBool(ANIM_IS_ATTACKING, isAttacking);
    }

    protected void SetMovementDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.01f)
        {
            lastMovementDirection = direction.normalized;
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    protected void StartAttackAnimation()
    {
        isAttacking = true;
    }

    protected void StopAttackAnimation()
    {
        isAttacking = false;
    }

    // Este método se llama desde un Animation Event al final de la animación de ataque
    public void OnAttackAnimationEnd()
    {
        StopAttackAnimation();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    protected abstract void HandleCombat();

    protected virtual void Attack(GameObject target)
    {
    }

    public void SetCreatureType(CreatureType type)
    {
        creatureType = type;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}