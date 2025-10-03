using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;
    [SerializeField] private float spriteScale = 2f; 

    private Vector2 moveInput;
    private Vector2 lastDirection = Vector2.down; 
    
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    
    private bool isDashing = false;
    private bool isRunning = false;
    private bool isCrouching = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector2 dashDirection;
    
    
    [Header("Audio Settings")]
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
    private PlayerControls controls;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        
        rb.mass = 1.0f;
        rb.drag = 8.0f;
        rb.angularDrag = 0.05f;
        rb.gravityScale = 0.0f;
        
        animator = GetComponent<Animator>();
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
spriteRenderer = GetComponent<SpriteRenderer>();
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        moveInput = controls.Player.Move.ReadValue<Vector2>();
        
        if (moveInput.sqrMagnitude > 0.01f) 
        {
            lastDirection = moveInput.normalized;
        }

        if (moveInput.x != 0)
        {
            spriteRenderer.transform.localScale = new Vector3(moveInput.x < 0 ? -spriteScale : spriteScale, spriteScale, spriteScale);
        }

        isRunning = controls.Player.Run.IsPressed() && !isCrouching;
        isCrouching = controls.Player.Crouch.IsPressed();
        
        
        HandleFootsteps();
HandleDash();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        HandleMovement();
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
            if (isCrouching)
            {
                currentSpeed *= crouchSpeedMultiplier;
            }
            Vector2 velocity = moveInput.normalized * currentSpeed;
            rb.velocity = velocity;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void HandleDash()
    {
        dashCooldownTimer -= Time.deltaTime;
        
        if (controls.Player.Dash.WasPressedThisFrame() && dashCooldownTimer <= 0f && !isDashing)
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
        
        if (animator.HasParameter("IsRunning"))
            animator.SetBool("IsRunning", isRunning);
            
        if (animator.HasParameter("IsCrouching"))
            animator.SetBool("IsCrouching", isCrouching);
            
        animator.SetFloat("MoveX", Mathf.Abs(lastDirection.x));
        animator.SetFloat("MoveY", lastDirection.y);
    }

    public void SetCanMove(bool state)
    {
        canMove = state;
    }

    public Vector2 GetLastDirection()
    {
        return lastDirection;
    }

    

   public bool IsCrouching()
    {
        return isCrouching;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
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
void OnCollisionStay2D(Collision2D collision)
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
    
    void OnCollisionExit2D(Collision2D collision)
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
public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }
}
