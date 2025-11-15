using UnityEngine;
using UnityEngine.SceneManagement;
// Comentario: Gestiona la salud del jugador usando HealthSystem y maneja efectos/escenas
// Comentario: Mantiene los mismos logs y comportamientos actuales

[RequireComponent(typeof(HealthSystem))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health Settings")]
    public bool showHealthInConsole = true;
    
    [Header("Damage Effects")]
    public bool enableDamageFlash = false;
    public Color damageFlashColor = Color.red;
    public float damageFlashDuration = 0.2f;
    
    [Header("Audio Settings")]
    public AudioClip damageSound;
    public AudioClip healSound;
    public AudioClip deathSound;
    [Range(0f, 1f)]
    public float audioVolume = 1f;
    
    private HealthSystem healthSystem;
    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    // Comentario: Variables internas para gestionar el flash de daño sin corrutinas
    private bool isDamageFlashing = false;
    private float damageFlashTimer = 0f;
    private Color originalSpriteColor;
    
    // Comentario: Conecta callbacks de HealthSystem y obtiene referencias necesarias
    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        healthSystem.OnDamageTaken.AddListener(OnPlayerDamageTaken);
        healthSystem.OnDeath.AddListener(OnPlayerDeath);
        healthSystem.OnHealed.AddListener(OnPlayerHealed);
        healthSystem.OnHealthChanged.AddListener(OnPlayerHealthChanged);
    }

    
    // Comentario: Callback cuando el jugador recibe daño
    private void OnPlayerDamageTaken(int damage)
    {
        if (showHealthInConsole)
        {
            Debug.Log($"Player took {damage} damage. Current Health: {healthSystem.CurrentHealth}/{healthSystem.maxHealth}");
        }
        
        PlayAudioClip(damageSound);
        
        if (enableDamageFlash)
        {
            StartDamageFlash();
        }
    }
    
    // Comentario: Callback cuando el jugador muere (cambia la escena)
    private void OnPlayerDeath()
    {
        if (showHealthInConsole)
        {
            Debug.Log("Player has died!");
        }

        PlayAudioClip(deathSound);
        SceneManager.LoadScene(2); 
    }
    
    // Comentario: Callback cuando el jugador se cura
    private void OnPlayerHealed(int amount)
    {
        if (showHealthInConsole)
        {
            Debug.Log($"Player healed {amount} HP. Current Health: {healthSystem.CurrentHealth}/{healthSystem.maxHealth}");
        }
        
        PlayAudioClip(healSound);
    }
    
    // Comentario: Actualiza estados habilitados cuando cambia la salud
    private void OnPlayerHealthChanged(int newHealth)
    {
        if (newHealth <= 0)
        {
            return;
        }
        
        if (!playerMovement.enabled && !healthSystem.IsDead)
        {
            playerMovement.enabled = true;
            
            PlayerActionController actionController = GetComponent<PlayerActionController>();
            if (actionController != null)
            {
                actionController.enabled = true;
            }
        }
    }
    
    // Comentario: Método público para aplicar daño al jugador
    public void TakeDamage(int damage)
    {
        healthSystem.TakeDamage(damage);
    }
    
    // Comentario: Método público para curar al jugador
    public void Heal(int amount)
    {
        healthSystem.Heal(amount);
    }
    
    // Comentario: Restaura la salud del jugador al máximo
    public void RestoreFullHealth()
    {
        healthSystem.RestoreFullHealth();
    }
    
    // Comentario: Revive al jugador y re-habilita componentes
    public void Revive()
    {
        healthSystem.Revive();
        
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        
        PlayerActionController actionController = GetComponent<PlayerActionController>();
        if (actionController != null)
        {
            actionController.enabled = true;
        }
        

    }
    
    // Comentario: Devuelve la salud actual
    public int GetCurrentHealth()
    {
        return healthSystem.CurrentHealth;
    }
    
    // Comentario: Devuelve la salud máxima
    public int GetMaxHealth()
    {
        return healthSystem.maxHealth;
    }
    
    // Comentario: Indica si el jugador está vivo
    public bool IsAlive()
    {
        return !healthSystem.IsDead;
    }
    
    // Comentario: Devuelve el porcentaje de salud actual
    public float GetHealthPercentage()
    {
        return healthSystem.HealthPercentage;
    }


    // Comentario: Inicia el efecto de flash de daño usando un temporizador sencillo
    private void StartDamageFlash()
    {
        if (spriteRenderer != null)
        {
            originalSpriteColor = spriteRenderer.color;
            spriteRenderer.color = damageFlashColor;
            isDamageFlashing = true;
            damageFlashTimer = damageFlashDuration;
        }
    }

    // Comentario: Actualiza el temporizador del flash de daño y restaura el color
    private void Update()
    {
        if (isDamageFlashing)
        {
            damageFlashTimer -= Time.deltaTime;
            if (damageFlashTimer <= 0f)
            {
                spriteRenderer.color = originalSpriteColor;
                isDamageFlashing = false;
            }
        }
    }
    
    // Comentario: Reproduce clips de daño/curación/muerte
    private void PlayAudioClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, audioVolume);
        }
    }
}
