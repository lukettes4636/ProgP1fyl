using UnityEngine;
// Comentario: Controla el movimiento del jugador en 2D top-down, incluyendo correr y dash
// Comentario: Mantiene el mismo comportamiento y parámetros actuales

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Configuraci�n de movimiento")]
    [SerializeField] private float moveSpeed = 5f;    // Velocidad de caminar
    [SerializeField] private float runSpeed = 8f;     // Velocidad de correr
    [SerializeField] private float spriteScale = 2f;  // Escala del sprite (para voltear)

    private Vector2 moveInput;       // Movimiento con el stick izquierdo
    private Vector2 aimInput;        // Apuntado con el stick derecho
    private Vector2 lastDirection = Vector2.down; // �ltima direcci�n usada

    [Header("Configuraci�n de dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    private bool isDashing = false;
    private bool isRunning = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector2 dashDirection;

    [Header("Configuraci�n de sonidos")]
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

    [Header("Configuraci�n de apuntado")]
    [Tooltip("Magnitud m�nima del stick derecho para considerarse apuntando.")]
    [SerializeField] private float aimThreshold = 0.2f;

// Comentario: Inicializa componentes y configura el Rigidbody2D
private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Configuraci�n b�sica del Rigidbody
        rb.mass = 1.0f;
        rb.drag = 8.0f;
        rb.angularDrag = 0.05f;
        rb.gravityScale = 0.0f;

        // Crear AudioSource si no existe
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

// Comentario: Lee entradas cada frame y gestiona dash, pasos y animaciones
private void Update()
    {
        HandleInput();
        HandleDash();
        HandleFootsteps();
        UpdateAnimator();
    }

// Comentario: Aplica el movimiento físico estable
private void FixedUpdate()
    {
        HandleMovement();
    }

// Comentario: Procesa sticks izquierdo/derecho y determina última dirección
private void HandleInput()
    {
        //Stick izquierdo Movimiento
        moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Stick derecho Apuntado o rotaci�n
        aimInput = new Vector2(Input.GetAxis("AimHorizontal"), Input.GetAxis("AimVertical"));

        // Si el stick derecho se usa, priorizar esa direcci�n
        if (aimInput.sqrMagnitude > aimThreshold)
        {
            lastDirection = aimInput.normalized;
        }
        // Si no hay apuntado, usar la direcci�n de movimiento
        else if (moveInput.sqrMagnitude > 0.01f)
        {
            lastDirection = moveInput.normalized;
        }

        // Voltear sprite seg�n direcci�n en X
        if (lastDirection.x != 0)
        {
            spriteRenderer.transform.localScale = new Vector3(
                lastDirection.x < 0 ? -spriteScale : spriteScale,
                spriteScale,
                spriteScale
            );
        }

        // Correr
        isRunning = Input.GetButton("Run");
    }

// Comentario: Aplica la velocidad de caminar/correr o el dash actual
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

// Comentario: Controla temporizador y cooldown del dash
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

// Comentario: Inicia dash y reproduce el sonido mediante el controlador de acciones
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

// Comentario: Finaliza el dash y limpia temporizadores
private void EndDash()
    {
        isDashing = false;
        dashTimer = 0f;
    }

// Comentario: Actualiza parámetros del Animator para reflejar movimiento/idle
private void UpdateAnimator()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsRunning", isRunning);

        // Actualiza direcci�n actual para el blend tree de movimiento / idle
        animator.SetFloat("MoveX", lastDirection.x);
        animator.SetFloat("MoveY", lastDirection.y);

        // Guarda �ltima direcci�n (para idle direccional)
        animator.SetFloat("LastMoveX", lastDirection.x);
        animator.SetFloat("LastMoveY", lastDirection.y);
    }

// Comentario: Genera sonidos de pasos según el estado de movimiento
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

// Comentario: Reproduce un clip de pasos con variación de pitch simple
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

// Comentario: Permite habilitar/deshabilitar movimiento desde otros scripts
public void SetCanMove(bool state)
    {
        canMove = state;
    }

    // Permite a otros scripts (como PlayerActionController) obtener la direcci�n actual
// Comentario: Devuelve la última dirección útil para orientar acciones
public Vector2 GetLastDirection()
    {
        return lastDirection;
    }

// Comentario: Devuelve el SpriteRenderer para ajustar flip/escala
public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }

// Comentario: Cambia el tipo de cuerpo al chocar con enemigos
private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

// Comentario: Limita movimiento en contacto con enemigos/agua
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

// Comentario: Restaura el movimiento al salir de colisiones
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
