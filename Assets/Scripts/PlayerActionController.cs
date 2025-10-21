using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public class PlayerActionController : MonoBehaviour
{
    public enum EquipType { None, Espada, Hacha, Pico, Arado, Arco }

    [SerializeField] private EquipType equipActual = EquipType.None;
    [SerializeField] private int baseDamage = 20;
    [SerializeField] private GameObject damageHitbox;

    private Dictionary<string, int> inventory = new Dictionary<string, int>();
    [SerializeField] private List<string> inventoryDisplay = new List<string>();

    private Animator animator;
    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;

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
    }

    private void Update()
    {
        // Si está realizando una acción, no permitir otras ni cambio de herramienta
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

        enAccion = true;

        // Dirección actual (stick derecho o movimiento)
        Vector2 actionDirection = playerMovement.GetLastDirection();

        // Actualiza Animator (dirección de ataque / acción)
        animator.SetFloat("MoveX", actionDirection.x);
        animator.SetFloat("MoveY", actionDirection.y);
        animator.SetFloat("LastMoveX", actionDirection.x);
        animator.SetFloat("LastMoveY", actionDirection.y);

        // Voltear sprite horizontalmente
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

            case EquipType.Arco:
                animator.SetBool("Disparar", true);
                var shootArrows = GetComponent<ShootArrows>();
                if (shootArrows != null)
                    shootArrows.TryShootViaActionController();
                break;
        }

        TryHitResource(actionDirection);

        // Seguridad: liberar control aunque no haya evento en la animación
        if (actionResetCoroutine != null)
            StopCoroutine(actionResetCoroutine);

        actionResetCoroutine = StartCoroutine(AutoResetActionState());
    }

    private IEnumerator AutoResetActionState()
    {
        yield return new WaitForSeconds(maxActionDuration);
        EndActionState(); // Libera automáticamente
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
        animator.SetBool("Disparar", false);
        animator.SetInteger("AttackIndex", 0);
    }

    public void CollectResource(string resourceName, int amount)
    {
        if (inventory.ContainsKey(resourceName))
            inventory[resourceName] += amount;
        else
            inventory.Add(resourceName, amount);

        UpdateInventoryDisplay();
        Debug.Log($"Collected {resourceName}. Total: {inventory[resourceName]}");
    }

    private void UpdateInventoryDisplay()
    {
        inventoryDisplay.Clear();
        foreach (var item in inventory)
            inventoryDisplay.Add($"{item.Key}: {item.Value}");
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

    public int GetBaseDamage() => baseDamage;
    public EquipType GetCurrentEquip() => equipActual;
    public bool HasItem(string itemName) => inventory.ContainsKey(itemName) && inventory[itemName] > 0;
}
