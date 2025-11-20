using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
[Header("Slots - Icons (drag the 9 icons in order)")]
    [SerializeField] private Image slot1Icon;
    [SerializeField] private Image slot2Icon;
    [SerializeField] private Image slot3Icon;
    [SerializeField] private Image slot4Icon;
    [SerializeField] private Image slot5Icon;
    [SerializeField] private Image slot6Icon;
    [SerializeField] private Image slot7Icon;
    [SerializeField] private Image slot8Icon;
    [SerializeField] private Image slot9Icon;

[Header("Slots - Selection frames (drag the 9 frames in order)")]
    [SerializeField] private GameObject slot1Selection;
    [SerializeField] private GameObject slot2Selection;
    [SerializeField] private GameObject slot3Selection;
    [SerializeField] private GameObject slot4Selection;
    [SerializeField] private GameObject slot5Selection;
    [SerializeField] private GameObject slot6Selection;
    [SerializeField] private GameObject slot7Selection;
    [SerializeField] private GameObject slot8Selection;
    [SerializeField] private GameObject slot9Selection;

[Header("Tool Sprites (order: Sword, Axe, Pickaxe, Plow, WateringCan, Bow, Torch, Seed1, Seed2)")]
    [SerializeField] private Sprite espadaSprite;
    [SerializeField] private Sprite hachaSprite;
    [SerializeField] private Sprite picoSprite;
    [SerializeField] private Sprite aradoSprite;
    [SerializeField] private Sprite regaderaSprite;
    [SerializeField] private Sprite arcoSprite;
    [SerializeField] private Sprite antorchaSprite;
    [SerializeField] private Sprite semilla1Sprite;
    [SerializeField] private Sprite semilla2Sprite;

    private int slotActual = 0; 
    private PlayerActionController playerController;

    private bool nextWeaponPressed = false;
    private bool prevWeaponPressed = false;

    void Start()
    {
        playerController = FindObjectOfType<PlayerActionController>();

        if (playerController == null)
        {
            Debug.LogError("PlayerActionController not found!");
            return;
        }

        slot1Icon.sprite = espadaSprite;
        slot2Icon.sprite = hachaSprite;
        slot3Icon.sprite = picoSprite;
        slot4Icon.sprite = aradoSprite;
        slot5Icon.sprite = regaderaSprite;
        slot6Icon.sprite = arcoSprite;
        slot7Icon.sprite = antorchaSprite;
        slot8Icon.sprite = semilla1Sprite;
        slot9Icon.sprite = semilla2Sprite;

        ActualizarSeleccion();
    }

    void Update()
    {
        float axisValue = Input.GetAxis("ChangeWeapon");

        if (axisValue > 0.5f && !nextWeaponPressed)
        {
            nextWeaponPressed = true;
            CambiarSlotAdelante();
        }

        if (axisValue <= 0.5f && nextWeaponPressed)
        {
            nextWeaponPressed = false;
        }

        if (axisValue < -0.5f && !prevWeaponPressed)
        {
            prevWeaponPressed = true;
            CambiarSlotAtras();
        }

        if (axisValue >= -0.5f && prevWeaponPressed)
        {
            prevWeaponPressed = false;
        }
    }

    void CambiarSlotAdelante()
    {
        slotActual = slotActual + 1;

        if (slotActual > 8)
        {
            slotActual = 0;
        }

        ActualizarSeleccion();
    }

    void CambiarSlotAtras()
    {
        slotActual = slotActual - 1;

        if (slotActual < 0)
        {
            slotActual = 8;
        }

        ActualizarSeleccion();
    }

    void ActualizarSeleccion()
    {
        slot1Selection.SetActive(false);
        slot2Selection.SetActive(false);
        slot3Selection.SetActive(false);
        slot4Selection.SetActive(false);
        slot5Selection.SetActive(false);
        slot6Selection.SetActive(false);
        slot7Selection.SetActive(false);
        slot8Selection.SetActive(false);
        slot9Selection.SetActive(false);

        if (slotActual == 0)
        {
            slot1Selection.SetActive(true);
            playerController.SetEquip(PlayerActionController.EquipType.Espada);
        }
        else if (slotActual == 1)
        {
            slot2Selection.SetActive(true);
            playerController.SetEquip(PlayerActionController.EquipType.Hacha);
        }
        else if (slotActual == 2)
        {
            slot3Selection.SetActive(true);
            playerController.SetEquip(PlayerActionController.EquipType.Pico);
        }
        else if (slotActual == 3)
        {
            slot4Selection.SetActive(true);
            playerController.SetEquip(PlayerActionController.EquipType.Arado);
        }
        else if (slotActual == 4)
        {
            slot5Selection.SetActive(true);
            playerController.SetEquip(PlayerActionController.EquipType.Regadera);
        }
        else if (slotActual == 5)
        {
            slot6Selection.SetActive(true);
            playerController.SetEquip(PlayerActionController.EquipType.Arco);
        }
        else if (slotActual == 6)
        {
            slot7Selection.SetActive(true);
            playerController.SetEquip(PlayerActionController.EquipType.Antorcha);
        }
        else if (slotActual == 7)
        {
            slot8Selection.SetActive(true);
            playerController.SetEquip(PlayerActionController.EquipType.Semilla1);
        }
        else if (slotActual == 8)
        {
            slot9Selection.SetActive(true);
            playerController.SetEquip(PlayerActionController.EquipType.Semilla2);
        }
    }
}
