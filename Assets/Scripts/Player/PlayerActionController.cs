using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

[RequireComponent(typeof(Animator))]
public class PlayerActionController : MonoBehaviour
{
    public enum EquipType
    {
        None, Sword, Axe, Pickaxe, Plow, WateringCan, Bow, Torch,
        Seed1, Seed2
    }

    [FormerlySerializedAs("equipActual")]
    [SerializeField] private EquipType currentEquip = EquipType.None;
    [SerializeField] private int baseDamage = 20;
    [SerializeField] private GameObject damageHitbox;

    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;

    private Dictionary<string, int> inventory = new Dictionary<string, int>();
    [SerializeField] private List<string> inventoryDisplay = new List<string>();

    private readonly Dictionary<EquipType, string> seedItemNames = new Dictionary<EquipType, string>
    {
        { EquipType.Seed1, "SunflowerSeeds" },
        { EquipType.Seed2, "OnionSeeds" }
    };

    private Animator animator;
    private PlayerMovement playerMovement;
    private DamageHitbox hitboxScript;

    [FormerlySerializedAs("plowManager")]
    [SerializeField] private PlowManager plowManager;
    [FormerlySerializedAs("tileCursorController")]
    [SerializeField] private TileCursorController tileCursorController;

    [SerializeField] private AudioClip[] attackSounds;
    [FormerlySerializedAs("treeCuttingSounds")]
    [SerializeField] private AudioClip[] woodcutSounds;
    [SerializeField] private AudioClip[] miningSounds;
    [SerializeField] private AudioClip[] plowSounds;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private float audioVolume = 1.0f;
    [SerializeField] private float attackVolume = 1.0f;
    [FormerlySerializedAs("treeCuttingVolume")]
    [SerializeField] private float woodcutVolume = 1.0f;
    [SerializeField] private float miningVolume = 1.0f;
    [SerializeField] private float plowVolume = 1.0f;
    [SerializeField] private float shootVolume = 1.0f;
    [SerializeField] private float dashVolume = 1.0f;
    [SerializeField] private float pitchVariation = 0.1f;

    private AudioSource audioSource;
    [FormerlySerializedAs("enAccion")]
    [SerializeField] private bool isActing = false;

    [SerializeField] private float maxActionDuration = 0.6f;
    private float actionTimer = 0f;

    [FormerlySerializedAs("lightPlayer")]
    [SerializeField] private GameObject playerLight;
    [SerializeField] private GameObject torchSprite;

    [FormerlySerializedAs("cicloDiaNoche")]
    [SerializeField] private DayNightCycle dayNightCycle;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();

        if (damageHitbox != null)
            hitboxScript = damageHitbox.GetComponent<DamageHitbox>();

        plowManager = PlowManager.Instance;
        if (plowManager == null)
            plowManager = FindObjectOfType<PlowManager>();

        tileCursorController = FindObjectOfType<TileCursorController>();

        foreach (var item in seedItemNames)
        {
            inventory.Add(item.Value, 10);
        }
        if (!inventory.ContainsKey("WateringCan"))
            inventory.Add("WateringCan", 1);
        UpdateInventoryDisplay();
    }

    private void Update()
    {
        if (isActing)
        {
            actionTimer -= Time.deltaTime;
            if (actionTimer <= 0f)
            {
                FinishActionState();
            }
            return;
        }

        if (Input.GetButtonDown("Action"))
            HandleAction();

        UpdateContinuousPlayerLightVisibility();
    }

    private void HandleAction()
    {
        if (isActing || currentEquip == EquipType.None) return;

        bool isSeedEquipped = currentEquip == EquipType.Seed1 || currentEquip == EquipType.Seed2;

        if (isSeedEquipped)
        {
            if (!HasItem(seedItemNames[currentEquip]))
            {
                return;
            }
            ExecuteInstantSeedAction();
            return;
        }

        isActing = true;
        playerMovement.SetIsActing(true);
        actionTimer = maxActionDuration;

        Vector2 actionDirection = playerMovement.GetLastDirection();

        animator.SetFloat("MoveX", actionDirection.x);
        animator.SetFloat("MoveY", actionDirection.y);
        animator.SetFloat("LastMoveX", actionDirection.x);
        animator.SetFloat("LastMoveY", actionDirection.y);

        switch (currentEquip)
        {
            case EquipType.Sword:
                PlayAttackSound();
                animator.SetInteger("AttackIndex", UnityEngine.Random.Range(1, 4));
                animator.SetBool("Atacando", true);
                ActivateHitbox();
                break;

            case EquipType.Axe:
                PlayWoodcutSound();
                animator.SetBool("Talar", true);
                ActivateHitbox();
                break;

            case EquipType.Pickaxe:
                PlayMiningSound();
                animator.SetBool("Minar", true);
                ActivateHitbox();
                break;

            case EquipType.Plow:
                PlayPlowSound();
                animator.SetBool("Arar", true);
                break;

            case EquipType.WateringCan:
                animator.SetBool("Regar", true);
                break;

            case EquipType.Bow:
                PlayShootSound();
                animator.SetBool("Disparar", true);
                break;

            case EquipType.Torch:
                break;
        }
    }

    private void ExecuteInstantSeedAction()
    {
        bool isSeedEquipped = currentEquip == EquipType.Seed1 || currentEquip == EquipType.Seed2;
        if (!isSeedEquipped || plowManager == null || tileCursorController == null)
            return;

        Vector3Int cellPos = tileCursorController.GetCurrentCellPosition();

        if (cellPos.x == 999)
        {
            return;
        }

        int seedIndex = (int)currentEquip - (int)EquipType.Seed1;
        bool wasPlanted = plowManager.PlantSeedAt(cellPos, seedIndex);

        if (wasPlanted)
        {
            string seedItemName = seedItemNames[currentEquip];
            RemoveItem(seedItemName, 1);
        }
    }

    public void ActivateHitbox()
    {
        if (hitboxScript != null)
        {
            hitboxScript.ActivateHitbox();
        }
    }

    public void DeactivateHitbox()
    {
        if (hitboxScript != null)
        {
            hitboxScript.DeactivateHitbox();
        }
    }

    [SerializeField] private float shootOffsetDistance = 1.0f;
    [SerializeField] private float shootOffsetY = 0.5f;

    public void ShootArrow()
    {
        if (arrowPrefab == null)
        {
            return;
        }
        
        Vector2 shootDirection = playerMovement.GetLastDirection();
        Vector3 spawnPosition;

        if (arrowSpawnPoint != null)
        {
            spawnPosition = arrowSpawnPoint.position;
        }
        else
        {
            Vector3 offsetVector = (Vector3)shootDirection * shootOffsetDistance;
            offsetVector.y += shootOffsetY;
            spawnPosition = transform.position + offsetVector;
        }

        GameObject arrowObject = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);
        Arrow arrowScript = arrowObject.GetComponent<Arrow>();
        if (arrowScript != null)
        {
            arrowScript.Launch(shootDirection);
        }
    }

    public void ExecutePlowAction()
    {
        if (currentEquip != EquipType.Plow || plowManager == null || tileCursorController == null)
        {
            return;
        }

        Vector3Int cellPos = tileCursorController.GetCurrentCellPosition();
        if (cellPos.x == 999) return;

        bool isReadyToHarvest = plowManager.IsCropReadyToHarvest(cellPos);

        if (isReadyToHarvest)
            plowManager.HarvestAt(cellPos);
        else
            plowManager.PlowAt(cellPos);
    }

    public void ExecuteWateringAction()
    {
        if (currentEquip != EquipType.WateringCan || plowManager == null || tileCursorController == null)
        {
            return;
        }

        Vector3Int cellPos = tileCursorController.GetCurrentCellPosition();
        if (cellPos.x == 999) return;

        plowManager.WaterAt(cellPos);
    }

    public void FinishActionState()
    {
        isActing = false;
        if (playerMovement != null) playerMovement.SetIsActing(false);

        animator.SetBool("Atacando", false);
        animator.SetBool("Talar", false);
        animator.SetBool("Minar", false);
        animator.SetBool("Arar", false);
        animator.SetBool("Regar", false);
        animator.SetBool("Disparar", false);
        animator.SetInteger("AttackIndex", 0);
    }

    public void EndActionState()
    {
        FinishActionState();
    }

    private void PlayAttackSound()
    {
        if (attackSounds != null && attackSounds.Length > 0 && audioSource != null)
        {
            audioSource.pitch = 1f;
            AudioClip clip = attackSounds[UnityEngine.Random.Range(0, attackSounds.Length)];
            audioSource.PlayOneShot(clip, audioVolume * attackVolume * 0.67f);
        }
    }

    private void PlayWoodcutSound()
    {
        if (woodcutSounds != null && woodcutSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = woodcutSounds[UnityEngine.Random.Range(0, woodcutSounds.Length)];
            audioSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(clip, audioVolume * woodcutVolume);
        }
    }

    private void PlayMiningSound()
    {
        if (miningSounds != null && miningSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = miningSounds[UnityEngine.Random.Range(0, miningSounds.Length)];
            audioSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(clip, audioVolume * miningVolume);
        }
    }

    private void PlayPlowSound()
    {
        if (plowSounds != null && plowSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = plowSounds[UnityEngine.Random.Range(0, plowSounds.Length)];
            audioSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(clip, audioVolume * plowVolume);
        }
    }

    private void PlayShootSound()
    {
        if (shootSound != null && audioSource != null)
        {
            audioSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(shootSound, audioVolume * shootVolume);
        }
    }

    public void PlayDashSound()
    {
        if (dashSound != null && audioSource != null)
        {
            audioSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(dashSound, audioVolume * dashVolume);
        }
    }

    public void CollectResource(string resourceName, int amount)
    {
        if (inventory.ContainsKey(resourceName))
            inventory[resourceName] += amount;
        else
            inventory.Add(resourceName, amount);

        UpdateInventoryDisplay();
    }

    public void RemoveItem(string itemName, int amount)
    {
        if (inventory.ContainsKey(itemName))
        {
            inventory[itemName] -= amount;
            if (inventory[itemName] <= 0)
                inventory.Remove(itemName);

            UpdateInventoryDisplay();
        }
    }

    private void UpdateInventoryDisplay()
    {
        inventoryDisplay.Clear();
        foreach (var item in inventory)
            inventoryDisplay.Add($"{item.Key}: {item.Value}");
    }

    public void SetEquipment(EquipType newEquip)
    {
        if (isActing) return;
        currentEquip = newEquip;

        UpdatePlayerLightVisibility();
    }

    public int GetBaseDamage()
    {
        return baseDamage;
    }

    public EquipType GetCurrentEquip()
    {
        return currentEquip;
    }

    public bool HasItem(string itemName)
    {
        return inventory.ContainsKey(itemName) && inventory[itemName] > 0;
    }

    public int GetItemCount(string itemName)
    {
        return inventory.TryGetValue(itemName, out var amount) ? amount : 0;
    }

    private void UpdatePlayerLightVisibility()
    {
        bool hasTorchEquipped = currentEquip == EquipType.Torch;
        bool isNight = IsNight();

        bool shouldBeOn = hasTorchEquipped && isNight;

        if (torchSprite != null)
        {
            torchSprite.SetActive(hasTorchEquipped);
        }

        if (playerLight != null)
        {
            if (playerLight.activeSelf != shouldBeOn)
            {
                playerLight.SetActive(shouldBeOn);
            }
        }
    }

    private void UpdateContinuousPlayerLightVisibility()
    {
        bool hasTorchEquipped = currentEquip == EquipType.Torch;

        if (torchSprite != null)
        {
            torchSprite.SetActive(hasTorchEquipped);
        }

        if (playerLight != null && dayNightCycle != null)
        {
            bool isNight = dayNightCycle.IsNight();
            bool shouldBeOn = hasTorchEquipped && isNight;

            if (playerLight.activeSelf != shouldBeOn)
            {
                playerLight.SetActive(shouldBeOn);
            }
        }
    }

    private bool IsNight()
    {
        if (dayNightCycle != null)
            return dayNightCycle.IsNight();
        return false;
    }
}
