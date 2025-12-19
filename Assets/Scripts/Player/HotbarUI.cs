using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class HotbarUI : MonoBehaviour
{
    [SerializeField] private Image slot1Icon;
    [SerializeField] private Image slot2Icon;
    [SerializeField] private Image slot3Icon;
    [SerializeField] private Image slot4Icon;
    [SerializeField] private Image slot5Icon;
    [SerializeField] private Image slot6Icon;
    [SerializeField] private Image slot7Icon;
    [SerializeField] private Image slot8Icon;
    [SerializeField] private Image slot9Icon;

    [SerializeField] private GameObject slot1Selection;
    [SerializeField] private GameObject slot2Selection;
    [SerializeField] private GameObject slot3Selection;
    [SerializeField] private GameObject slot4Selection;
    [SerializeField] private GameObject slot5Selection;
    [SerializeField] private GameObject slot6Selection;
    [SerializeField] private GameObject slot7Selection;
    [SerializeField] private GameObject slot8Selection;
    [SerializeField] private GameObject slot9Selection;

    [FormerlySerializedAs("espadaSprite")]
    [SerializeField] private Sprite swordSprite;
    [FormerlySerializedAs("hachaSprite")]
    [SerializeField] private Sprite axeSprite;
    [FormerlySerializedAs("picoSprite")]
    [SerializeField] private Sprite pickaxeSprite;
    [FormerlySerializedAs("aradoSprite")]
    [SerializeField] private Sprite plowSprite;
    [FormerlySerializedAs("regaderaSprite")]
    [SerializeField] private Sprite wateringCanSprite;
    [FormerlySerializedAs("arcoSprite")]
    [SerializeField] private Sprite bowSprite;
    [FormerlySerializedAs("antorchaSprite")]
    [SerializeField] private Sprite torchSprite;
    [FormerlySerializedAs("semilla1Sprite")]
    [SerializeField] private Sprite seed1Sprite;
    [FormerlySerializedAs("semilla2Sprite")]
    [SerializeField] private Sprite seed2Sprite;

    private int currentSlot = 0; 
    private PlayerActionController playerController;

    private bool nextButtonPressed = false;
    private bool previousButtonPressed = false;

    void Start()
    {
        playerController = FindObjectOfType<PlayerActionController>();

        if (playerController == null)
        {
            return;
        }

        if (slot1Icon != null) slot1Icon.sprite = swordSprite;
        if (slot2Icon != null) slot2Icon.sprite = axeSprite;
        if (slot3Icon != null) slot3Icon.sprite = pickaxeSprite;
        if (slot4Icon != null) slot4Icon.sprite = plowSprite;
        if (slot5Icon != null) slot5Icon.sprite = wateringCanSprite;
        if (slot6Icon != null) slot6Icon.sprite = bowSprite;
        if (slot7Icon != null) slot7Icon.sprite = torchSprite;
        if (slot8Icon != null) slot8Icon.sprite = seed1Sprite;
        if (slot9Icon != null) slot9Icon.sprite = seed2Sprite;

        UpdateSelection();
    }

    void Update()
    {
        float axisValue = Input.GetAxis("ChangeWeapon");

        if (axisValue > 0.5f && !nextButtonPressed)
        {
            nextButtonPressed = true;
            ChangeSlotForward();
        }

        if (axisValue <= 0.5f && nextButtonPressed)
        {
            nextButtonPressed = false;
        }

        if (axisValue < -0.5f && !previousButtonPressed)
        {
            previousButtonPressed = true;
            ChangeSlotBackward();
        }

        if (axisValue >= -0.5f && previousButtonPressed)
        {
            previousButtonPressed = false;
        }
    }

    void ChangeSlotForward()
    {
        currentSlot = currentSlot + 1;

        if (currentSlot > 8)
        {
            currentSlot = 0;
        }

        UpdateSelection();
    }

    void ChangeSlotBackward()
    {
        currentSlot = currentSlot - 1;

        if (currentSlot < 0)
        {
            currentSlot = 8;
        }

        UpdateSelection();
    }

    void UpdateSelection()
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

        if (currentSlot == 0)
        {
            slot1Selection.SetActive(true);
            playerController.SetEquipment(PlayerActionController.EquipType.Sword);
        }
        else if (currentSlot == 1)
        {
            slot2Selection.SetActive(true);
            playerController.SetEquipment(PlayerActionController.EquipType.Axe);
        }
        else if (currentSlot == 2)
        {
            slot3Selection.SetActive(true);
            playerController.SetEquipment(PlayerActionController.EquipType.Pickaxe);
        }
        else if (currentSlot == 3)
        {
            slot4Selection.SetActive(true);
            playerController.SetEquipment(PlayerActionController.EquipType.Plow);
        }
        else if (currentSlot == 4)
        {
            slot5Selection.SetActive(true);
            playerController.SetEquipment(PlayerActionController.EquipType.WateringCan);
        }
        else if (currentSlot == 5)
        {
            slot6Selection.SetActive(true);
            playerController.SetEquipment(PlayerActionController.EquipType.Bow);
        }
        else if (currentSlot == 6)
        {
            slot7Selection.SetActive(true);
            playerController.SetEquipment(PlayerActionController.EquipType.Torch);
        }
        else if (currentSlot == 7)
        {
            slot8Selection.SetActive(true);
            playerController.SetEquipment(PlayerActionController.EquipType.Seed1);
        }
        else if (currentSlot == 8)
        {
            slot9Selection.SetActive(true);
            playerController.SetEquipment(PlayerActionController.EquipType.Seed2);
        }
    }
}

