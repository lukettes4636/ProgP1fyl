using UnityEngine;

public class AngelCreature : SummonedCreature
{
    [SerializeField] private int healAmount = 30;
    [SerializeField] private float healRange = 5f;
    [SerializeField] private float healCooldown = 3f;
    [SerializeField] private KeyCode healKey = KeyCode.Q;

    [SerializeField] private float followSpeed = 12f;
    [SerializeField] private float followDist = 2f;
    [SerializeField] private float stopDist = 1.5f;

    [SerializeField] private GameObject healEffect;
    [SerializeField] private AudioClip healSound;

    [SerializeField] private float attackAnimDuration = 0.6f;

    [SerializeField] private float fadeDelay = 0.5f;
    [SerializeField] private float fadeDuration = 1f;

    private PlayerHealth playerHealth;
    private Transform playerTransform;
    private float lastHealTime;
    private bool isOnCooldown = false;
    private AudioSource audioSource;
    private Rigidbody2D rb2d;
    private float attackAnimTimer = 0f;
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
        SetCreatureType(CreatureType.Angel);
        
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            moveSpeed = player.GetRunSpeed();
        }
        else
        {
            moveSpeed = followSpeed;
        }

        attackDamage = 0;
        attackRange = 0f;
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

                if (hasHealed)
                {
                    StartCoroutine(FadeAndDestroy());
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
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist <= healRange)
            {
                Heal();
            }
        }

        if (!isAttacking && !isFading)
        {
            FollowPlayer();
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

    protected override void HandleCombat()
    {
    }

    private void Heal()
    { 
        if (playerHealth == null) return;

        playerHealth.Heal(healAmount);
        lastHealTime = Time.time;
        isOnCooldown = true;
        hasHealed = true;

        if (healEffect != null)
        {
            Instantiate(healEffect, playerTransform.position, Quaternion.identity);
        }

        if (healSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(healSound);
        }

        if (anim != null)
        {
            if (HasParameter(PARAM_ATK)) anim.SetTrigger(PARAM_ATK);
            else if (HasParameter("Attack")) anim.SetTrigger("Attack");
            
            StartAttackAnimation();
        }
    }

    private bool HasParameter(string paramName)
    {
        if (anim == null) return false;
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    private System.Collections.IEnumerator FadeAndDestroy()
    { 
        isFading = true;
        yield return new WaitForSeconds(fadeDelay);

        float t = 0;
        Color col = spriteRenderer.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            col.a = Mathf.Lerp(1, 0, t / fadeDuration);
            spriteRenderer.color = col;
            yield return null;
        }

        Destroy(gameObject);
    }
}
