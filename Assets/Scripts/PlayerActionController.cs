using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public class PlayerActionController : MonoBehaviour
{
    public enum EquipType { None, Espada, Hacha, Pico, Arado, Arco }

    [SerializeField] private EquipType equipActual = EquipType.None;
    [SerializeField] private int baseDamage = 1;

    [SerializeField] private GameObject damageHitbox;

    private Dictionary<string, int> inventory = new Dictionary<string, int>();
    [SerializeField] private List<string> inventoryDisplay = new List<string>();

    private Animator animator;
    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;
    private PlayerControls controls;

    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private AudioClip[] treeCuttingSounds;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private float audioVolume = 1.0f;
    [SerializeField] private float pitchVariation = 0.1f;
    
    private AudioSource audioSource;
[SerializeField] private bool enAccion = false;

    private void Awake()
    {
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        if (controls.Player.Action.WasPressedThisFrame())
        {
            HandleAction();
        }

        if (controls.Player.ChangeWeapon.WasPressedThisFrame())
        {
            ChangeEquip(1);
        }
    }

    private void HandleAction()
    {
        if (enAccion || equipActual == EquipType.None) return;

        Vector2 lastDirection = playerMovement.GetLastDirection();
        
        enAccion = true;

        if (lastDirection.x != 0)
        {
            if (spriteRenderer != null)
            {
                float currentScale = spriteRenderer.transform.localScale.y;
                spriteRenderer.transform.localScale = new Vector3(lastDirection.x < 0 ? -currentScale : currentScale, currentScale, currentScale);
            }
        }

        switch (equipActual)
        {case EquipType.Espada:
                PlayAttackSound();
                int attackIndex = UnityEngine.Random.Range(1, 4);
                animator.SetInteger("AttackIndex", attackIndex);
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
                break;
        }
    }

    public void ActivateHitbox()
    {
        Vector2 lastDirection = playerMovement.GetLastDirection();

        DamageHitbox hitboxScript = damageHitbox.GetComponent<DamageHitbox>();
        if (hitboxScript != null)
        {
            hitboxScript.Initialize(equipActual, baseDamage);
        }

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


    public void CollectResource(string resourceName, int amount)
    {
        if (inventory.ContainsKey(resourceName))
        {
            inventory[resourceName] += amount;
        }
        else
        {
            inventory.Add(resourceName, amount);
        }

        UpdateInventoryDisplay();

        Debug.Log($"Has recogido {resourceName}. Tienes {inventory[resourceName]} unidades.");
    }

    private void UpdateInventoryDisplay()
    {
        inventoryDisplay.Clear();
        foreach (var item in inventory)
        {
            inventoryDisplay.Add($"{item.Key}: {item.Value}");
        }
    }

    private void ChangeEquip(int direction)
    {
        if (enAccion) return;

        int currentEquipIndex = (int)equipActual;
        int maxEquipIndex = Enum.GetValues(typeof(EquipType)).Length - 1;

        int newIndex = currentEquipIndex + direction;

        if (newIndex < 0)
        {
            newIndex = maxEquipIndex;
        }
        else if (newIndex > maxEquipIndex)
        {
            newIndex = 0;
        }

        equipActual = (EquipType)newIndex;

        Debug.Log("�Equipo cambiado! Ahora tienes: " + equipActual.ToString());
    }

    
    private void PlayAttackSound()
    {
        if (attackSounds != null && attackSounds.Length > 0 && audioSource != null)
        {
            AudioClip clipToPlay = attackSounds[UnityEngine.Random.Range(0, attackSounds.Length)];
            audioSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(clipToPlay, audioVolume);
        }
    }
    
    private void PlayTreeCuttingSound()
    {
        if (treeCuttingSounds != null && treeCuttingSounds.Length > 0 && audioSource != null)
        {
            AudioClip clipToPlay = treeCuttingSounds[UnityEngine.Random.Range(0, treeCuttingSounds.Length)];
            audioSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(clipToPlay, audioVolume);
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
}