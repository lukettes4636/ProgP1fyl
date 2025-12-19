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

    public enum EnemyAttackType
    {
        MeleeOnly,
        RangedOnly,
        Both
    }

    public Transform target;
    
    public float detectionRange = 5f;
    public float chaseRange = 8f;
    public float attackRange = 1.2f;

    public float moveSpeed = 2f;
    public float stoppingDistance = 0.5f;

    public bool avoidAllies = true;
    public float separationRadius = 1.0f;
    public float separationForce = 0.6f;

    public float attackDamage = 10f;
    public float attackCooldown = 1.2f;

    [Header("Ranged")]
    public EnemyAttackType attackType = EnemyAttackType.Both;
    public GameObject arrowPrefab;
    public float rangedRange = 5f;
    public float arrowCooldown = 2f;
    public Transform arrowSpawnPoint;
    [SerializeField] private AudioClip shootSound;
    private AudioSource audioSource;
    private float lastAttackTime;
    private float lastArrowTime;

    private EnemyState currentState = EnemyState.Idle;
    private Vector3 initialPosition;
    private Rigidbody2D rb2d;
    private Animator animator;

    private readonly Vector3 spriteScale = new Vector3(1.8f, 1.8f, 1.8f);
    private float prevMoveX = 0f;
    private float prevMoveY = -1f;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.mass = 1000f;
        rb2d.drag = 5.0f;
        rb2d.angularDrag = 0.05f;
        rb2d.gravityScale = 0.0f;
        rb2d.freezeRotation = true;

        animator = GetComponent<Animator>();
        initialPosition = transform.position;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        prevMoveY = -1f;
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
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        Move();
    }

    void HandleStateLogic()
    {
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        float distanceToStart = Vector2.Distance(transform.position, initialPosition);
        
        bool canMelee = (attackType == EnemyAttackType.MeleeOnly || attackType == EnemyAttackType.Both) && distanceToTarget <= attackRange;
        bool canShoot = (attackType == EnemyAttackType.RangedOnly || attackType == EnemyAttackType.Both) && arrowPrefab != null && distanceToTarget <= rangedRange;
        
        bool attackAvailable = canMelee || canShoot;

        switch (currentState)
        {
            case EnemyState.Idle:
                if (distanceToTarget <= detectionRange)
                    currentState = EnemyState.Chasing;
                break;

            case EnemyState.Chasing:
                if (distanceToTarget > chaseRange)
                    currentState = EnemyState.Returning;
                else if (attackAvailable)
                    currentState = EnemyState.Attacking;
                break;

            case EnemyState.Attacking:
                if (!canMelee && !canShoot)
                {
                    currentState = EnemyState.Chasing;
                    break;
                }

                LookAtTarget();

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    ExecuteAttack();
                }
                break;

            case EnemyState.Returning:
                if (distanceToTarget <= detectionRange)
                    currentState = EnemyState.Chasing;
                else if (distanceToStart <= stoppingDistance + 0.1f)
                {
                    currentState = EnemyState.Idle;
                }
                break;
        }

        if (currentState == EnemyState.Idle)
        {
            if (rb2d.bodyType != RigidbodyType2D.Kinematic)
            {
                rb2d.bodyType = RigidbodyType2D.Kinematic;
                rb2d.velocity = Vector2.zero;
            }
        }
        else
        {
            if (rb2d.bodyType != RigidbodyType2D.Dynamic)
            {
                rb2d.bodyType = RigidbodyType2D.Dynamic;
            }
        }
    }

    void Move()
    {
        if (currentState == EnemyState.Attacking)
        {
            if (attackType == EnemyAttackType.RangedOnly && target != null)
            {
                float distanceToTarget = Vector2.Distance(transform.position, target.position);
                float optimalDistance = rangedRange * 0.7f;
                
                if (distanceToTarget < optimalDistance)
                {
                    Vector2 awayDirection = (transform.position - target.position).normalized;
                    rb2d.velocity = awayDirection * moveSpeed * 0.5f;
                    return;
                }
            }
            
            rb2d.velocity = Vector2.zero;
            return;
        }

        Vector3 destination = transform.position;
        float distance = 0f;
        bool shouldMove = false;

        switch (currentState)
        {
            case EnemyState.Chasing:
                destination = target.position;
                distance = Vector2.Distance(transform.position, target.position);
                shouldMove = distance > stoppingDistance;
                break;

            case EnemyState.Returning:
                destination = initialPosition;
                distance = Vector2.Distance(transform.position, initialPosition);
                shouldMove = distance > stoppingDistance;
                break;

            case EnemyState.Idle:
                rb2d.velocity = Vector2.zero;
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

        if (avoidAllies)
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

        rb2d.velocity = direction * moveSpeed;

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

    void LookAtTarget()
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
            prevMoveX = Mathf.Sign(direction.x);
            prevMoveY = 0f;
        }
        else
        {
            prevMoveX = 0f;
            prevMoveY = Mathf.Sign(direction.y);
        }
    }

    void ExecuteAttack()
    {
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        
        bool canMelee = (attackType == EnemyAttackType.MeleeOnly || attackType == EnemyAttackType.Both) && distanceToTarget <= attackRange;
        bool canShoot = (attackType == EnemyAttackType.RangedOnly || attackType == EnemyAttackType.Both) && arrowPrefab != null && distanceToTarget <= rangedRange;

        if (attackType == EnemyAttackType.RangedOnly && canShoot && Time.time >= lastArrowTime + arrowCooldown)
        {
            LaunchProjectile();
            return;
        }

        if (canShoot && Time.time >= lastArrowTime + arrowCooldown)
        {
            if (attackType == EnemyAttackType.Both && distanceToTarget > attackRange)
            {
                LaunchProjectile();
                return;
            }
            else if (attackType != EnemyAttackType.MeleeOnly)
            {
                LaunchProjectile();
                return;
            }
        }

        if (canMelee && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            if (animator != null)
            {
                animator.Play("Attacking", 0, 0f);
            }
        }
    }

    public void DamagePlayer()
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

    public void HacerDañoAlJugador()
    {
        DamagePlayer();
    }

    void LaunchProjectile()
    {
        if (arrowPrefab == null || target == null) return;

        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        lastArrowTime = Time.time;

        Vector2 shootDirection = (target.position - transform.position).normalized;
        Vector3 spawnPosition;

        if (arrowSpawnPoint != null)
        {
            spawnPosition = arrowSpawnPoint.position;
        }
        else
        {
            spawnPosition = transform.position + (Vector3)shootDirection * 0.5f;
        }

        GameObject arrowObject = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);
        EnemyArrow arrowScript = arrowObject.GetComponent<EnemyArrow>();
        if (arrowScript != null)
        {
            arrowScript.Launch(shootDirection);
        }

        if (animator != null)
        {
            animator.Play("Attacking", 0, 0f);
        }
    }

    public void PlayImpactAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        Vector2 velocity = rb2d.velocity;
        bool isMoving = (velocity.sqrMagnitude > 0.01f);

        if (HasParameter("IsMoving")) animator.SetBool("IsMoving", isMoving);

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

            if (HasParameter("MoveX")) animator.SetFloat("MoveX", currentMoveX);
            if (HasParameter("MoveY")) animator.SetFloat("MoveY", currentMoveY);

            prevMoveX = currentMoveX;
            prevMoveY = currentMoveY;
        }
        else
        {
            if (HasParameter("MoveX")) animator.SetFloat("MoveX", prevMoveX);
            if (HasParameter("MoveY")) animator.SetFloat("MoveY", prevMoveY);
        }
    }

    private bool HasParameter(string paramName)
    {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
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
