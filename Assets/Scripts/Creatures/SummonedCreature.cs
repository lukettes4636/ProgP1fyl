using UnityEngine;
using System;

public abstract class SummonedCreature : MonoBehaviour
{
    public event Action OnCreatureDestroyed;
    
    public void NotifyDestruction()
    {
        OnCreatureDestroyed?.Invoke();
    }
    
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int attackDamage = 25;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float attackCooldown = 1.5f;
    [SerializeField] protected float moveSpeed = 3f;
    
    [SerializeField] protected float followDistance = 5f;
    [SerializeField] protected float maxDistanceFromPlayer = 25f;
    [SerializeField] protected float teleportDistance = 30f;
    [SerializeField] protected float fastFollowMultiplier = 2.5f;
    
    protected int currentHealth;
    protected float lastAttackTime;
    protected Transform playerTransform;
    protected EnemyAI enemyAI;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    
    public enum CreatureType { Angel, Demon }
    [SerializeField] protected CreatureType creatureType;
    
    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        enemyAI = GetComponent<EnemyAI>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (playerTransform == null)
        {
            Destroy(gameObject, 30f);
        }
    }
    
    protected virtual void Start()
    {
    }
    
    protected virtual void Update()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer > teleportDistance)
        {
            TeleportToPlayer();
            return;
        }
        
        HandleMovement();
        HandleCombat();
        UpdateAnimation();
    }
    
    protected virtual void HandleMovement()
    {
        if (enemyAI != null)
        {
            return;
        }
        
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer > followDistance)
        {
            float currentMoveSpeed = moveSpeed;
            if (distanceToPlayer > maxDistanceFromPlayer)
            {
                currentMoveSpeed *= fastFollowMultiplier;
            }
            
            Vector2 movement = directionToPlayer * currentMoveSpeed * Time.deltaTime;
            transform.Translate(movement);
        }
    }
    
    protected virtual void HandleCombat()
    {
        BasicMeleeAttack();
    }
    
    protected void BasicMeleeAttack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange);
        
        foreach (Collider2D enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    Attack(enemy.gameObject);
                    lastAttackTime = Time.time;
                    break;
                }
            }
        }
    }
    
    protected virtual void TeleportToPlayer()
    {
        if (playerTransform == null) return;
        
        Vector2 teleportPosition = (Vector2)playerTransform.position + UnityEngine.Random.insideUnitCircle * 2f;
        transform.position = teleportPosition;
        
        if (animator != null)
        {
            animator.SetTrigger("Teleport");
        }
    }
    
    protected virtual void Attack(GameObject target)
    {
        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(attackDamage);
            
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }
    }
    
    protected virtual void UpdateAnimation()
    {
        if (animator == null || spriteRenderer == null) return;
        
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        
        animator.SetFloat("MoveX", directionToPlayer.x);
        animator.SetFloat("MoveY", directionToPlayer.y);
        animator.SetFloat("LastMoveX", directionToPlayer.x);
        animator.SetFloat("LastMoveY", directionToPlayer.y);
        
        spriteRenderer.flipX = directionToPlayer.x < 0;
    }
    
    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    protected virtual void Die()
    {
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        Destroy(gameObject, 1f);
    }
    
    protected virtual void OnDestroy()
    {
        OnCreatureDestroyed?.Invoke();
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, followDistance);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistanceFromPlayer);
    }
    
    public virtual void SetAsAngel()
    {
        creatureType = CreatureType.Angel;
        gameObject.name = "Angel_Invocado";
    }
    
    public virtual void SetAsDemon()
    {
        creatureType = CreatureType.Demon;
        gameObject.name = "Diablo_Invocado";
    }
    
    protected void SetCreatureType(CreatureType type)
    {
        creatureType = type;
    }
    
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public CreatureType GetCreatureType() => creatureType;
    public bool IsAlive() => currentHealth > 0;
}