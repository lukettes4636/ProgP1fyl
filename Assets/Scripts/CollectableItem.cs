using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class CollectableItem : MonoBehaviour
{
    private string itemName = "Resource";
    private int itemAmount = 1;

    [SerializeField] private float initialDelay = 0.2f;
    [SerializeField] private float attractionSpeed = 5f;

    private bool canBeCollected = false;
    private Transform playerTransform;
    private PlayerActionController playerActionController;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        playerActionController = FindObjectOfType<PlayerActionController>();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();

        StartCoroutine(StartCollectionDelay());
    }

    public void Initialize(string name, int amount, Sprite sprite)
    {
        itemName = name;
        itemAmount = amount;

        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    private IEnumerator StartCollectionDelay()
    {
        yield return new WaitForSeconds(initialDelay);
        canBeCollected = true;
    }

    private void Update()
    {
        if (canBeCollected && playerTransform != null && attractionSpeed > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, attractionSpeed * Time.deltaTime);
        }
    }

    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float soundVolume = 0.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (canBeCollected && other.CompareTag("Player"))
        {
            if (playerActionController != null)
            {
                playerActionController.CollectResource(itemName, itemAmount);
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
                }
            }
            Destroy(gameObject);
        }
    }
}
