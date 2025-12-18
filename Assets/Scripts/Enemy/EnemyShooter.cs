using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Combat Settings")]
    public GameObject projectilePrefab;
    public Transform targetPlayer;
    public Transform firePoint;
    public float projectileSpeed = 8f;
    public float fireRate = 2f;

    [Header("Range Settings")]
    public float detectionRange = 10f;
    public float shootRange = 6f;
    public float moveSpeed = 2f;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float shootAnimationDuration = 0.8f;

    private float nextFireTime;
    private Vector2 lastDirection = Vector2.down;
    private bool isShooting = false;
    private bool isDead = false;
    private Vector2 pendingShootDirection;
    private EnemyHealth enemyHealth;
    private Rigidbody2D rb;
    private bool isMoving = false;

    // Animation parameter names
    private readonly string ANIM_HORIZONTAL = "Horizontal";
    private readonly string ANIM_VERTICAL = "Vertical";
    private readonly string ANIM_IS_MOVING = "IsMoving";
    private readonly string ANIM_SHOOT = "Shoot";
    private readonly string ANIM_HIT = "Hit";
    private readonly string ANIM_DEATH = "Death";

    private void Start()
    {
        if (targetPlayer == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                targetPlayer = player.transform;
            }
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        enemyHealth = GetComponent<EnemyHealth>();

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // Configurar FirePoint
        SetupFirePoint();

        InitializeAnimator();
    }

    private void SetupFirePoint()
    {
        if (firePoint == null)
        {
            // Buscar si ya existe un FirePoint hijo
            Transform existingFirePoint = transform.Find("FirePoint");
            if (existingFirePoint != null)
            {
                firePoint = existingFirePoint;
            }
            else
            {
                // Crear un nuevo FirePoint
                GameObject firePointObj = new GameObject("FirePoint");
                firePointObj.transform.SetParent(transform);
                firePointObj.transform.localPosition = Vector3.right * 0.5f; // Ajusta esta posición
                firePoint = firePointObj.transform;
            }
        }
    }

    private void InitializeAnimator()
    {
        if (animator != null)
        {
            animator.SetFloat(ANIM_HORIZONTAL, 0f);
            animator.SetFloat(ANIM_VERTICAL, -1f);
            animator.SetBool(ANIM_IS_MOVING, false);
        }
    }

    private void Update()
    {
        if (isDead || targetPlayer == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);

        // Calcular dirección hacia el jugador
        Vector2 directionToPlayer = (targetPlayer.position - transform.position).normalized;

        // Sistema de dos rangos
        if (distanceToPlayer <= detectionRange && distanceToPlayer > shootRange)
        {
            // RANGO 1: Detectado pero fuera de rango de disparo - PERSEGUIR
            if (!isShooting)
            {
                MoveTowardsPlayer(directionToPlayer);
            }
        }
        else if (distanceToPlayer <= shootRange)
        {
            // RANGO 2: Dentro del rango de disparo - DETENERSE Y DISPARAR
            StopMoving();
            UpdateDirection(directionToPlayer);

            if (!isShooting && Time.time >= nextFireTime)
            {
                StartShootAnimation();
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            // Fuera de ambos rangos - IDLE
            StopMoving();
        }

        // Actualizar animación de movimiento
        if (animator != null)
        {
            animator.SetBool(ANIM_IS_MOVING, isMoving);
        }
    }

    private void MoveTowardsPlayer(Vector2 direction)
    {
        // Moverse hacia el jugador
        Vector2 targetPosition = (Vector2)transform.position + direction * moveSpeed * Time.deltaTime;
        rb.MovePosition(targetPosition);

        isMoving = true;
        UpdateDirection(direction);
    }

    private void StopMoving()
    {
        isMoving = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void UpdateDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.01f && !isShooting)
        {
            lastDirection = direction.normalized;

            if (animator != null)
            {
                animator.SetFloat(ANIM_HORIZONTAL, lastDirection.x);
                animator.SetFloat(ANIM_VERTICAL, lastDirection.y);
            }
        }
    }

    private void StartShootAnimation()
    {
        if (isShooting || isDead) return;

        isShooting = true;
        pendingShootDirection = lastDirection;

        if (animator != null)
        {
            animator.SetTrigger(ANIM_SHOOT);
        }

        // Fallback: disparar automáticamente si no hay Animation Event
        // Calcula cuándo debería disparar (en el medio de la animación)
        float shootTiming = shootAnimationDuration * 0.5f; // 50% de la animación
        Invoke(nameof(ExecuteShootFallback), shootTiming);

        // Terminar el estado de disparo
        Invoke(nameof(EndShooting), shootAnimationDuration);
    }

    // Este método es llamado por el Animation Event (MÉTODO PRINCIPAL)
    public void OnShootFrame()
    {
        // Cancelar el fallback porque el Animation Event funcionó
        CancelInvoke(nameof(ExecuteShootFallback));
        ExecuteShoot();
    }

    // Fallback por si no hay Animation Event configurado
    private void ExecuteShootFallback()
    {
        ExecuteShoot();
    }

    private void ExecuteShoot()
    {
        if (projectilePrefab == null || isDead) return;

        Vector3 spawnPosition = transform.position;
        if (firePoint != null)
        {
            spawnPosition = firePoint.position;
        }

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        Vector2 direction = pendingShootDirection;

        EnemyArrow arrowScript = projectile.GetComponent<EnemyArrow>();
        if (arrowScript != null)
        {
            arrowScript.Launch(direction);

            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direction * projectileSpeed;
            }
        }
    }

    private void EndShooting()
    {
        isShooting = false;
    }

    public void PlayHitAnimation()
    {
        if (animator != null && !isDead)
        {
            animator.SetTrigger(ANIM_HIT);
        }
    }

    public void PlayDeathAnimation()
    {
        if (isDead) return;

        isDead = true;
        isShooting = false;

        if (animator != null)
        {
            // Para muerte, usar la dirección horizontal (izquierda/derecha)
            float horizontal = lastDirection.x;
            animator.SetFloat(ANIM_HORIZONTAL, horizontal);
            animator.SetTrigger(ANIM_DEATH);
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    private void OnDrawGizmosSelected()
    {
        // Rango de detección (verde)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rango de disparo (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);

        // Línea hacia el jugador si está en rango
        if (targetPlayer != null)
        {
            float dist = Vector2.Distance(transform.position, targetPlayer.position);
            if (dist <= detectionRange)
            {
                Gizmos.color = dist <= shootRange ? Color.red : Color.yellow;
                Gizmos.DrawLine(transform.position, targetPlayer.position);
            }
        }
    }
}