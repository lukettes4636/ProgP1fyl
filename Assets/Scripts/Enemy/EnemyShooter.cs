using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Combat Settings")]
    public GameObject projectilePrefab;
    public Transform targetPlayer;
    public Transform firePoint;
    public float projectileSpeed = 8f;
    public float fireRate = 2f;
    public float detectionRange = 8f;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float shootAnimationDuration = 0.8f;

    private float nextFireTime;
    private Vector2 lastDirection = Vector2.down;
    private bool isShooting = false;
    private bool isDead = false;
    private Vector2 pendingShootDirection;
    private EnemyHealth enemyHealth;

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

        InitializeAnimator();
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

        float distance = Vector2.Distance(transform.position, targetPlayer.position);

        // Calcular dirección hacia el jugador
        Vector2 directionToPlayer = (targetPlayer.position - transform.position).normalized;
        UpdateDirection(directionToPlayer);

        if (distance <= detectionRange && !isShooting)
        {
            if (Time.time >= nextFireTime)
            {
                StartShootAnimation();
                nextFireTime = Time.time + fireRate;
            }
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

        // Fallback por si no hay Animation Event
        Invoke(nameof(EndShooting), shootAnimationDuration);
    }

    // Este método es llamado por el Animation Event
    public void OnShootFrame()
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}