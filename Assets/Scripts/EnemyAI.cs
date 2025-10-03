using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // === ENUMS ===
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chasing,
        Attacking,
        Returning
    }

    // === PARÁMETROS DE CONFIGURACIÓN ===
    [Header("Target")]
    public Transform target;

    [Header("Detection")]
    public float detectionRange = 5f;
    public float followRange = 8f;
    public float attackRange = 1.5f;
    public float crouchDetectionMultiplier = 0.5f;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float stopDistance = 0.5f; // Distancia para detenerse del objetivo o punto

    [Header("Attack")]
    public float attackDamage = 10f;
    // CORRECCIÓN 1: Cooldown de ataque cambiado a 5 segundos.
    public float attackCooldown = 5f;
    private float lastAttackTime;

    [Header("Patrol")]
    public bool usePatrol = false;
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;

    [Header("Debug")]
    public bool enableLogs = false;
    public bool showGizmos = true;
    public float gizmoCenterYOffset = 0.5f;

    // === ESTADO INTERNO ===
    private EnemyState currentState = EnemyState.Idle;
    private Vector3 startPosition;
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerMovement playerMovement;
    private float originalDetectionRange;
    private int currentPatrolIndex = 0;
    private float patrolWaitTimer;
    private bool isWaiting = false;

    // Variables de animación y Flip (Escala 1.8 asumida)
    private readonly Vector3 spriteScale = new Vector3(1.8f, 1.8f, 1.8f);
    private float lastMoveX = 0f;
    private float lastMoveY = -1f;

    // =================================================================
    // START & UPDATE
    // =================================================================

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startPosition = transform.position;
        originalDetectionRange = detectionRange;

        lastMoveY = -1f;
        transform.localScale = spriteScale;

        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if (target != null)
        {
            playerMovement = target.GetComponent<PlayerMovement>();
        }
    }

    void Update()
    {
        if (target == null) return;

        UpdateDetectionRange();

        HandleStateLogic();

        UpdateAnimator();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    // =================================================================
    // LÓGICA DE ESTADOS Y TRANSICIONES (Update)
    // =================================================================

    void UpdateDetectionRange()
    {
        if (playerMovement != null)
        {
            detectionRange = playerMovement.IsCrouching()
                ? originalDetectionRange * crouchDetectionMultiplier
                : originalDetectionRange;
        }
    }

    void HandleStateLogic()
    {
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        float distanceToStart = Vector2.Distance(transform.position, startPosition);

        switch (currentState)
        {
            case EnemyState.Idle:
                if (distanceToTarget <= detectionRange) currentState = EnemyState.Chasing;
                else if (usePatrol && patrolPoints.Length > 0) currentState = EnemyState.Patrol;
                break;

            case EnemyState.Patrol:
                if (distanceToTarget <= detectionRange) currentState = EnemyState.Chasing;
                else if (patrolPoints.Length == 0) currentState = EnemyState.Idle;

                if (isWaiting)
                {
                    patrolWaitTimer -= Time.deltaTime;
                    if (patrolWaitTimer <= 0)
                    {
                        isWaiting = false;
                        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                    }
                }
                break;

            case EnemyState.Chasing:
                if (distanceToTarget > followRange) currentState = EnemyState.Returning;
                else if (distanceToTarget <= attackRange) currentState = EnemyState.Attacking;
                break;

            case EnemyState.Attacking:
                if (distanceToTarget > attackRange) currentState = EnemyState.Chasing;
                // CORRECCIÓN 2: Llama a FaceTarget para la animación de ataque direccional.
                FaceTarget();
                if (Time.time >= lastAttackTime + attackCooldown) Attack();
                break;

            case EnemyState.Returning:
                if (distanceToTarget <= detectionRange) currentState = EnemyState.Chasing;
                else if (distanceToStart <= stopDistance + 0.1f)
                {
                    if (usePatrol && patrolPoints.Length > 0) currentState = EnemyState.Patrol;
                    else currentState = EnemyState.Idle;
                }
                break;
        }

        // MANEJO DEL RIGIDBODY (Anti-Parpadeo)
        if (currentState == EnemyState.Idle || (currentState == EnemyState.Patrol && isWaiting))
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

    // =================================================================
    // MOVIMIENTO FÍSICO (FixedUpdate)
    // =================================================================

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

            case EnemyState.Patrol:
                if (patrolPoints.Length > 0 && !isWaiting)
                {
                    Transform currentPoint = patrolPoints[currentPatrolIndex];
                    destination = currentPoint.position;
                    distance = Vector2.Distance(transform.position, destination);

                    if (distance <= stopDistance)
                    {
                        rb.velocity = Vector2.zero;
                        isWaiting = true;
                        patrolWaitTimer = patrolWaitTime;
                    }
                    else
                    {
                        shouldMove = true;
                    }
                }
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
        rb.velocity = direction * moveSpeed;

        // El Flip se maneja aquí al moverse
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

    // NUEVA FUNCIÓN: Mantiene el Flip en la dirección del objetivo sin movimiento
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

        // Además, actualizamos lastMoveX/Y para que el Idle direccional funcione correctamente después del ataque
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
        if (enableLogs) Debug.Log($"{gameObject.name}: Atacando al jugador");
        lastAttackTime = Time.time;
        // Llama a DealDamageToPlayer() en el momento deseado de la animación de ataque.
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
                if (enableLogs) Debug.Log($"{gameObject.name}: Daño aplicado al jugador");
            }
        }
    }

    // =================================================================
    // ANIMATOR (Anti-Parpadeo y Priorización de Ejes)
    // =================================================================

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
            // Si está quieto, usa la última dirección guardada para el Idle
            animator.SetFloat("Move X", lastMoveX);
            animator.SetFloat("Move Y", lastMoveY);
        }
    }

    // =================================================================
    // GIZMOS Y COLISIONES (Con Offset de Centrado)
    // =================================================================

    void OnDrawGizmosSelected()
    {
        if (showGizmos)
        {
            Vector3 centerPosition = transform.position + new Vector3(0, gizmoCenterYOffset, 0);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(centerPosition, detectionRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(centerPosition, followRange);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(centerPosition, attackRange);
        }

        if (usePatrol && patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(patrolPoints[i].position, 0.5f);

                    if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                    else if (i == patrolPoints.Length - 1 && patrolPoints[0] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
                    }
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            currentState = EnemyState.Attacking;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            currentState = EnemyState.Attacking;
        }
    }

    // Funciones públicas
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