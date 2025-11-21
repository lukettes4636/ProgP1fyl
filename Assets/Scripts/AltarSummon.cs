using UnityEngine;
using UnityEngine.UI;

public class AltarSummon : MonoBehaviour
{
    [SerializeField] private PlayerActionController player;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int requiredMineralCount = 5;
    [SerializeField] private string mineralName = "Mineral";
    [SerializeField] private bool requireWeaponEquipped = false;

    [SerializeField] private GameObject promptCanvas;
    [SerializeField] private Text promptText;

    private bool playerInRange = false;

    private void Awake()
    {
        if (promptText == null && promptCanvas != null)
        {
            promptText = promptCanvas.GetComponentInChildren<Text>();
        }
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (player == null) return;

        if (promptCanvas != null)
        {
            if (!promptCanvas.activeSelf) promptCanvas.SetActive(true);
            if (promptText != null)
            {
                int have = player.GetItemCount(mineralName);
                promptText.text = "Presiona Acción para invocar (" + have + "/" + requiredMineralCount + " " + mineralName + ")";
            }
        }

        if (Input.GetButtonDown("Action"))
        {
            int count = player.GetItemCount(mineralName);
            if (count >= requiredMineralCount && enemyPrefab != null)
            {
                player.RemoveItem(mineralName, requiredMineralCount);
                Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
                Instantiate(enemyPrefab, pos, Quaternion.identity);
                if (promptCanvas != null) promptCanvas.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (player == null)
            {
                player = other.GetComponent<PlayerActionController>();
            }
            if (promptCanvas != null) promptCanvas.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptCanvas != null) promptCanvas.SetActive(false);
        }
    }
}