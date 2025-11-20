using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public class PlayerActionController : MonoBehaviour
{
    public enum EquipType
    {
        None, Espada, Hacha, Pico, Arado, Regadera, Arco, Antorcha,
        Semilla1, Semilla2
    }

    [SerializeField] private EquipType equipActual = EquipType.None;
    [SerializeField] private int baseDamage = 20;
    [SerializeField] private GameObject damageHitbox;

    private Dictionary<string, int> inventory = new Dictionary<string, int>();
    [SerializeField] private List<string> inventoryDisplay = new List<string>();

    private readonly Dictionary<EquipType, string> SeedItemNames = new Dictionary<EquipType, string>
    {
        { EquipType.Semilla1, "SemillasDeGirasol" },
        { EquipType.Semilla2, "SemillasDeUva" }
    };

    private Animator animator;
    private PlayerMovement playerMovement;
    private DamageHitbox hitboxScript;

    [SerializeField] private PlowManager plowManager;
    [SerializeField] private TileCursorController tileCursorController;

    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private AudioClip[] treeCuttingSounds;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private float audioVolume = 1.0f;
    [SerializeField] private float attackVolume = 1.0f;
    [SerializeField] private float treeCuttingVolume = 1.0f;
    [SerializeField] private float dashVolume = 1.0f;
    [SerializeField] private float pitchVariation = 0.1f;

    private AudioSource audioSource;
    [SerializeField] private bool enAccion = false;

    [SerializeField] private float maxActionDuration = 0.6f;
    private float actionTimer = 0f;

    [SerializeField] private GameObject lightPlayer;
    [SerializeField] private GameObject torchSprite;
    

    
    [SerializeField] private CicloDiaNoche cicloDiaNoche;

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


        foreach (var item in SeedItemNames)
        {
            inventory.Add(item.Value, 10);
        }
        if (!inventory.ContainsKey("Regadera"))
            inventory.Add("Regadera", 1);
        UpdateInventoryDisplay();

        if (plowManager == null)
            plowManager = FindObjectOfType<PlowManager>();
        if (tileCursorController == null)
            tileCursorController = FindObjectOfType<TileCursorController>();
    }

    private void Update()
    {
        if (enAccion)
        {
            actionTimer -= Time.deltaTime;
            if (actionTimer <= 0f)
            {
                EndActionState();
            }
            return;
        }

        if (Input.GetButtonDown("Action"))
            HandleAction();
            

        UpdateLightPlayerVisibilityContinuous();
    }

    private void HandleAction()
    {
        if (enAccion || equipActual == EquipType.None) return;

        bool isSeedEquipped = equipActual == EquipType.Semilla1 || equipActual == EquipType.Semilla2;

        if (isSeedEquipped)
        {
            if (!HasItem(SeedItemNames[equipActual]))
            {
                return;
            }
            ExecuteSeedActionInstant();
            return;
        }

        enAccion = true;
        actionTimer = maxActionDuration;

        Vector2 actionDirection = playerMovement.GetLastDirection();


        animator.SetFloat("MoveX", actionDirection.x);
        animator.SetFloat("MoveY", actionDirection.y);
        animator.SetFloat("LastMoveX", actionDirection.x);
        animator.SetFloat("LastMoveY", actionDirection.y);

        switch (equipActual)
        {
            case EquipType.Espada:
                PlayAttackSound();
                animator.SetInteger("AttackIndex", UnityEngine.Random.Range(1, 4));
                animator.SetBool("Atacando", true);
                ActivateHitbox();
                break;

            case EquipType.Hacha:
                PlayTreeCuttingSound();
                animator.SetBool("Talar", true);
                break;

            case EquipType.Pico:
                animator.SetBool("Minar", true);
                break;

            case EquipType.Arado:
                animator.SetBool("Arar", true);
                break;

            case EquipType.Regadera:
                animator.SetBool("Regar", true);
                break;

            case EquipType.Arco:
                animator.SetBool("Disparar", true);
                break;

            case EquipType.Antorcha:
                break;
        }
    }

    private void ExecuteSeedActionInstant()
    {
        bool isSeedEquipped = equipActual == EquipType.Semilla1 || equipActual == EquipType.Semilla2;
        if (!isSeedEquipped || plowManager == null || tileCursorController == null)
            return;

        Vector3Int cellPos = tileCursorController.GetCurrentCellPosition();

        if (cellPos.x == 999)
        {
            return;
        }

        int seedIndex = (int)equipActual - (int)EquipType.Semilla1;
        bool wasPlanted = plowManager.PlantSeedAt(cellPos, seedIndex);

        if (wasPlanted)
        {
            string seedItemName = SeedItemNames[equipActual];
            RemoveItem(seedItemName, 1);
        }
    }



    
    public void ActivateHitbox()
    {
        if (hitboxScript != null)
        {
            hitboxScript.ActivateHitbox();
        }
        else
        {
        }
    }

    
    public void DisableHitbox()
    {
        if (hitboxScript != null)
        {
            hitboxScript.DeactivateHitbox();
        }
    }

    
    public void ExecutePlowAction()
    {
        if (equipActual != EquipType.Arado || plowManager == null || tileCursorController == null)
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

    
    public void ExecuteWaterAction()
    {
        if (equipActual != EquipType.Regadera || plowManager == null || tileCursorController == null)
        {
            return;
        }

        Vector3Int cellPos = tileCursorController.GetCurrentCellPosition();
        if (cellPos.x == 999) return;

        plowManager.WaterAt(cellPos);
    }

    
    public void EndActionState()
    {
        enAccion = false;

        animator.SetBool("Atacando", false);
        animator.SetBool("Talar", false);
        animator.SetBool("Minar", false);
        animator.SetBool("Arar", false);
        animator.SetBool("Regar", false);
        animator.SetBool("Disparar", false);
        animator.SetInteger("AttackIndex", 0);
    }



    private void PlayAttackSound()
    {
        if (attackSounds != null && attackSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = attackSounds[UnityEngine.Random.Range(0, attackSounds.Length)];
            audioSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(clip, audioVolume * attackVolume);
        }
    }

    private void PlayTreeCuttingSound()
    {
        if (treeCuttingSounds != null && treeCuttingSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = treeCuttingSounds[UnityEngine.Random.Range(0, treeCuttingSounds.Length)];
            audioSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(clip, audioVolume * treeCuttingVolume);
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



    public void SetEquip(EquipType newEquip)
    {
        if (enAccion) return;
        equipActual = newEquip;
        
        UpdateLightPlayerVisibility();
    }

    public int GetBaseDamage()
    {
        return baseDamage;
    }

    public EquipType GetCurrentEquip()
    {
        return equipActual;
    }

    public bool HasItem(string itemName)
    {
        return inventory.ContainsKey(itemName) && inventory[itemName] > 0;
    }

    public int GetItemCount(string itemName)
    {
        return inventory.TryGetValue(itemName, out var count) ? count : 0;
    }

    private void UpdateLightPlayerVisibility()
    {
        bool tieneAntorchaEquipada = equipActual == EquipType.Antorcha;
        bool esDeNoche = EsDeNoche();
        

        bool deberiaEstarEncendida = tieneAntorchaEquipada && esDeNoche;
        

        if (torchSprite != null)
        {
            torchSprite.SetActive(tieneAntorchaEquipada);
        }
        


        

        if (lightPlayer != null)
        {
            if (lightPlayer.activeSelf != deberiaEstarEncendida)
            {
                lightPlayer.SetActive(deberiaEstarEncendida);
                
                if (deberiaEstarEncendida)
                {
                }
                else if (tieneAntorchaEquipada)
                {
                }
                else
                {
                }
            }
        }
        else
        {
        }
    }
    
    private void UpdateLightPlayerVisibilityContinuous()
    {
        bool tieneAntorchaEquipada = equipActual == EquipType.Antorcha;
        

        if (torchSprite != null)
        {
            torchSprite.SetActive(tieneAntorchaEquipada);
        }
        


        

        if (lightPlayer != null && cicloDiaNoche != null)
        {
            bool esDeNoche = cicloDiaNoche.EsDeNoche();
            

            bool deberiaEstarEncendida = tieneAntorchaEquipada && esDeNoche;
            

            if (lightPlayer.activeSelf != deberiaEstarEncendida)
            {
                lightPlayer.SetActive(deberiaEstarEncendida);
                
                if (deberiaEstarEncendida)
                {
                }
                else if (tieneAntorchaEquipada)
                {
                }
            }
        }
    }
    
    private bool EsDeNoche()
    {
        if (cicloDiaNoche == null)
        {

            cicloDiaNoche = FindObjectOfType<CicloDiaNoche>();
            
            if (cicloDiaNoche == null)
            {
                return false;
            }
        }
        

        return cicloDiaNoche.EsDeNoche();
    }
}
