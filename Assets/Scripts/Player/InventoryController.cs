using UnityEngine;

public class InventoryController : MonoBehaviour
{
[Header("UI Settings")]
[Tooltip("Drag the inventory Canvas GameObject here.")]
    [SerializeField] private GameObject inventoryCanvas;

[Header("Audio Settings")]
[Tooltip("Sound when opening inventory.")]
    [SerializeField] private AudioClip openSound;
[Tooltip("Sound when closing inventory.")]
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
        if (Input.GetButtonDown("Inventory"))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        if (inventoryCanvas == null)
        {
            Debug.LogWarning("Inventory Canvas is not assigned in InventoryController.");
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
