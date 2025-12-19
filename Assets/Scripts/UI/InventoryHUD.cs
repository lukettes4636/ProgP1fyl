using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class InventoryHUD : MonoBehaviour
{
    [SerializeField] private PlayerActionController player;

    [FormerlySerializedAs("maderaIcon")]
    [SerializeField] private Image woodIcon;
    [FormerlySerializedAs("maderaText")]
    [SerializeField] private TMP_Text woodText;

    [FormerlySerializedAs("piedraIcon")]
    [SerializeField] private Image stoneIcon;
    [FormerlySerializedAs("piedraText")]
    [SerializeField] private TMP_Text stoneText;

    [FormerlySerializedAs("girasolIcon")]
    [SerializeField] private Image sunflowerIcon;
    [FormerlySerializedAs("girasolText")]
    [SerializeField] private TMP_Text sunflowerText;

    [FormerlySerializedAs("uvaIcon")]
    [SerializeField] private Image grapeIcon;
    [FormerlySerializedAs("uvaText")]
    [SerializeField] private TMP_Text grapeText;

    [SerializeField] private Image mineralIcon;
    [SerializeField] private TMP_Text mineralText;

    void Update()
    {
        if (player == null) player = FindObjectOfType<PlayerActionController>();
        if (player == null) return;

        if (woodText != null) woodText.text = player.GetItemCount("Wood").ToString();
        if (stoneText != null) stoneText.text = player.GetItemCount("Stone").ToString();
        if (sunflowerText != null) sunflowerText.text = player.GetItemCount("Sunflower").ToString();
        if (grapeText != null) grapeText.text = player.GetItemCount("Grape").ToString();
        if (mineralText != null) mineralText.text = player.GetItemCount("Mineral").ToString();
    }
}