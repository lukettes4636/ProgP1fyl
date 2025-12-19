using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 2.8f;
    [SerializeField] private float runSpeed = 5.0f;

    private Vector2 moveInput;
    private Vector2 aimInput;
    private Vector2 lastDir = Vector2.down;

    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    private bool isDashing = false;
    private bool isRunning = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector2 dashDirection;

    [SerializeField] private AudioClip[] walkFootsteps;
    [SerializeField] private AudioClip[] runFootsteps;
    [SerializeField] private float footstepVolume = 0.5f;
    [SerializeField] private float pitchVariation = 0.2f;
    [SerializeField] private float walkStepInterval = 0.35f;
    [SerializeField] private float runStepInterval = 0.25f;

    private AudioSource audioSource;
    private float stepTimer = 0f;
    private bool wasMoving = false;

    [SerializeField] private bool canMove = true;
    [SerializeField] private bool useAimSystem = true; 

    private Rigidbody2D rb2d;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
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
            ProcessInputs();
            HandleDash();
        }
        ManageAnimations();
        HandleFootstepSounds();
    }

    private void FixedUpdate()
    {
        if (canMove && !isDashing)
        {
            MoveCharacter();
        }
        else if (isDashing)
        {
            MoveDash();
        }
    }

    private void ProcessInputs()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        if (useAimSystem)
        {
            aimInput.x = Input.GetAxisRaw("AimHorizontal");
            aimInput.y = Input.GetAxisRaw("AimVertical");

            if (aimInput.sqrMagnitude > 0.1f)
            {
                lastDir = aimInput.normalized;
            }
            else if (moveInput.sqrMagnitude > 0.1f)
            {
                lastDir = moveInput.normalized;
            }
        }
        else
        {
            if (moveInput.sqrMagnitude > 0.1f)
            {
                lastDir = moveInput.normalized;
            }
        }

        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetButtonDown("Dash") && dashCooldownTimer <= 0f)
        {
            StartDash();
        }

        dashCooldownTimer -= Time.deltaTime;
    }

    private void MoveCharacter()
    {
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        rb2d.MovePosition(rb2d.position + moveInput * currentSpeed * Time.fixedDeltaTime);
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

    private void MoveDash()
    {
        rb2d.MovePosition(rb2d.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
    }

    private void StartDash()
    {
        if (moveInput.sqrMagnitude > 0.1f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashDirection = moveInput.normalized;
        }
        else if (lastDir.sqrMagnitude > 0.1f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashDirection = lastDir;
        }
    }

    private void ManageAnimations()
    {
        if (anim != null && !isActingInternally)
        {
            Vector2 animDir;

            if (useAimSystem && aimInput.sqrMagnitude > 0.1f)
            {
                animDir = aimInput;
            }
            else if (moveInput.sqrMagnitude > 0.01f)
            {
                animDir = moveInput;
            }
            else
            {
                animDir = lastDir;
            }

            bool isMoving = moveInput.sqrMagnitude > 0.01f;

            anim.SetBool("IsMoving", isMoving);
            anim.SetBool("IsRunning", isRunning);

            if (animDir.sqrMagnitude > 0.01f)
            {
                if (HasParameter("MoveX")) anim.SetFloat("MoveX", animDir.x);
                if (HasParameter("MoveY")) anim.SetFloat("MoveY", animDir.y);
                if (HasParameter("LastMoveX")) anim.SetFloat("LastMoveX", animDir.x);
                if (HasParameter("LastMoveY")) anim.SetFloat("LastMoveY", animDir.y);
            }
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

    private void HandleFootstepSounds()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.01f && canMove && !isDashing;

        if (isMoving)
        {
            if (!wasMoving)
            {
                PlayFootstepSound();
                stepTimer = 0f;
            }

            stepTimer += Time.deltaTime;
            float currentInterval = isRunning ? runStepInterval : walkStepInterval;

            if (stepTimer >= currentInterval)
            {
                PlayFootstepSound();
                stepTimer = 0f;
            }
        }
        else
        {
            if (wasMoving)
            {
                audioSource.Stop();
            }
            stepTimer = 0f;
        }

        wasMoving = isMoving;
    }

    private void PlayFootstepSound()
    {
        AudioClip[] soundArray = isRunning ? runFootsteps : walkFootsteps;

        if (soundArray != null && soundArray.Length > 0 && audioSource != null)
        {
            AudioClip clip = soundArray[Random.Range(0, soundArray.Length)];

            if (clip != null)
            {
                float rndPitch = 1f + Random.Range(-pitchVariation, pitchVariation);
                audioSource.pitch = rndPitch;
                audioSource.PlayOneShot(clip, footstepVolume);
                if (isRunning)
                {
                    var vibration = GetComponent<JoystickVibration>();
                    if (vibration != null) vibration.OnRun();
                }
            }
        }
    }

    public void SetCanMove(bool state)
    {
        canMove = state;
    }

    private bool isActingInternally = false;

    public void SetIsActing(bool state)
    {
        isActingInternally = state;

        if (!state)
        {
            bool isMoving = moveInput.sqrMagnitude > 0.01f;
            anim.SetBool("IsMoving", isMoving);
        }
    }

    public void SetUseAimSystem(bool use)
    {
        useAimSystem = use;
    }

    public Vector2 GetLastDirection()
    {
        return lastDir;
    }

    public Vector2 GetMovementInput()
    {
        return moveInput;
    }

    public Vector2 GetAimInput()
    {
        return aimInput;
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

    public bool IsUsingAimSystem()
    {
        return useAimSystem;
    }
}
