using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float spriteScale = 2f;

    private Vector2 moveInput;
    private Vector2 aimInput;
    private Vector2 lastDirection = Vector2.down;

    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    private bool isDashing = false;
    private bool isRunning = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector2 dashDirection;

    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip[] runFootstepSounds;
    [SerializeField] private float footstepVolume = 0.5f;
    [SerializeField] private float pitchVariation = 0.2f;
    [SerializeField] private float walkStepInterval = 0.35f;
    [SerializeField] private float runStepInterval = 0.25f;

    private AudioSource audioSource;
    private float stepTimer = 0f;
    private bool wasMovingLastFrame = false;

    [SerializeField] private bool canMove = true;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private float aimThreshold = 0.2f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (canMove)
        {
            HandleInput();
            HandleDash();
        }
        HandleFootsteps();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (canMove && !isDashing)
        {
            HandleMovement();
        }
        else if (isDashing)
        {
            HandleDashMovement();
        }
    }

    private void HandleInput()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetButtonDown("Dash") && dashCooldownTimer <= 0f)
        {
            StartDash();
        }

        if (moveInput.sqrMagnitude > 0.1f)
        {
            lastDirection = moveInput.normalized;
        }

        dashCooldownTimer -= Time.deltaTime;
    }

    private void HandleMovement()
    {
        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        rb.MovePosition(rb.position + moveInput * currentSpeed * Time.fixedDeltaTime);
    }

    private void HandleDash()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                dashCooldownTimer = dashCooldown;
            }
        }
    }

    private void HandleDashMovement()
    {
        rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
    }

    private void StartDash()
    {
        if (moveInput.sqrMagnitude > 0.1f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashDirection = moveInput.normalized;
        }
        else if (lastDirection.sqrMagnitude > 0.1f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashDirection = lastDirection;
        }
    }

    private void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
            animator.SetFloat("Speed", moveInput.sqrMagnitude);
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsDashing", isDashing);
        }
    }

    private void HandleFootsteps()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.01f && canMove && !isDashing;
        
        if (isMoving)
        {
            if (!wasMovingLastFrame)
            {
                PlayFootstepSound();
                stepTimer = 0f;
            }

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
                if (isRunning)
                {
                    var vib = GetComponent<JoystickVibration>();
                    if (vib != null) vib.OnRun();
                }
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

    public bool IsRunning()
    {
        return isRunning;
    }

    public float GetRunSpeed()
    {
        return runSpeed;
    }
}
