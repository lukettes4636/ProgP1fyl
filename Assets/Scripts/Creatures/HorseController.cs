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
    [SerializeField] private float cooldownSpeedMultiplier = 0.05f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip runClip;
    [SerializeField] private AudioClip genericClip;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 1.5f;
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    [SerializeField] private bool showInteractionPrompt = true;

    [Header("Mount Position")]
    [SerializeField] private Vector3 mountOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private int playerSortingOrderOffset = 1;

    private GameObject player;
    private PlayerMovement playerMovement;
    private Rigidbody2D playerRb;
    private Collider2D[] playerColliders;
    private SpriteRenderer playerSprite;
    private JoystickVibration joystickVibration;

    private bool isMounted = false;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    private SpriteRenderer horseSprite;
    private float defaultSpeed;
    private int defaultSortingOrder;
    
    private AudioSource audioSource;
    private float currentRunTime;
    private float currentCooldownTime;
    private bool isCooldown;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        horseSprite = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

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
            joystickVibration = player.GetComponent<JoystickVibration>();
            
            if (playerSprite != null)
                defaultSortingOrder = playerSprite.sortingOrder;
        }
    }

    private void Update()
    {
        if (player == null) return;

        if (isMounted)
        {
            HandleInput();
            HandleFatigue();
            HandleAudio();
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
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(interactKey))
        {
            Dismount();
        }

        UpdateAnimation();
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
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void CheckForMount()
    {
        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance <= interactionRange && Input.GetKeyDown(interactKey))
        {
            Mount();
        }
    }

    private void Mount()
    {
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

        if (playerSprite != null && horseSprite != null)
        {
            playerSprite.sortingOrder = horseSprite.sortingOrder + playerSortingOrderOffset;
        }
    }

    private void Dismount()
    {
        isMounted = false;
        player.transform.SetParent(null);
        player.transform.rotation = Quaternion.identity;

        if (playerRb != null)
        {
            playerRb.isKinematic = false;
            playerRb.simulated = true;
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
        }

        moveSpeed = defaultSpeed;
        StopAudio();
    }

    private void Move()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            float currentSpeed = isCooldown ? defaultSpeed * mountedSpeedMultiplier * cooldownSpeedMultiplier : defaultSpeed * mountedSpeedMultiplier;
            rb.MovePosition(rb.position + moveInput.normalized * currentSpeed * Time.fixedDeltaTime);
            
            if (joystickVibration != null && !isCooldown) joystickVibration.OnRun();
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        animator.SetBool("IsMoving", isMoving);

        if (isMoving)
        {
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);

            if (Mathf.Abs(moveInput.x) > 0.01f)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(moveInput.x);
                transform.localScale = scale;
            }
        }
    }
    
    // Public methods for external interaction
    public bool IsMounted()
    {
        return isMounted;
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
    }

    public void SetMountedSpeedMultiplier(float multiplier)
    {
        mountedSpeedMultiplier = multiplier;
    }

    public void SetMountOffset(Vector3 offset)
    {
        mountOffset = offset;
    }

    public void ForceMount()
    {
        if (!isMounted && player != null)
        {
            Mount();
        }
    }

    public void ForceDismount()
    {
        if (isMounted)
        {
            Dismount();
        }
    }
}
