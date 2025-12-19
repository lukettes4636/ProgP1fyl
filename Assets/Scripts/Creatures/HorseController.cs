using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class HorseController : MonoBehaviour
{
    public float moveSpeed = 14.0f;
    public float mountedSpeedMultiplier = 1.5f;

    public float maxRunTime = 5.0f;
    public float cooldownTime = 3.0f;
    public float cooldownSpeedMultiplier = 0.5f;

    public AudioClip runClip;
    public AudioClip gallopClip;
    public AudioClip mountClip;
    public AudioClip dismountClip;

    public float interactionRange = 1.5f;
    public KeyCode interactKey = KeyCode.F;
    public GameObject interactionPrompt;

    public Vector3 mountOffset = Vector3.zero;
    public int playerSortingOrderOffset = 1;
    public float dismountDistance = 1.5f;

    public bool hideHorseWhenMounted = true;
    public bool useFlipForPlayer = true;
    public bool useAimWhileMounted = true;
    public SpriteRenderer horseSpriteRenderer;
    public Animator horseAnimator;

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
    
    private AudioSource audioSource;
    private float currentRunTime;
    private float currentCooldownTime;
    private bool isCooldown;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        if (horseSpriteRenderer == null)
            horseSpriteRenderer = GetComponent<SpriteRenderer>();

        if (horseAnimator == null)
            horseAnimator = GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.bodyType = RigidbodyType2D.Kinematic;

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
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        if (useAimWhileMounted)
        {
            float rawAimX = Input.GetAxisRaw("AimHorizontal");
            float rawAimY = Input.GetAxisRaw("AimVertical");

            aimInput.x = rawAimX;
            aimInput.y = rawAimY;

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

        if (isMoving)
        {
            AudioClip clipToPlay = (isCooldown || gallopClip == null) ? runClip : gallopClip;
            
            if (clipToPlay != null && (!audioSource.isPlaying || audioSource.clip != clipToPlay))
            {
                audioSource.clip = clipToPlay;
                audioSource.loop = true;
                audioSource.Play();
            }
            else if (clipToPlay == null)
            {
                StopAudio();
            }
        }
        else
        {
            StopAudio();
        }
    }

    private void StopAudio()
    {
        if (audioSource.isPlaying && (audioSource.clip == runClip || audioSource.clip == gallopClip))
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

        isMounted = true;
        rb.bodyType = RigidbodyType2D.Dynamic;

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
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;

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
            horseSpriteRenderer.enabled = false;

        if (horseAnimator != null)
            horseAnimator.enabled = false;
    }

    private void ShowHorse()
    {
        if (horseSpriteRenderer != null)
            horseSpriteRenderer.enabled = true;

        if (horseAnimator != null)
            horseAnimator.enabled = true;
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
        Vector2 animDirection;

        if (useAimWhileMounted && aimInput.sqrMagnitude > 0.1f)
        {
            animDirection = aimInput.normalized;
        }
        else if (isMoving)
        {
            animDirection = moveInput.normalized;
        }
        else
        {
            animDirection = lastDirection;
        }

        playerAnimator.SetBool("MountedMoving", isMoving);
        playerAnimator.SetFloat("MoveX", animDirection.x);
        playerAnimator.SetFloat("MoveY", animDirection.y);
        playerAnimator.SetFloat("LastMoveX", animDirection.x);
        playerAnimator.SetFloat("LastMoveY", animDirection.y);

        if (useFlipForPlayer && playerSprite != null && Mathf.Abs(animDirection.x) > 0.01f)
        {
            playerSprite.flipX = animDirection.x < 0;
        }
    }
}
