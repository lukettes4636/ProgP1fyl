using UnityEngine;

public class DemonCreature : SummonedCreature
{
    [Header("Combat Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.8f;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private int projectileDamage = 35;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float projectileLifetime = 3f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private LayerMask enemyLayers = -1;
    [SerializeField] private string[] enemyTags = { "Enemy", "Hostile", "Boss" };

    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float optimalAttackDistance = 5f;
    [SerializeField] private float minAttackDistance = 3f;

    [Header("Follow Player")]
    [SerializeField] private float followDistance = 3f;
    [SerializeField] private float stopDistance = 2f;

    [Header("Attack Animation Duration")]
    [SerializeField] private float attackAnimationDuration = 0.5f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDelay = 0.5f;
    [SerializeField] private float fadeDuration = 1f;

    private Transform currentTarget;
    private Transform playerTransform;
    private float lastFireTime;
    private AudioSource audioSource;
    private GameObject activeProjectile;
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float attackAnimationTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private bool isFading = false;
    private Transform lastKilledEnemy;

    protected override void Awake()
    {
        base.Awake();

        // Buscar al jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (firePoint == null)
        {
            Transform existingFirePoint = transform.Find("FirePoint");
            if (existingFirePoint != null)
            {
                firePoint = existingFirePoint;
            }
            else
            {
                GameObject firePointObj = new GameObject("FirePoint");
                firePointObj.transform.SetParent(transform);
                firePointObj.transform.localPosition = Vector3.right * 0.5f;
                firePoint = firePointObj.transform;
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        SetCreatureType(CreatureType.Demon);
        moveSpeed = chaseSpeed;
    }

    protected override void Update()
    {
        if (isFading) return;

        // Manejar timer de animación de ataque
        if (isAttacking)
        {
            attackAnimationTimer += Time.deltaTime;
            if (attackAnimationTimer >= attackAnimationDuration)
            {
                StopAttackAnimation();
                attackAnimationTimer = 0f;
            }
        }

        base.Update();

        FindTarget();

        // Verificar si el enemigo murió
        if (lastKilledEnemy != null && currentTarget == null)
        {
            StartCoroutine(FadeOutAndDestroy());
            lastKilledEnemy = null;
        }

        if (currentTarget != null)
        {
            if (!isAttacking)
            {
                HandleTargetMovement();
            }

            if (activeProjectile == null && !isAttacking && Time.time - lastFireTime >= 1f / fireRate)
            {
                AttackTarget();
            }
        }
        else
        {
            if (!isAttacking)
            {
                // Si no hay enemigos, seguir al jugador
                FollowPlayer();
            }
        }
    }

    private void HandleTargetMovement()
    {
        if (currentTarget == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        Vector2 directionToTarget = (currentTarget.position - transform.position).normalized;

        if (distanceToTarget > optimalAttackDistance)
        {
            // Acercarse al enemigo
            moveDirection = directionToTarget;
            Vector2 targetPosition = (Vector2)transform.position + moveDirection * moveSpeed * Time.deltaTime;
            rb.MovePosition(targetPosition);
            SetMovementDirection(moveDirection);
        }
        else if (distanceToTarget < minAttackDistance)
        {
            // Alejarse si está muy cerca
            moveDirection = -directionToTarget;
            Vector2 targetPosition = (Vector2)transform.position + moveDirection * (moveSpeed * 0.5f) * Time.deltaTime;
            rb.MovePosition(targetPosition);
            SetMovementDirection(moveDirection);
        }
        else
        {
            // Distancia óptima, dejar de moverse pero mantener orientación
            SetMovementDirection(Vector2.zero);
            lastMovementDirection = directionToTarget;
        }
    }

    protected override void HandleCombat()
    {
    }

    private void FollowPlayer()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > followDistance)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            Vector2 targetPosition = (Vector2)transform.position + direction * moveSpeed * Time.deltaTime;

            rb.MovePosition(targetPosition);
            SetMovementDirection(direction);
        }
        else if (distanceToPlayer < stopDistance)
        {
            Vector2 direction = (transform.position - playerTransform.position).normalized;
            Vector2 targetPosition = (Vector2)transform.position + direction * (moveSpeed * 0.5f) * Time.deltaTime;

            rb.MovePosition(targetPosition);
            SetMovementDirection(direction);
        }
        else
        {
            SetMovementDirection(Vector2.zero);
        }
    }

    private void FindTarget()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRange, enemyLayers);

        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider2D enemy in enemies)
        {
            if (IsEnemy(enemy.gameObject))
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy.transform;
                }
            }
        }

        // Si perdimos el objetivo, marcarlo como último enemigo matado
        if (currentTarget != null && closestEnemy == null)
        {
            lastKilledEnemy = currentTarget;
        }

        currentTarget = closestEnemy;
    }

    private bool IsEnemy(GameObject target)
    {
        if (target.CompareTag("Player") || target == gameObject)
            return false;

        foreach (string enemyTag in enemyTags)
        {
            if (target.CompareTag(enemyTag))
                return true;
        }

        if (target.GetComponent<EnemyHealth>() != null || target.GetComponent<EnemyAI>() != null)
            return true;

        return false;
    }

    private void AttackTarget()
    {
        if (currentTarget == null || projectilePrefab == null) return;

        // Orientar hacia el objetivo y detener movimiento
        Vector2 directionToTarget = (currentTarget.position - transform.position).normalized;
        lastMovementDirection = directionToTarget;
        SetMovementDirection(Vector2.zero);

        // Iniciar animación de ataque
        StartAttackAnimation();
        attackAnimationTimer = 0f;

        Vector2 shootDirection = CalculatePredictiveShootDirection();

        activeProjectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        if (shootClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootClip);
        }

        Projectile projScript = activeProjectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.SetupProjectile(shootDirection, projectileDamage, projectileSpeed, projectileLifetime);
            projScript.SetEnemyLayers(enemyLayers);
        }

        lastFireTime = Time.time;
    }

    private Vector2 CalculatePredictiveShootDirection()
    {
        if (currentTarget == null) return lastMovementDirection;

        Vector2 targetVelocity = Vector2.zero;
        Rigidbody2D targetRb = currentTarget.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            targetVelocity = targetRb.velocity;
        }

        Vector2 toTarget = currentTarget.position - transform.position;
        float distance = toTarget.magnitude;
        float timeToTarget = distance / projectileSpeed;

        Vector2 predictedPosition = (Vector2)currentTarget.position + targetVelocity * timeToTarget;
        Vector2 shootDirection = (predictedPosition - (Vector2)transform.position).normalized;

        return shootDirection;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, optimalAttackDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minAttackDistance);

        if (currentTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }

    public bool HasTarget()
    {
        return currentTarget != null;
    }

    private System.Collections.IEnumerator FadeOutAndDestroy()
    {
        isFading = true;

        // Esperar un momento antes de empezar a desvanecer
        yield return new WaitForSeconds(fadeDelay);

        if (spriteRenderer != null)
        {
            Color startColor = spriteRenderer.color;
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}