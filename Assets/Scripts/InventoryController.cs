using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("Arrastra aquí el GameObject del Canvas que quieres mostrar/ocultar.")]
    [SerializeField] private GameObject inventoryCanvas;

    [Header("Audio Settings")]
    [Tooltip("Sonido al abrir el inventario.")]
    [SerializeField] private AudioClip openSound;
    [Tooltip("Sonido al cerrar el inventario.")]
    [SerializeField] private AudioClip closeSound;

    private AudioSource audioSource;
    private bool isInventoryOpen = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        // Usa el botón "Inventory" definido en el Input Manager clásico
        if (Input.GetButtonDown("Inventory"))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        if (inventoryCanvas == null)
        {
            Debug.LogWarning("El Canvas del inventario no está asignado en el InventoryController.");
            return;
        }

        isInventoryOpen = !isInventoryOpen;
        inventoryCanvas.SetActive(isInventoryOpen);

        if (isInventoryOpen && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }
        else if (!isInventoryOpen && closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }
    }
}
