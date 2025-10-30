using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chasing,
        Attacking,
        Returning
    }

    
    [Header("Target")]
    public Transform target;

    [Header("Detection")]
    public float detectionRange = 5f;
    public float followRange = 8f;
    public float attackRange = 1.2f;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float stopDistance = 0.5f; 

    [Header("Avoidance (Separación de otros enemigos)")]
    public bool avoidOtherEnemies = true;   
    public float separationRadius = 1.0f;   
    public float separationForce = 0.6f;    

    [Header("Attack")]
    public float attackDamage = 10f;
    public float attackCooldown = 1.2f;
    private float lastAttackTime;

    

    
    private EnemyState currentState = EnemyState.Idle;
    private Vector3 startPosition;
    private Rigidbody2D rb;
    private Animator animator;
    

    
    private readonly Vector3 spriteScale = new Vector3(1.8f, 1.8f, 1.8f);
    private float lastMoveX = 0f;
    private float lastMoveY = -1f;

    
    
    

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        
        rb.mass = 3.0f;
        rb.drag = 5.0f;
        rb.angularDrag = 0.05f;
        rb.gravityScale = 0.0f;
        
        animator = GetComponent<Animator>();
        startPosition = transform.position;

        lastMoveY = -1f;
        transform.localScale = spriteScale;

        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        
        lastAttackTime = -attackCooldown;
    }

    void Update()
    {
        if (target == null) return;

        HandleStateLogic();

        UpdateAnimator();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    
    
    

    

    void HandleStateLogic()
    {
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        float distanceToStart = Vector2.Distance(transform.position, startPosition);
        bool canAttackDistance = distanceToTarget <= attackRange;

        switch (currentState)
        {
            case EnemyState.Idle:
                if (distanceToTarget <= detectionRange) currentState = EnemyState.Chasing;
                break;

            

            case EnemyState.Chasing:
                if (distanceToTarget > followRange) currentState = EnemyState.Returning;
                else if (canAttackDistance) currentState = EnemyState.Attacking;
                break;

            case EnemyState.Attacking:
                if (!canAttackDistance) { currentState = EnemyState.Chasing; break; }
                FaceTarget();
                if (Time.time >= lastAttackTime + attackCooldown) Attack();
                break;

            case EnemyState.Returning:
                if (distanceToTarget <= detectionRange) currentState = EnemyState.Chasing;
                else if (distanceToStart <= stopDistance + 0.1f)
                {
                    currentState = EnemyState.Idle;
                }
                break;
        }

        
        if (currentState == EnemyState.Idle)
        {
            if (rb.bodyType != RigidbodyType2D.Kinematic)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            if (rb.bodyType != RigidbodyType2D.Dynamic)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }
    }

    
    
    

    void HandleMovement()
    {
        Vector3 destination = transform.position;
        float distance = 0f;
        bool shouldMove = false;

        switch (currentState)
        {
            case EnemyState.Chasing:
                destination = target.position;
                distance = Vector2.Distance(transform.position, target.position);
                shouldMove = distance > stopDistance;
                break;

            case EnemyState.Returning:
                destination = startPosition;
                distance = Vector2.Distance(transform.position, startPosition);
                shouldMove = distance > stopDistance;
                break;

            

            case EnemyState.Idle:
            case EnemyState.Attacking:
                rb.velocity = Vector2.zero;
                return;
        }

        if (shouldMove)
        {
            MoveTowards(destination);
        }
    }

    void MoveTowards(Vector3 targetPosition)
    {
        Vector2 direction = (targetPosition - transform.position).normalized;

        
        if (avoidOtherEnemies)
        {
            Vector2 separation = Vector2.zero;
            Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, separationRadius);
            foreach (var col in neighbors)
            {
                if (col != null && col.gameObject != this.gameObject && col.GetComponent<EnemyAI>() != null)
                {
                    Vector2 away = (Vector2)(transform.position - col.transform.position);
                    float dist = Mathf.Max(away.magnitude, 0.01f);
                    separation += away.normalized / dist; 
                }
            }
            direction = (direction + separation * separationForce).normalized;
        }

        rb.velocity = direction * moveSpeed;

        
        if (Mathf.Abs(direction.x) > 0.01f)
        {
            if (direction.x < 0)
            {
                transform.localScale = new Vector3(-spriteScale.x, spriteScale.y, spriteScale.z);
            }
            else
            {
                transform.localScale = spriteScale;
            }
        }
    }

    
    void FaceTarget()
    {
        Vector2 direction = (target.position - transform.position).normalized;

        if (Mathf.Abs(direction.x) > 0.01f)
        {
            if (direction.x < 0)
            {
                transform.localScale = new Vector3(-spriteScale.x, spriteScale.y, spriteScale.z);
            }
            else
            {
                transform.localScale = spriteScale;
            }
        }

        
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            lastMoveX = Mathf.Sign(direction.x);
            lastMoveY = 0f;
        }
        else
        {
            lastMoveX = 0f;
            lastMoveY = Mathf.Sign(direction.y);
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        DealDamageToPlayer();
    }

    public void DealDamageToPlayer()
    {
        
        
        if (target == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        if (distanceToTarget <= attackRange)
        {
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage((int)attackDamage);
            }
        }
    }

    
    
    

    void UpdateAnimator()
    {
        if (animator == null) return;

        Vector2 velocity = rb.velocity;

        bool isMoving = (rb.velocity.sqrMagnitude > 0.01f);

        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsAttacking", currentState == EnemyState.Attacking);

        if (isMoving)
        {
            float currentMoveX = 0f;
            float currentMoveY = 0f;

            if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
            {
                currentMoveX = Mathf.Sign(velocity.x);
                currentMoveY = 0f;
            }
            else
            {
                currentMoveX = 0f;
                currentMoveY = Mathf.Sign(velocity.y);
            }

            animator.SetFloat("Move X", currentMoveX);
            animator.SetFloat("Move Y", currentMoveY);

            lastMoveX = currentMoveX;
            lastMoveY = currentMoveY;
        }
        else
        {
            
            animator.SetFloat("Move X", lastMoveX);
            animator.SetFloat("Move Y", lastMoveY);
        }
    }

    
    

    

    

    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public EnemyState GetCurrentState()
    {
        return currentState;
    }

    public void ForceState(EnemyState newState)
    {
        currentState = newState;
    }
}
