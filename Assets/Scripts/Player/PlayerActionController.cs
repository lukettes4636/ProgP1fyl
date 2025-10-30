using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public class PlayerActionController : MonoBehaviour
{
    // Añadimos 8 tipos de semillas diferentes
    public enum EquipType
    {
        None, Espada, Hacha, Pico, Arado, Regadera,
        Semilla1, Semilla2, Semilla3, Semilla4,
        Semilla5, Semilla6, Semilla7, Semilla8,
        Arco
    }

    [SerializeField] private EquipType equipActual = EquipType.None;
    [SerializeField] private int baseDamage = 20;
    [SerializeField] private GameObject damageHitbox;

    private Dictionary<string, int> inventory = new Dictionary<string, int>();
    [SerializeField] private List<string> inventoryDisplay = new List<string>();

    // Diccionario para mapear EquipType de Semillas a un nombre de ítem en el inventario.
    // **NOTA:** AJUSTA LOS NOMBRES DE ÍTEMS DE INVENTARIO AQUÍ si es necesario.
    private readonly Dictionary<EquipType, string> SeedItemNames = new Dictionary<EquipType, string>
    {
        { EquipType.Semilla1, "SemillasDeGirasol" },
        { EquipType.Semilla2, "SemillasDeCebolla" },
        { EquipType.Semilla3, "SemillasDePatata" },
        { EquipType.Semilla4, "SemillasDeFresa" },
        { EquipType.Semilla5, "SemillasDeRemolacha" },
        { EquipType.Semilla6, "SemillasDeZanahoria" },
        { EquipType.Semilla7, "SemillasDeApio" },
        { EquipType.Semilla8, "SemillasDeUva" }
    };


    private Animator animator;
    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;

    private PlowManager plowManager;
    private TileCursorController tileCursorController;

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

    private Coroutine actionResetCoroutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        plowManager = PlowManager.Instance;
        tileCursorController = FindObjectOfType<TileCursorController>();

        // --- CÓDIGO TEMPORAL PARA PRUEBAS (Inicializa el inventario con todas las semillas) ---
        foreach (var item in SeedItemNames)
        {
            inventory.Add(item.Value, 10);
        }
        if (!inventory.ContainsKey("Regadera"))
            inventory.Add("Regadera", 1);
        UpdateInventoryDisplay();
        // ---------------------------------------------------------------------------------------

        if (plowManager == null)
            Debug.LogWarning("PlayerActionController: PlowManager.Instance es nulo. ¿Está el script en la escena?");
        if (tileCursorController == null)
            Debug.LogWarning("PlayerActionController: No se encontró TileCursorController en la escena.");
    }

    private void Update()
    {
        // Si está realizando una acción o es siembra instantánea, bloquear otras entradas
        if (enAccion) return;

        // Acción
        if (Input.GetButtonDown("Action"))
            HandleAction();

        // Cambio de herramienta
        if (Input.GetButtonDown("ChangeWeapon"))
            ChangeEquip(1);
    }

    private void HandleAction()
    {
        if (enAccion || equipActual == EquipType.None) return;

        bool isSeedEquipped = equipActual >= EquipType.Semilla1 && equipActual <= EquipType.Semilla8;

        // 1. Verificación de Semillas
        if (isSeedEquipped && !HasItem(SeedItemNames[equipActual]))
        {
            Debug.Log($"No tienes {SeedItemNames[equipActual]} para plantar.");
            return;
        }

        // 2. Ejecución de Siembra Instantánea (sin animación)
        if (isSeedEquipped)
        {
            ExecuteSeedActionInstant();
            // La siembra instantánea NO establece 'enAccion = true' para no bloquear el movimiento.
            return;
        }

        // 3. Ejecución de Otras Acciones (con animación)
        enAccion = true; // Bloquea el input durante la animación

        Vector2 actionDirection = playerMovement.GetLastDirection();

        // Actualiza Animator (dirección de ataque / acción)
        animator.SetFloat("MoveX", actionDirection.x);
        animator.SetFloat("MoveY", actionDirection.y);
        animator.SetFloat("LastMoveX", actionDirection.x);
        animator.SetFloat("LastMoveY", actionDirection.y);

        if (actionDirection.x != 0)
        {
            float currentScale = spriteRenderer.transform.localScale.y;
            spriteRenderer.transform.localScale = new Vector3(
                actionDirection.x < 0 ? -currentScale : currentScale,
                currentScale,
                currentScale
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
                var shootArrows = GetComponent<ShootArrows>();
                if (shootArrows != null)
                    shootArrows.TryShootViaActionController();
                break;
        }

        TryHitResource(actionDirection);

        if (actionResetCoroutine != null)
            StopCoroutine(actionResetCoroutine);

        actionResetCoroutine = StartCoroutine(AutoResetActionState());
    }

    // ----------------------------------------------------------------------------------
    // FUNCIÓN DE SIEMBRA INSTANTÁNEA (Llamada desde HandleAction)
    // ----------------------------------------------------------------------------------

    /// <summary>
    /// Ejecuta la acción de siembra inmediatamente (sin Animation Event).
    /// </summary>
    private void ExecuteSeedActionInstant()
    {
        // Verificar que la herramienta sea una semilla
        bool isSeedEquipped = equipActual >= EquipType.Semilla1 && equipActual <= EquipType.Semilla8;
        if (!isSeedEquipped || plowManager == null || tileCursorController == null)
            return;

        Vector3Int cellPos = tileCursorController.GetCurrentCellPosition();

        if (cellPos.x == 999)
        {
            Debug.LogWarning("Siembra fallida: No se pudo obtener una posición de celda válida.");
            return;
        }

        // 1. Calcular el índice de la semilla (0 para Semilla1, 7 para Semilla8)
        int seedIndex = (int)equipActual - (int)EquipType.Semilla1;

        // 2. Intentar sembrar en el PlowManager
        bool wasPlanted = plowManager.PlantSeedAt(cellPos, seedIndex);

        // 3. Si la siembra fue exitosa, consumir el item del inventario.
        if (wasPlanted)
        {
            string seedItemName = SeedItemNames[equipActual];
            RemoveItem(seedItemName, 1);
        }
    }

    // ----------------------------------------------------------------------------------
    // FUNCIONES DE ACCIÓN (Llamadas por Animation Event)
    // ----------------------------------------------------------------------------------

    public void ExecutePlowAction()
    {
        if (equipActual != EquipType.Arado || plowManager == null || tileCursorController == null)
            return;

        Vector3Int cellPos = tileCursorController.GetCurrentCellPosition();
        if (cellPos.x == 999) return;
        plowManager.PlowAt(cellPos);
    }

    public void ExecuteWaterAction()
    {
        if (equipActual != EquipType.Regadera || plowManager == null || tileCursorController == null)
            return;

        Vector3Int cellPos = tileCursorController.GetCurrentCellPosition();
        if (cellPos.x == 999) return;
        plowManager.WaterAt(cellPos);
    }

    // --- Resto de funciones ---

    private IEnumerator AutoResetActionState()
    {
        yield return new WaitForSeconds(maxActionDuration);
        EndActionState();
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

    private void ChangeEquip(int direction)
    {
        if (enAccion) return;

        int currentEquipIndex = (int)equipActual;
        int maxEquipIndex = Enum.GetValues(typeof(EquipType)).Length - 1;
        int newIndex = currentEquipIndex + direction;

        if (newIndex < 0) newIndex = maxEquipIndex;
        else if (newIndex > maxEquipIndex) newIndex = 0;

        equipActual = (EquipType)newIndex;
        Debug.Log("Equipment changed to: " + equipActual);
    }

    public void EndActionState()
    {
        enAccion = false;

        animator.SetBool("Atacando", false);
        animator.SetBool("Talar", false);
        animator.SetBool("Minar", false);
        animator.SetBool("Arar", false);
        animator.SetBool("Regar", false);
        // La siembra ya no tiene un bool de animación
        animator.SetBool("Disparar", false);
        animator.SetInteger("AttackIndex", 0);
    }

    // --- Funciones de Inventario ---

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

    // --- Funciones de Soporte (Sonidos, Getters) ---

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

    public int GetBaseDamage() => baseDamage;
    public EquipType GetCurrentEquip() => equipActual;
    public bool HasItem(string itemName) => inventory.ContainsKey(itemName) && inventory[itemName] > 0;
}