using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public class PlayerActionController : MonoBehaviour
{
    // 9 herramientas: Espada, Hacha, Pico, Arado, Regadera, Arco, Antorcha, Semilla1, Semilla2
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

    // Diccionario para mapear EquipType de Semillas a un nombre de ítem en el inventario
    private readonly Dictionary<EquipType, string> SeedItemNames = new Dictionary<EquipType, string>
    {
        { EquipType.Semilla1, "SemillasDeGirasol" },
        { EquipType.Semilla2, "SemillasDeCebolla" }
    };

    private Animator animator;
    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;

    // Referencias para arado/siembra/riego
    [SerializeField] private PlowManager plowManager;
    [SerializeField] private TileCursorController tileCursorController;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private AudioClip[] treeCuttingSounds;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private float audioVolume = 1.0f;
    [SerializeField] private float pitchVariation = 0.1f;

    private AudioSource audioSource;
    [SerializeField] private bool enAccion = false;

    [Header("Configuración de acción")]
    [SerializeField] private float actionRange = 1.2f;
    [SerializeField] private LayerMask resourceLayer;

    [Tooltip("Duración máxima de una acción antes de desbloquear automáticamente (seguridad).")]
    [SerializeField] private float maxActionDuration = 0.6f;

    // Temporizador simple para finalizar acciones
    private float actionTimer = 0f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Inicializar managers
        plowManager = PlowManager.Instance;

        if (plowManager == null)
        {
            plowManager = FindObjectOfType<PlowManager>();
        }

        tileCursorController = FindObjectOfType<TileCursorController>();

        // Código temporal para pruebas (Inventario)
        foreach (var item in SeedItemNames)
        {
            inventory.Add(item.Value, 10);
        }
        if (!inventory.ContainsKey("Regadera"))
            inventory.Add("Regadera", 1);
        UpdateInventoryDisplay();

        if (plowManager == null)
            Debug.LogError("PlayerActionController: PlowManager.Instance es nulo.");
        if (tileCursorController == null)
            Debug.LogError("PlayerActionController: No se encontró TileCursorController.");
    }

    private void Update()
    {
        // Si estamos en acción, reducimos el temporizador y liberamos al terminar
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
    }

    private void HandleAction()
    {
        if (enAccion || equipActual == EquipType.None) return;

        bool isSeedEquipped = equipActual == EquipType.Semilla1 || equipActual == EquipType.Semilla2;

        // 1. Siembra Instantánea
        if (isSeedEquipped)
        {
            if (!HasItem(SeedItemNames[equipActual]))
            {
                Debug.Log($"No tienes {SeedItemNames[equipActual]} para plantar.");
                return;
            }
            ExecuteSeedActionInstant();
            return;
        }

        // 2. Otras acciones (con animación y bloqueo temporal)
        enAccion = true;
        actionTimer = maxActionDuration;

        Vector2 actionDirection = playerMovement.GetLastDirection();

        // Actualiza Animator y Sprite
        animator.SetFloat("MoveX", actionDirection.x);
        animator.SetFloat("MoveY", actionDirection.y);
        animator.SetFloat("LastMoveX", actionDirection.x);
        animator.SetFloat("LastMoveY", actionDirection.y);

        if (actionDirection.x != 0)
        {
            float baseScale = playerMovement.GetSpriteRenderer().transform.localScale.y;
            spriteRenderer.transform.localScale = new Vector3(
                actionDirection.x < 0 ? -baseScale : baseScale,
                baseScale,
                baseScale
            );
        }

        // Ejecutar animación según herramienta
        switch (equipActual)
        {
            case EquipType.Espada:
                PlayAttackSound();
                animator.SetInteger("AttackIndex", UnityEngine.Random.Range(1, 4));
                animator.SetBool("Atacando", true);
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
                Debug.Log("Disparar Arco");
                break;

            case EquipType.Antorcha:
                animator.SetBool("UsarAntorcha", true);
                Debug.Log("Usar Antorcha");
                break;
        }

        TryHitResource(actionDirection);
    }

    // Función de siembra instantánea
    private void ExecuteSeedActionInstant()
    {
        bool isSeedEquipped = equipActual == EquipType.Semilla1 || equipActual == EquipType.Semilla2;
        if (!isSeedEquipped || plowManager == null || tileCursorController == null)
            return;

        Vector3Int cellPos = tileCursorController.GetCurrentCellPosition();

        if (cellPos.x == 999)
        {
            Debug.LogWarning("Siembra fallida: Cursor no apunta a un tile válido.");
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

    // Llamado por el evento de animación "ExecutePlowAction"
    public void ExecutePlowAction()
    {
        if (equipActual != EquipType.Arado || plowManager == null || tileCursorController == null)
        {
            Debug.LogWarning("Plow/Harvest Action fallida: Manager(s) null o herramienta incorrecta.");
            return;
        }

        Vector3Int cellPos = tileCursorController.GetCurrentCellPosition();

        if (cellPos.x == 999) return;

        bool isReadyToHarvest = plowManager.IsCropReadyToHarvest(cellPos);

        if (isReadyToHarvest)
        {
            plowManager.HarvestAt(cellPos);
        }
        else
        {
            plowManager.PlowAt(cellPos);
        }
    }

    // Llamado por el evento de animación "ExecuteWaterAction"
    public void ExecuteWaterAction()
    {
        if (equipActual != EquipType.Regadera || plowManager == null || tileCursorController == null)
        {
            Debug.LogWarning("WaterAction fallida: Manager(s) null o herramienta incorrecta.");
            return;
        }

        Vector3Int cellPos = tileCursorController.GetCurrentCellPosition();

        if (cellPos.x == 999) return;

        plowManager.WaterAt(cellPos);
    }

    public void CollectResource(string resourceName, int amount)
    {
        if (inventory.ContainsKey(resourceName))
            inventory[resourceName] += amount;
        else
            inventory.Add(resourceName, amount);

        UpdateInventoryDisplay();
        Debug.Log($"¡Recogido! {amount} de {resourceName}.");
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

    private void TryHitResource(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, actionRange, resourceLayer);
        if (hit.collider != null)
        {
            Resource_Collect resource = hit.collider.GetComponent<Resource_Collect>();
            if (resource != null)
            {
                resource.TakeHit(equipActual, baseDamage);
            }
        }
    }

    public void ActivateHitbox()
    {
        Vector2 lastDirection = playerMovement.GetLastDirection();
        DamageHitbox hitboxScript = damageHitbox.GetComponent<DamageHitbox>();

        if (hitboxScript != null)
            hitboxScript.Initialize(equipActual, baseDamage);

        damageHitbox.transform.localPosition = lastDirection * 0.4f;
        float angle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
        damageHitbox.transform.rotation = Quaternion.Euler(0, 0, angle + 90f);
        damageHitbox.SetActive(true);
    }

    public void DisableHitbox()
    {
        damageHitbox.SetActive(false);
        damageHitbox.transform.localPosition = Vector3.zero;
        damageHitbox.transform.localRotation = Quaternion.identity;
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
        animator.SetBool("UsarAntorcha", false);
        animator.SetInteger("AttackIndex", 0);
    }

    private void PlayAttackSound()
    {
        if (attackSounds != null && attackSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = attackSounds[UnityEngine.Random.Range(0, attackSounds.Length)];
            audioSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(clip, audioVolume);
        }
    }

    private void PlayTreeCuttingSound()
    {
        if (treeCuttingSounds != null && treeCuttingSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = treeCuttingSounds[UnityEngine.Random.Range(0, treeCuttingSounds.Length)];
            audioSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(clip, audioVolume);
        }
    }

    public void PlayDashSound()
    {
        if (dashSound != null && audioSource != null)
        {
            audioSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(dashSound, audioVolume);
        }
    }

    // MÉTODO PÚBLICO PARA CAMBIAR EQUIPO DESDE EL HOTBAR
    public void SetEquip(EquipType newEquip)
    {
        if (enAccion) return;

        equipActual = newEquip;
        Debug.Log("Equipo cambiado a: " + equipActual);
    }

    // MÉTODOS PÚBLICOS DE ACCESO
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
}