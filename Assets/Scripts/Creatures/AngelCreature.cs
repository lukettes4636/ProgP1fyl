using UnityEngine;

public class AngelCreature : SummonedCreature
{
    [Header("Angel Settings")]
    [SerializeField] private int healAmount = 30;
    [SerializeField] private float healRange = 5f;
    [SerializeField] private float healCooldown = 3f;
    [SerializeField] private KeyCode healKey = KeyCode.Q;

    [Header("Follow Player")]
    [SerializeField] private float followDistance = 2f;
    [SerializeField] private float stopDistance = 1.5f;

    [Header("Effects")]
    [SerializeField] private GameObject healEffect;
    [SerializeField] private AudioClip healSound;

    [Header("Attack Animation Duration")]
    [SerializeField] private float attackAnimationDuration = 0.6f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDelay = 0.5f;
    [SerializeField] private float fadeDuration = 1f;

    private PlayerHealth playerHealth;
    private Transform playerTransform;
    private float lastHealTime;
    private bool isOnCooldown = false;
    private AudioSource audioSource;
    private Rigidbody2D rb;
    private float attackAnimationTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private bool isFading = false;
    private bool hasHealed = false;

    protected override void Awake()
    {
        base.Awake();

        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerTransform = playerHealth.transform;
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
    }

    protected override void Start()
    {
        base.Start();
        SetCreatureType(CreatureType.Angel);

        attackDamage = 0;
        attackRange = 0f;
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

                // Si ya curó, comenzar a desvanecerse
                if (hasHealed)
                {
                    StartCoroutine(FadeOutAndDestroy());
                }
            }
        }

        base.Update();

        if (isOnCooldown)
        {
            if (Time.time - lastHealTime >= healCooldown)
            {
                isOnCooldown = false;
            }
        }

        if (Input.GetKeyDown(healKey) && !isOnCooldown && !isAttacking)
        {
            TryHealPlayer();
        }

        if (!isAttacking)
        {
            FollowPlayer();
        }
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

    protected override void HandleCombat()
    {
    }

    protected override void Attack(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            if (CanHeal() && !IsOnCooldown())
            {
                ForceHeal();
            }
        }
    }

    private void TryHealPlayer()
    {
        if (playerHealth == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerHealth.transform.position);
        if (distanceToPlayer > healRange)
        {
            Debug.Log("Player too far to heal!");
            return;
        }

        ExecuteHeal();
    }

    private void ExecuteHeal()
    {
        if (playerHealth == null) return;

        // Orientar hacia el jugador al curar
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        lastMovementDirection = directionToPlayer;

        // Detener movimiento y comenzar animación de ataque
        SetMovementDirection(Vector2.zero);
        StartAttackAnimation();
        attackAnimationTimer = 0f;
        hasHealed = true;

        // Curar al jugador
        playerHealth.Heal(healAmount);

        if (healEffect != null)
        {
            GameObject effect = Instantiate(healEffect, playerHealth.transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        if (healSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(healSound);
        }

        lastHealTime = Time.time;
        isOnCooldown = true;
    }

    public bool CanHeal()
    {
        if (isOnCooldown) return false;
        if (playerHealth == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, playerHealth.transform.position);
        return distanceToPlayer <= healRange;
    }

    public bool IsOnCooldown()
    {
        return isOnCooldown;
    }

    public void ForceHeal()
    {
        if (CanHeal())
        {
            ExecuteHeal();
        }
    }

    public float GetCooldownProgress()
    {
        if (!isOnCooldown) return 1f;
        return (Time.time - lastHealTime) / healCooldown;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, healRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, followDistance);
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