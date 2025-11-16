using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// Script principal del Hotbar - Maneja la UI y selección de herramientas
public class HotbarUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Transform slotsContainer; // El panel que contiene los slots

    [Header("Configuración")]
    [SerializeField] private int numberOfSlots = 6; // Número de espacios en el hotbar
    [SerializeField] private Color selectedColor = Color.yellow; // Color del slot seleccionado
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.3f); // Color de slots normales (semi-transparente)

    [Header("Sprites de Herramientas")]
    [SerializeField] private Sprite espadaSprite;
    [SerializeField] private Sprite hachaSprite;
    [SerializeField] private Sprite picoSprite;
    [SerializeField] private Sprite aradoSprite;
    [SerializeField] private Sprite regaderaSprite;
    [SerializeField] private Sprite semilla1Sprite;
    [SerializeField] private Sprite semilla2Sprite;
    [SerializeField] private Sprite arcoSprite;

    private List<HotbarSlot> slots = new List<HotbarSlot>(); // Lista de slots creados
    private int currentSlotIndex = 0; // Slot actualmente seleccionado
    private PlayerActionController playerController; // Referencia al controlador del jugador

    // Diccionario que asocia cada tipo de equipo con su sprite
    private Dictionary<PlayerActionController.EquipType, Sprite> equipSprites;

    // Control de input para evitar múltiples cambios
    private bool changeWeaponPressed = false;

    void Start()
    {
        // Buscar el PlayerActionController en la escena
        playerController = FindObjectOfType<PlayerActionController>();

        if (playerController == null)
        {
            Debug.LogError("No se encontró PlayerActionController en la escena!");
            return;
        }

        // Inicializar el diccionario de sprites
        InitializeEquipSprites();

        // Buscar los slots que ya existen como hijos
        FindExistingSlots();

        // Configurar las herramientas iniciales
        SetupInitialTools();

        // Seleccionar el primer slot
        SelectSlot(0);
    }

    void Update()
    {
        // Detectar el botón ChangeWeapon del joystick
        bool changeWeaponButton = Input.GetButton("ChangeWeapon");

        // Solo cambiar cuando se PRESIONA (no mientras se mantiene)
        if (changeWeaponButton && !changeWeaponPressed)
        {
            changeWeaponPressed = true;
            SelectNextSlot();
        }

        // Resetear cuando se suelta el botón
        if (!changeWeaponButton && changeWeaponPressed)
        {
            changeWeaponPressed = false;
        }
    }

    // Inicializa el diccionario que relaciona equipos con sprites
    private void InitializeEquipSprites()
    {
        equipSprites = new Dictionary<PlayerActionController.EquipType, Sprite>
        {
            { PlayerActionController.EquipType.Espada, espadaSprite },
            { PlayerActionController.EquipType.Hacha, hachaSprite },
            { PlayerActionController.EquipType.Pico, picoSprite },
            { PlayerActionController.EquipType.Arado, aradoSprite },
            { PlayerActionController.EquipType.Regadera, regaderaSprite },
            { PlayerActionController.EquipType.Semilla1, semilla1Sprite },
            { PlayerActionController.EquipType.Semilla2, semilla2Sprite },
            { PlayerActionController.EquipType.Arco, arcoSprite }
        };
    }

    // Busca los slots que ya existen como hijos del container
    private void FindExistingSlots()
    {
        slots.Clear();

        // Recorrer todos los hijos del container
        for (int i = 0; i < slotsContainer.childCount; i++)
        {
            Transform child = slotsContainer.GetChild(i);
            HotbarSlot slot = child.GetComponent<HotbarSlot>();

            if (slot != null)
            {
                slot.Initialize(i, this);
                slots.Add(slot);
            }
        }

        Debug.Log("Slots encontrados: " + slots.Count);
    }

    // Configura las herramientas iniciales en el hotbar
    private void SetupInitialTools()
    {
        // Coloca las herramientas en los slots
        if (slots.Count > 0) SetSlotEquip(0, PlayerActionController.EquipType.Espada);
        if (slots.Count > 1) SetSlotEquip(1, PlayerActionController.EquipType.Hacha);
        if (slots.Count > 2) SetSlotEquip(2, PlayerActionController.EquipType.Pico);
        if (slots.Count > 3) SetSlotEquip(3, PlayerActionController.EquipType.Arado);
        if (slots.Count > 4) SetSlotEquip(4, PlayerActionController.EquipType.Regadera);
        if (slots.Count > 5) SetSlotEquip(5, PlayerActionController.EquipType.Semilla1);
    }

    // Establece qué equipo tiene un slot específico
    public void SetSlotEquip(int slotIndex, PlayerActionController.EquipType equipType)
    {
        if (slotIndex >= 0 && slotIndex < slots.Count)
        {
            Sprite sprite = null;

            if (equipType != PlayerActionController.EquipType.None)
            {
                equipSprites.TryGetValue(equipType, out sprite);
            }

            slots[slotIndex].SetEquip(equipType, sprite);
        }
    }

    // Selecciona el siguiente slot (avanza uno)
    public void SelectNextSlot()
    {
        int nextSlot = currentSlotIndex + 1;

        // Volver al principio si llega al final
        if (nextSlot >= slots.Count)
        {
            nextSlot = 0;
        }

        SelectSlot(nextSlot);
    }

    // Selecciona un slot específico (cambia la herramienta equipada)
    public void SelectSlot(int slotIndex)
    {
        // Validar que el índice sea válido
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            return;
        }

        // Deseleccionar el slot anterior
        if (currentSlotIndex >= 0 && currentSlotIndex < slots.Count)
        {
            slots[currentSlotIndex].SetSelected(false, normalColor);
        }

        // Seleccionar el nuevo slot
        currentSlotIndex = slotIndex;
        slots[currentSlotIndex].SetSelected(true, selectedColor);

        // Cambiar el equipo en el PlayerActionController
        PlayerActionController.EquipType equipType = slots[currentSlotIndex].GetEquipType();
        ChangePlayerEquip(equipType);
    }

    // Cambia el equipo del jugador
    private void ChangePlayerEquip(PlayerActionController.EquipType newEquip)
    {
        if (playerController != null)
        {
            playerController.SetEquip(newEquip);
        }
    }
}