using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))] // Aseguramos que tenga un SpriteRenderer
public class CollectableItem : MonoBehaviour
{
    // Variables inicializadas por PlowManager
    private string itemName = "Recurso";
    private int itemAmount = 1;

    [Header("Configuración de Recolección")]
    [SerializeField] private float initialDelay = 0.2f;
    [SerializeField] private float attractionSpeed = 5f;

    private bool canBeCollected = false;
    private Transform playerTransform;
    private PlayerActionController playerActionController;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        // Obtener referencias
        playerActionController = FindObjectOfType<PlayerActionController>();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>(); // Obtener el componente

        StartCoroutine(StartCollectionDelay());
    }

    // CAMBIO: Ahora recibe el Sprite que debe mostrar
    /// <summary>
    /// Inicializa la data y el sprite visual del item.
    /// </summary>
    public void Initialize(string name, int amount, Sprite sprite)
    {
        itemName = name;
        itemAmount = amount;

        // Asignar el sprite recibido
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
        // Atracción magnética
        if (canBeCollected && playerTransform != null && attractionSpeed > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, attractionSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (canBeCollected && other.CompareTag("Player"))
        {
            if (playerActionController != null)
            {
                playerActionController.CollectResource(itemName, itemAmount);
            }
            Destroy(gameObject);
        }
    }
}