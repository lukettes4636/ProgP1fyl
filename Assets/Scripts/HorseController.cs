using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HorseController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float interactionRange = 1.5f;
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    private GameObject player;
    private PlayerMovement playerMovement;
    private Rigidbody2D playerRb;
    private Collider2D playerCollider;

    private bool isMounted = false;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    private SpriteRenderer playerSprite;
    private JoystickVibration joystickVibration;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Ensure Rigidbody settings
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.bodyType = RigidbodyType2D.Dynamic;

        // Find Player
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            playerRb = player.GetComponent<Rigidbody2D>();
            playerCollider = player.GetComponent<Collider2D>();
            playerSprite = player.GetComponent<SpriteRenderer>();
            joystickVibration = player.GetComponent<JoystickVibration>();
        }
    }

    private void Update()
    {
        if (player == null) return;

        if (isMounted)
        {
            HandleMountedInput();
        }
        else
        {
            CheckForMount();
        }
    }

    private void FixedUpdate()
    {
        if (isMounted)
        {
            MoveHorse();
        }
        else
        {
            // Optional: Host stays still or wanders. For now, stay still.
            rb.velocity = Vector2.zero;
        }
    }

    private void CheckForMount()
    {
        float dist = Vector2.Distance(transform.position, player.transform.position);
        if (dist <= interactionRange && Input.GetKeyDown(interactKey))
        {
            Mount();
        }
    }

    private int originalSortingOrder;

    private Collider2D[] playerColliders; // Store all colliders to re-enable them later

    private void Mount()
    {
        if (isMounted) return;
        isMounted = true;
        Debug.Log("Mounted Horse");

        // 1. Gather ALL colliders (including children)
        if (player != null)
        {
            playerColliders = player.GetComponentsInChildren<Collider2D>(true);
            foreach (var col in playerColliders)
            {
                col.enabled = false;
            }

            // 2. Handle Rigidbody
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
                playerRb.isKinematic = true; 
                playerRb.simulated = false;
            }

            // 3. Disable Player Inputs/Movement
            if (playerMovement != null)
            {
                 playerMovement.SetCanMove(false);
                 playerMovement.enabled = false;
            }

            // 4. Parent and Position - NOW SAFE because physics are off
            player.transform.SetParent(transform);
            // Position slightly up so it looks like riding
            player.transform.localPosition = new Vector3(0, 0.5f, 0); 
            
            // 5. Visual handling
            if (playerSprite != null) 
            {
                playerSprite.enabled = true;
                originalSortingOrder = playerSprite.sortingOrder;
                var horseSprite = GetComponent<SpriteRenderer>();
                if (horseSprite != null)
                {
                    playerSprite.sortingOrder = horseSprite.sortingOrder + 1;
                }
            }

            // 6. Disable Action Controller
            var actionController = player.GetComponent<PlayerActionController>();
            if (actionController != null) actionController.enabled = false;
        }
    }

    private void Dismount()
    {
        if (!isMounted) return;
        isMounted = false;
        Debug.Log("Dismounted Horse");
        
        // 1. Unparent
        if (player != null)
        {
            player.transform.SetParent(null);
            // Reset rotation just in case
            player.transform.rotation = Quaternion.identity; 
        }

        // 2. Re-enable Player Control
        if (playerMovement != null) 
        {
            playerMovement.enabled = true;
            playerMovement.SetCanMove(true);
        }
        
        // 3. Restore Physics
        if (playerRb != null)
        {
            playerRb.isKinematic = false;
            playerRb.simulated = true;
            playerRb.velocity = Vector2.zero;
        }

        // 4. Restore ALL Colliders
        if (playerColliders != null)
        {
            foreach (var col in playerColliders)
            {
                if (col != null) col.enabled = true;
            }
        }

        // 5. Restore Visuals
        if (playerSprite != null)
        {
             playerSprite.enabled = true;
             playerSprite.sortingOrder = originalSortingOrder;
        }

        var actionController = player.GetComponent<PlayerActionController>();
        if (actionController != null) actionController.enabled = true;
    }

    // UpdatePlayerPosition removed because we are parenting now


    private void HandleMountedInput()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(interactKey))
        {
            Dismount();
        }

        UpdateAnimation();
    }

    private void MoveHorse()
    {
        if (moveInput.sqrMagnitude > 0.1f)
        {
            rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);
            if (joystickVibration != null) joystickVibration.OnRun();
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        bool isMoving = moveInput.sqrMagnitude > 0.1f;
        animator.SetBool("IsMoving", isMoving);
        
        if (isMoving)
        {
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);

            // Flip sprite Logic if needed (assuming Side-view style flipping or 4-dir)
            if (Mathf.Abs(moveInput.x) > 0.1f)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(moveInput.x);
                transform.localScale = scale;
            }
        }
    }
}
