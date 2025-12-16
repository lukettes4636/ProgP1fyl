using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class HorseController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 14.0f;
    [SerializeField] private float mountedSpeedMultiplier = 1.5f;

    [Header("Fatigue Settings")]
    [SerializeField] private float maxRunTime = 5.0f;
    [SerializeField] private float cooldownTime = 3.0f;
    [SerializeField] private float cooldownSpeedMultiplier = 0.5f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip runClip;
    [SerializeField] private AudioClip mountClip;
    [SerializeField] private AudioClip dismountClip;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 1.5f;
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    [Header("Mount Position")]
    [SerializeField] private Vector3 mountOffset = new Vector3(0, 0, 0);
    [SerializeField] private int playerSortingOrderOffset = 1;
    [SerializeField] private float dismountDistance = 1.5f;

    [Header("Visual Settings")]
    [SerializeField] private bool hideHorseWhenMounted = true;
    [SerializeField] private bool useFlipForPlayer = true;
    [SerializeField] private bool useAimWhileMounted = true;

    [Header("UI Prompt (Optional)")]
    [SerializeField] private GameObject interactionPrompt;

    [Header("Horse Visual Components")]
    [SerializeField] private SpriteRenderer horseSpriteRenderer;
    [SerializeField] private Animator horseAnimator;

    private GameObject player;
    private PlayerMovement playerMovement;
    private Rigidbody2D playerRb;
    private Collider2D[] playerColliders;
    private SpriteRenderer playerSprite;
    private Animator playerAnimator;
    private JoystickVibration joystickVibration;

    private bool isMounted = false;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 aimInput;
    private Vector2 lastDirection = Vector2.down;
    private float defaultSpeed;
    private int defaultSortingOrder;
    private Vector3 horsePositionBeforeMount;

    private AudioSource audioSource;
    private float currentRunTime;
    private float currentCooldownTime;
    private bool isCooldown;

    private Collider2D horseCollider;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        if (horseSpriteRenderer == null)
            horseSpriteRenderer = GetComponent<SpriteRenderer>();

        if (horseAnimator == null)
            horseAnimator = GetComponent<Animator>();

        horseCollider = GetComponent<Collider2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        defaultSpeed = moveSpeed;

        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            playerRb = player.GetComponent<Rigidbody2D>();
            playerColliders = player.GetComponentsInChildren<Collider2D>(true);
            playerSprite = player.GetComponent<SpriteRenderer>();
            playerAnimator = player.GetComponent<Animator>();
            joystickVibration = player.GetComponent<JoystickVibration>();

            if (playerSprite != null)
                defaultSortingOrder = playerSprite.sortingOrder;
        }

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void Update()
    {
        if (player == null) return;

        if (isMounted)
        {
            HandleInput();
            HandleFatigue();
            HandleAudio();
            UpdateMountedAnimations();
        }
        else
        {
            CheckForMount();
            StopAudio();
        }
    }

    private void FixedUpdate()
    {
        if (isMounted)
        {
            Move();
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void HandleInput()
    {
        // Input de movimiento - CORREGIDO
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        // Input de aim - CORREGIDO: Ahora lee los ejes correctamente
        if (useAimWhileMounted)
        {
            // Leer inputs raw del joystick derecho
            float rawAimX = Input.GetAxisRaw("AimHorizontal");
            float rawAimY = Input.GetAxisRaw("AimVertical");

            // Asignar correctamente: X es horizontal, Y es vertical
            aimInput.x = rawAimX;
            aimInput.y = rawAimY;

            // Actualizar última dirección (prioridad: aim > movimiento)
            if (aimInput.sqrMagnitude > 0.1f)
            {
                lastDirection = aimInput.normalized;
            }
            else if (moveInput.sqrMagnitude > 0.1f)
            {
                lastDirection = moveInput.normalized;
            }
        }
        else
        {
            // Sin aim, usar solo movimiento
            if (moveInput.sqrMagnitude > 0.1f)
            {
                lastDirection = moveInput.normalized;
            }
        }

        if (Input.GetKeyDown(interactKey))
        {
            Dismount();
        }
    }

    private void HandleFatigue()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        if (isCooldown)
        {
            currentCooldownTime -= Time.deltaTime;
            if (currentCooldownTime <= 0)
            {
                isCooldown = false;
                currentRunTime = 0;
            }
        }
        else if (isMoving)
        {
            currentRunTime += Time.deltaTime;
            if (currentRunTime >= maxRunTime)
            {
                isCooldown = true;
                currentCooldownTime = cooldownTime;
            }
        }
        else
        {
            currentRunTime = Mathf.Max(0, currentRunTime - Time.deltaTime);
        }
    }

    private void HandleAudio()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        if (isMoving && !isCooldown)
        {
            if (!audioSource.isPlaying && runClip != null)
            {
                audioSource.clip = runClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            StopAudio();
        }
    }

    private void StopAudio()
    {
        if (audioSource.isPlaying && audioSource.clip == runClip)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }

    private void CheckForMount()
    {
        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(distance <= interactionRange);
        }

        if (distance <= interactionRange && Input.GetKeyDown(interactKey))
        {
            Mount();
        }
    }

    private void Mount()
    {
        if (mountClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(mountClip);
        }

        horsePositionBeforeMount = transform.position;
        isMounted = true;

        player.transform.SetParent(transform);
        player.transform.localPosition = mountOffset;

        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero;
            playerRb.isKinematic = true;
            playerRb.simulated = false;
        }

        foreach (var col in playerColliders)
        {
            if (col != null) col.enabled = false;
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerSprite != null && horseSpriteRenderer != null)
        {
            playerSprite.sortingOrder = horseSpriteRenderer.sortingOrder + playerSortingOrderOffset;
        }

        if (hideHorseWhenMounted)
        {
            HideHorse();
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsMounted", true);
            playerAnimator.SetBool("MountedMoving", false);
            playerAnimator.SetFloat("MoveX", 0);
            playerAnimator.SetFloat("MoveY", 0);
        }

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void Dismount()
    {
        if (dismountClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(dismountClip);
        }

        isMounted = false;

        if (hideHorseWhenMounted)
        {
            ShowHorse();
        }

        Vector3 dismountPosition = transform.position + new Vector3(dismountDistance, 0, 0);

        RaycastHit2D hit = Physics2D.CircleCast(dismountPosition, 0.3f, Vector2.zero, 0f);
        if (hit.collider != null && hit.collider.gameObject != player)
        {
            dismountPosition = transform.position + new Vector3(-dismountDistance, 0, 0);
        }

        player.transform.SetParent(null);
        player.transform.position = dismountPosition;
        player.transform.rotation = Quaternion.identity;

        if (playerRb != null)
        {
            playerRb.isKinematic = false;
            playerRb.simulated = true;
            playerRb.velocity = Vector2.zero;
        }

        foreach (var col in playerColliders)
        {
            if (col != null) col.enabled = true;
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        if (playerSprite != null)
        {
            playerSprite.sortingOrder = defaultSortingOrder;
            if (useFlipForPlayer) playerSprite.flipX = false;
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsMounted", false);
            playerAnimator.SetBool("MountedMoving", false);
        }

        if (horseAnimator != null)
        {
            horseAnimator.SetBool("IsMoving", false);
            horseAnimator.SetFloat("MoveX", 0);
            horseAnimator.SetFloat("MoveY", 0);
        }

        moveSpeed = defaultSpeed;
        moveInput = Vector2.zero;
        aimInput = Vector2.zero;
        StopAudio();
    }

    private void HideHorse()
    {
        if (horseSpriteRenderer != null)
        {
            horseSpriteRenderer.enabled = false;
        }

        if (horseCollider != null)
        {
            horseCollider.enabled = false;
        }

        if (horseAnimator != null)
        {
            horseAnimator.enabled = false;
        }
    }

    private void ShowHorse()
    {
        if (horseSpriteRenderer != null)
        {
            horseSpriteRenderer.enabled = true;
        }

        if (horseCollider != null)
        {
            horseCollider.enabled = true;
        }

        if (horseAnimator != null)
        {
            horseAnimator.enabled = true;
        }
    }

    private void Move()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            float currentSpeed = isCooldown ?
                defaultSpeed * mountedSpeedMultiplier * cooldownSpeedMultiplier :
                defaultSpeed * mountedSpeedMultiplier;

            rb.MovePosition(rb.position + moveInput.normalized * currentSpeed * Time.fixedDeltaTime);

            if (joystickVibration != null && !isCooldown)
                joystickVibration.OnRun();
        }
    }

    private void UpdateMountedAnimations()
    {
        if (playerAnimator == null) return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        // Determinar qué dirección usar para las animaciones - CORREGIDO
        Vector2 animDirection;

        if (useAimWhileMounted && aimInput.sqrMagnitude > 0.1f)
        {
            // Si hay aim, usar aim para las animaciones
            animDirection = aimInput.normalized;
        }
        else if (isMoving)
        {
            // Si no hay aim pero se está moviendo, usar movimiento
            animDirection = moveInput.normalized;
        }
        else
        {
            // Si no se mueve, usar última dirección
            animDirection = lastDirection;
        }

        // Actualizar parámetros del jugador montado - CORREGIDO
        playerAnimator.SetBool("MountedMoving", isMoving);
        playerAnimator.SetFloat("MoveX", animDirection.x);  // X es X
        playerAnimator.SetFloat("MoveY", animDirection.y);  // Y es Y
        playerAnimator.SetFloat("LastMoveX", animDirection.x);
        playerAnimator.SetFloat("LastMoveY", animDirection.y);

        // Flip del jugador basado en dirección de animación
        if (useFlipForPlayer && playerSprite != null && Mathf.Abs(animDirection.x) > 0.01f)
        {
            playerSprite.flipX = animDirection.x < 0;
        }

        // DEBUG: Descomentar para verificar valores
        // Debug.Log($"Move: ({moveInput.x:F2}, {moveInput.y:F2}) | Aim: ({aimInput.x:F2}, {aimInput.y:F2}) | Anim: ({animDirection.x:F2}, {animDirection.y:F2})");
    }

    // ==================== MÉTODOS PÚBLICOS ====================

    public bool IsMounted()
    {
        return isMounted;
    }

    public float GetFatiguePercentage()
    {
        if (isCooldown) return 0f;
        return 1f - (currentRunTime / maxRunTime);
    }

    public bool IsInCooldown()
    {
        return isCooldown;
    }

    public Vector2 GetLastDirection()
    {
        return lastDirection;
    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    public Vector2 GetAimInput()
    {
        return aimInput;
    }

    public void SetInteractionRange(float range)
    {
        interactionRange = range;
    }

    public void SetInteractKey(KeyCode key)
    {
        interactKey = key;
    }

    public void SetMountSpeed(float speed)
    {
        moveSpeed = speed;
        defaultSpeed = speed;
    }

    public void SetMountedSpeedMultiplier(float multiplier)
    {
        mountedSpeedMultiplier = multiplier;
    }

    public void SetUseAim(bool useAim)
    {
        useAimWhileMounted = useAim;
    }

    public void ForceMount()
    {
        if (!isMounted && player != null)
            Mount();
    }

    public void ForceDismount()
    {
        if (isMounted)
            Dismount();
    }

    public Vector3 GetHorsePosition()
    {
        return transform.position;
    }

    // Para debugging
    private void OnDrawGizmosSelected()
    {
        // Dibujar rango de interacción
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Dibujar posición de desmontaje
        if (isMounted)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + new Vector3(dismountDistance, 0, 0), 0.3f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position + new Vector3(-dismountDistance, 0, 0), 0.3f);
        }

        // Dibujar dirección de aim (ROJO)
        if (isMounted && useAimWhileMounted && aimInput.sqrMagnitude > 0.1f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, aimInput.normalized * 2f);
            Gizmos.DrawWireSphere(transform.position + (Vector3)(aimInput.normalized * 2f), 0.2f);
        }

        // Dibujar dirección de movimiento (CYAN)
        if (isMounted && moveInput.sqrMagnitude > 0.1f)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, moveInput.normalized * 1.5f);
            Gizmos.DrawWireSphere(transform.position + (Vector3)(moveInput.normalized * 1.5f), 0.15f);
        }
    }
}