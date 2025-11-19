using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de movimiento")]
    [SerializeField] private float moveSpeed = 5f;    
    [SerializeField] private float runSpeed = 8f;     
    [SerializeField] private float spriteScale = 2f;  

    private Vector2 moveInput;       
    private Vector2 aimInput;        
    private Vector2 lastDirection = Vector2.down; 

    [Header("Configuración de dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    private bool isDashing = false;
    private bool isRunning = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector2 dashDirection;

    [Header("Configuración de sonidos")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip[] runFootstepSounds;
    [SerializeField] private float footstepVolume = 0.5f;
    [SerializeField] private float pitchVariation = 0.2f;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;

    private AudioSource audioSource;
    private float stepTimer = 0f;
    private bool wasMovingLastFrame = false;

    [SerializeField] private bool canMove = true;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Header("Configuración de apuntado")]
    [Tooltip("Magnitud mínima del stick derecho para considerarse apuntando.")]
    [SerializeField] private float aimThreshold = 0.2f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.mass = 1.0f;
        rb.drag = 8.0f;
        rb.angularDrag = 0.05f;
        rb.gravityScale = 0.0f;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        HandleInput();
        HandleDash();
        HandleFootsteps();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleInput()
    {
        moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        aimInput = new Vector2(Input.GetAxis("AimHorizontal"), Input.GetAxis("AimVertical"));

        if (aimInput.sqrMagnitude > aimThreshold)
        {
            lastDirection = aimInput.normalized;
        }
        else if (moveInput.sqrMagnitude > 0.01f)
        {
            lastDirection = moveInput.normalized;
        }

        if (lastDirection.x != 0)
        {
            spriteRenderer.transform.localScale = new Vector3(
                lastDirection.x < 0 ? -spriteScale : spriteScale,
                spriteScale,
                spriteScale
            );
        }

        isRunning = Input.GetButton("Run");
    }

    private void HandleMovement()
    {
        if (isDashing)
        {
            rb.velocity = dashDirection * dashSpeed;
        }
        else if (canMove)
        {
            float currentSpeed = isRunning ? runSpeed : moveSpeed;
            rb.velocity = moveInput.normalized * currentSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void HandleDash()
    {
        dashCooldownTimer -= Time.deltaTime;

        if (Input.GetButtonDown("Dash") && dashCooldownTimer <= 0f && !isDashing)
        {
            StartDash();
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                EndDash();
            }
        }
    }

    private void StartDash()
    {
        PlayerActionController actionController = GetComponent<PlayerActionController>();
        if (actionController != null)
        {
            actionController.PlayDashSound();
        }

        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        dashDirection = lastDirection;
    }

    private void EndDash()
    {
        isDashing = false;
        dashTimer = 0f;
    }

    private void UpdateAnimator()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsRunning", isRunning);

        animator.SetFloat("MoveX", lastDirection.x);
        animator.SetFloat("MoveY", lastDirection.y);

        animator.SetFloat("LastMoveX", lastDirection.x);
        animator.SetFloat("LastMoveY", lastDirection.y);
    }

    private void HandleFootsteps()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.01f && canMove && !isDashing;

        if (isMoving)
        {
            stepTimer += Time.deltaTime;
            float currentStepInterval = isRunning ? runStepInterval : walkStepInterval;

            if (stepTimer >= currentStepInterval)
            {
                PlayFootstepSound();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }

        wasMovingLastFrame = isMoving;
    }

    private void PlayFootstepSound()
    {
        AudioClip[] soundArray = isRunning ? runFootstepSounds : footstepSounds;

        if (soundArray != null && soundArray.Length > 0 && audioSource != null)
        {
            AudioClip clipToPlay = soundArray[Random.Range(0, soundArray.Length)];

            if (clipToPlay != null)
            {
                float randomPitch = 1f + Random.Range(-pitchVariation, pitchVariation);
                audioSource.pitch = randomPitch;
                audioSource.PlayOneShot(clipToPlay, footstepVolume);
            }
        }
    }

    public void SetCanMove(bool state)
    {
        canMove = state;
    }

    public Vector2 GetLastDirection()
    {
        return lastDirection;
    }

    public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (collision.gameObject.CompareTag("Water"))
        {
            canMove = false;
            rb.velocity = Vector2.zero;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        if (collision.gameObject.CompareTag("Water"))
        {
            canMove = true;
        }
    }
}