using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Combat Settings")]
    public GameObject projectilePrefab;
    public Transform playerTarget;
    public Transform firePoint;
    public float projectileSpeed = 8f;
    public float fireRate = 2f;

    [Header("Range Settings")]
    public float detectionRange = 10f;
    public float shootingRange = 6f;
    public float movementSpeed = 2f;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float shootAnimationDuration = 0.8f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip shootSound;
    private AudioSource audioSource;

    private float nextFireTime;
    private Vector2 lastDirection = Vector2.down;
    private bool isShooting = false;
    private bool isDead = false;
    private Vector2 pendingShootDirection;
    private EnemyHealth enemyHealth;
    private Rigidbody2D rb;
    private bool isMoving = false;

    private readonly string ANIM_HORIZONTAL = "MoveX";
    private readonly string ANIM_VERTICAL = "MoveY";
    private readonly string ANIM_IS_MOVING = "IsMoving";
    private readonly string ANIM_SHOOT = "Shoot";
    private readonly string ANIM_HIT = "Hit";
    private readonly string ANIM_DEATH = "Death";

    private void Start()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        enemyHealth = GetComponent<EnemyHealth>();
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

        SetupFirePoint();
        InitializeAnimator();
    }

    private void SetupFirePoint()
    {
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

    private void InitializeAnimator()
    {
        if (animator != null)
        {
            if (HasParameter(ANIM_HORIZONTAL)) animator.SetFloat(ANIM_HORIZONTAL, 0f);
            if (HasParameter(ANIM_VERTICAL)) animator.SetFloat(ANIM_VERTICAL, -1f);
            if (HasParameter(ANIM_IS_MOVING)) animator.SetBool(ANIM_IS_MOVING, false);
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

    private void Update()
    {
        if (isDead || playerTarget == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
        Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;

        if (distanceToPlayer <= detectionRange && distanceToPlayer > shootingRange)
        {
            if (!isShooting)
            {
                MoveTowardsPlayer(directionToPlayer);
            }
        }
        else if (distanceToPlayer <= shootingRange)
        {
            StopMovement();
            UpdateDirection(directionToPlayer);

            if (!isShooting && Time.time >= nextFireTime)
            {
                StartShootAnimation();
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            StopMovement();
        }

        if (animator != null)
        {
            animator.SetBool(ANIM_IS_MOVING, isMoving);
        }
    }

    private void MoveTowardsPlayer(Vector2 direction)
    {
        Vector2 targetPosition = (Vector2)transform.position + direction * movementSpeed * Time.deltaTime;
        rb.MovePosition(targetPosition);

        isMoving = true;
        UpdateDirection(direction);
    }

    private void StopMovement()
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

        float shootTiming = shootAnimationDuration * 0.5f;
        Invoke(nameof(ExecuteBackupShoot), shootTiming);
        Invoke(nameof(FinishShooting), shootAnimationDuration);
    }

    public void OnShootFrame()
    {
        CancelInvoke(nameof(ExecuteBackupShoot));
        ExecuteShoot();
    }

    private void ExecuteBackupShoot()
    {
        ExecuteShoot();
    }

    private void ExecuteShoot()
    {
        if (projectilePrefab == null || isDead) return;

        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

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

    private void FinishShooting()
    {
        isShooting = false;
    }

    public void PlayImpactAnimation()
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
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        if (playerTarget != null)
        {
            float dist = Vector2.Distance(transform.position, playerTarget.position);
            if (dist <= detectionRange)
            {
                Gizmos.color = dist <= shootingRange ? Color.red : Color.yellow;
                Gizmos.DrawLine(transform.position, playerTarget.position);
            }
        }
    }
}