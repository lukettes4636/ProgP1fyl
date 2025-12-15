using UnityEngine;
using System;

public abstract class SummonedCreature : MonoBehaviour
{
    // Evento que se dispara cuando la criatura es destruida
    public event Action OnCreatureDestroyed;
    
    // Método público para notificar destrucción desde PlayerSummoner
    public void NotifyDestruction()
    {
        OnCreatureDestroyed?.Invoke();
    }
    [Header("Configuración Base")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int attackDamage = 25;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float attackCooldown = 1.5f;
    [SerializeField] protected float moveSpeed = 3f;
    
    [Header("Comportamiento")]
    [SerializeField] protected float followDistance = 5f;
    [SerializeField] protected float maxDistanceFromPlayer = 25f; // Aumentado de 10 a 25
    [SerializeField] protected float teleportDistance = 30f; // Distancia para teletransportarse
    [SerializeField] protected float fastFollowMultiplier = 2.5f; // Multiplicador de velocidad cuando está lejos
    
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
            Debug.LogWarning("Jugador no encontrado. El invocado se destruirá en 30 segundos.");
            Destroy(gameObject, 30f);
        }
    }
    
    protected virtual void Start()
    {
        // Override en clases hijas
    }
    
    protected virtual void Update()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Si está demasiado lejos, teletransportarse al jugador
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
            // Si está lejos, usar velocidad aumentada para alcanzar rápidamente
            float currentMoveSpeed = moveSpeed;
            if (distanceToPlayer > maxDistanceFromPlayer)
            {
                currentMoveSpeed *= fastFollowMultiplier; // Velocidad aumentada cuando está lejos
            }
            
            Vector2 movement = directionToPlayer * currentMoveSpeed * Time.deltaTime;
            transform.Translate(movement);
        }
    }
    
    protected virtual void HandleCombat()
    {
        // Override en clases hijas para comportamiento específico
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
        
        // Teletransportarse a una posición cerca del jugador
        Vector2 teleportPosition = (Vector2)playerTransform.position + UnityEngine.Random.insideUnitCircle * 2f;
        transform.position = teleportPosition;
        
        Debug.Log($"🌀 {creatureType} se teletransportó al jugador (estaba demasiado lejos)");
        
        // Agregar efecto visual opcional
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
            
            Debug.Log($"{creatureType} atacó a {target.name} por {attackDamage} daño");
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
        
        Debug.Log($"{creatureType} ha muerto");
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        Destroy(gameObject, 1f);
    }
    
    protected virtual void OnDestroy()
    {
        // Disparar el evento cuando la criatura es destruida
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
    
    // Métodos de utilidad
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public CreatureType GetCreatureType() => creatureType;
    public bool IsAlive() => currentHealth > 0;
}