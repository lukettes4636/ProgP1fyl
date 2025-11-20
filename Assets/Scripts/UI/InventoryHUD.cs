using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryHUD : MonoBehaviour
{
    [SerializeField] private PlayerActionController player;

    [SerializeField] private Image maderaIcon;
    [SerializeField] private TMP_Text maderaText;

    [SerializeField] private Image piedraIcon;
    [SerializeField] private TMP_Text piedraText;

    [SerializeField] private Image girasolIcon;
    [SerializeField] private TMP_Text girasolText;

    [SerializeField] private Image uvaIcon;
    [SerializeField] private TMP_Text uvaText;

    [SerializeField] private Image mineralIcon;
    [SerializeField] private TMP_Text mineralText;

    void Update()
    {
        if (player == null) player = FindObjectOfType<PlayerActionController>();
        if (player == null) return;

        if (maderaText != null) maderaText.text = player.GetItemCount("Madera").ToString();
        if (piedraText != null) piedraText.text = player.GetItemCount("Piedra").ToString();
        if (girasolText != null) girasolText.text = player.GetItemCount("Girasol").ToString();
        if (uvaText != null) uvaText.text = player.GetItemCount("Uva").ToString();
        if (mineralText != null) mineralText.text = player.GetItemCount("Mineral").ToString();
    }
}
