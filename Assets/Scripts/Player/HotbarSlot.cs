using UnityEngine;
using UnityEngine.UI;

// Script para cada slot individual del hotbar
public class HotbarSlot : MonoBehaviour
{
    [Header("Referencias UI (Arrastra desde el Inspector)")]
    [SerializeField] private Image iconImage; // Imagen del icono de la herramienta
    [SerializeField] private Image selectionImage; // Imagen que muestra cuando está seleccionado
    [SerializeField] private Text numberText; // Texto que muestra el número del slot (1-6)

    private int slotIndex; // Índice de este slot
    private HotbarUI hotbarUI; // Referencia al hotbar principal
    private PlayerActionController.EquipType equipType = PlayerActionController.EquipType.None;

    // Inicializa el slot con su índice y referencia al hotbar
    public void Initialize(int index, HotbarUI hotbar)
    {
        slotIndex = index;
        hotbarUI = hotbar;

        // Mostrar el número del slot (1-6)
        if (numberText != null)
        {
            numberText.text = (index + 1).ToString();
        }

        // Ocultar el icono inicialmente
        if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        // Ocultar selección inicialmente
        if (selectionImage != null)
        {
            selectionImage.enabled = false;
        }
    }

    // Establece el equipo de este slot
    public void SetEquip(PlayerActionController.EquipType type, Sprite icon)
    {
        equipType = type;

        if (iconImage != null)
        {
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }
    }

    // Cambia el aspecto visual cuando el slot está seleccionado
    public void SetSelected(bool selected, Color color)
    {
        if (selectionImage != null)
        {
            selectionImage.enabled = selected;
            selectionImage.color = color;
        }
    }

    // Devuelve el tipo de equipo de este slot
    public PlayerActionController.EquipType GetEquipType()
    {
        return equipType;
    }
}