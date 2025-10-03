using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("Arrastra aquí el GameObject del Canvas que quieres mostrar/ocultar.")]
    [SerializeField] private GameObject inventoryCanvas;

    [Header("Input Settings")]
    [Tooltip("Acción de input para abrir/cerrar el inventario. Por defecto, el botón Norte del gamepad.")]
    [SerializeField] private InputAction openInventoryAction;

    [Header("Audio Settings")]
    [Tooltip("Sonido al abrir el inventario.")]
    [SerializeField] private AudioClip openSound;
    [Tooltip("Sonido al cerrar el inventario.")]
    [SerializeField] private AudioClip closeSound;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (openInventoryAction.bindings.Count == 0)
        {
            openInventoryAction.AddBinding("<Gamepad>/buttonNorth");
        }
    }

    private void OnEnable()
    {
        openInventoryAction.Enable();
        openInventoryAction.performed += ToggleInventory;
    }

    private void OnDisable()
    {
        openInventoryAction.performed -= ToggleInventory;
        openInventoryAction.Disable();
    }

    private void ToggleInventory(InputAction.CallbackContext context)
    {
        if (inventoryCanvas == null)
        {
            Debug.LogWarning("El Canvas del inventario no está asignado en el InventoryController.");
            return;
        }

        bool isNowActive = !inventoryCanvas.activeSelf;
        inventoryCanvas.SetActive(isNowActive);

        if (isNowActive && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }
        else if (!isNowActive && closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }
    }
}
