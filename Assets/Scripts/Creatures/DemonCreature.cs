using UnityEngine;

public class DemonCreature : SummonedCreature
{
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private int meleeDamage = 35;

    [SerializeField] private float detectionRange = 10f; 
    [SerializeField] private LayerMask enemyLayers = -1;
    [SerializeField] private string[] enemyTags = { "Enemy", "Hostile", "Boss" };

    [SerializeField] private float chaseSpeed = 12f;
    [SerializeField] private float meleeAttackRange = 2f; 
    [SerializeField] private float meleeStopDist = 1.8f; 

    public void AttackHitboxOn() { }
    public void AttackHitboxOff() { }

    [SerializeField] private float followDist = 3f;
    [SerializeField] private float stopDist = 2f;

    [SerializeField] private float attackAnimDuration = 0.5f;

    [SerializeField] private float fadeDelay = 0.5f;
    [SerializeField] private float fadeDuration = 1f;

    private Transform currentTarget;
    private Transform playerTransform;
    private float lastAttackTime;
    private AudioSource audioSource;
    private Rigidbody2D rb2d;
    private Vector2 moveDir;
    private float attackAnimTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private bool isFading = false;
    private Transform lastDeadEnemy;

    protected override void Awake()
    {
        base.Awake();

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

        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d == null)
        {
            rb2d = gameObject.AddComponent<Rigidbody2D>();
        }
        rb2d.gravityScale = 0f;
        rb2d.drag = 0f;
        rb2d.angularDrag = 0f;
        rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb2d.bodyType = RigidbodyType2D.Kinematic;

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void Start()
    {
        base.Start();
        SetCreatureType(CreatureType.Demon);
        
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            moveSpeed = player.GetRunSpeed();
        }
        else
        {
            moveSpeed = chaseSpeed;
        }
    }

    protected override void Update()
    {
        if (isFading) return;

        if (isAttacking)
        {
            attackAnimTimer += Time.deltaTime;
            if (attackAnimTimer >= attackAnimDuration)
            {
                StopAttackAnimation();
                attackAnimTimer = 0f;
            }
        }

        base.Update();
        FindTarget();

        if (currentTarget == null && lastDeadEnemy != null)
        {
            Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(transform.position, detectionRange, enemyLayers);
            bool enemiesLeft = false;
            foreach (var col in potentialTargets)
            {
                if (IsEnemy(col.gameObject)) { enemiesLeft = true; break; }
            }

            if (!enemiesLeft)
            {
                StartCoroutine(FadeAndDestroy());
                lastDeadEnemy = null;
            }
        }

        if (currentTarget != null)
        {
            float distToTarget = Vector2.Distance(transform.position, currentTarget.position);

            if (!isAttacking)
            {
                MoveTowardsTarget();
            }

            if (!isAttacking && distToTarget <= meleeAttackRange && Time.time - lastAttackTime >= attackCooldown)
            {
                AttackTarget();
            }
        }
        else
        {
            if (!isAttacking)
            {
                FollowPlayer();
            }
        }
    }

    private void MoveTowardsTarget()
    {
        if (currentTarget == null) return;

        float dist = Vector2.Distance(transform.position, currentTarget.position);
        Vector2 dir = (currentTarget.position - transform.position).normalized;

        if (dist > meleeStopDist)
        {
            moveDir = dir;
            Vector2 targetPos = (Vector2)transform.position + moveDir * moveSpeed * Time.deltaTime;
            rb2d.MovePosition(targetPos);
            SetMoveDirection(moveDir);
        }
        else
        {
            SetMoveDirection(Vector2.zero);
            lastMoveDir = dir;
        }
    }

    private void FollowPlayer()
    {
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        if (dist > followDist)
        {
            Vector2 dir = (playerTransform.position - transform.position).normalized;
            Vector2 targetPos = (Vector2)transform.position + dir * moveSpeed * Time.deltaTime;
            rb2d.MovePosition(targetPos);
            SetMoveDirection(dir);
        }
        else if (dist < stopDist)
        {
            Vector2 dir = (transform.position - playerTransform.position).normalized;
            Vector2 targetPos = (Vector2)transform.position + dir * (moveSpeed * 0.5f) * Time.deltaTime;
            rb2d.MovePosition(targetPos);
            SetMoveDirection(dir);
        }
        else
        {
            SetMoveDirection(Vector2.zero);
        }
    }

    private void FindTarget()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRange, enemyLayers);

        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (Collider2D enemy in enemies)
        {
            if (IsEnemy(enemy.gameObject))
            {
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = enemy.transform;
                }
            }
        }

        if (currentTarget != null && closest == null)
        {
            lastDeadEnemy = currentTarget;
        }

        currentTarget = closest;
    }

    private bool IsEnemy(GameObject target)
    {
        if (target.CompareTag("Player") || target == gameObject)
            return false;

        foreach (string t in enemyTags)
        {
            if (target.CompareTag(t))
                return true;
        }

        if (target.GetComponent<EnemyHealth>() != null || target.GetComponent<EnemyAI>() != null)
            return true;

        return false;
    }

    private void AttackTarget()
    {
        if (currentTarget == null) return;

        Vector2 dir = (currentTarget.position - transform.position).normalized;
        lastMoveDir = dir;
        SetMoveDirection(Vector2.zero);

        StartAttackAnimation();
        attackAnimTimer = 0f;

        EnemyHealth enemyHealth = currentTarget.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(meleeDamage);
        }

        if (attackClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackClip);
        }

        lastAttackTime = Time.time;
    }

    protected override void HandleCombat()
    {
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeStopDist);

        if (currentTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }

    private System.Collections.IEnumerator FadeAndDestroy()
    {
        isFading = true;
        yield return new WaitForSeconds(fadeDelay);

        if (spriteRenderer != null)
        {
            Color startColor = spriteRenderer.color;
            float time = 0f;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}
