using UnityEngine;
using UnityEngine.SceneManagement;

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
    
    private void OnPlayerDeath()
    {
        if (showHealthInConsole)
        {
            Debug.Log("Player has died!");
        }

        PlayAudioClip(deathSound);
        SceneManager.LoadScene(2); 
    }
    
    private void OnPlayerHealed(int amount)
    {
        if (showHealthInConsole)
        {
            Debug.Log($"Player healed {amount} HP. Current Health: {healthSystem.CurrentHealth}/{healthSystem.maxHealth}");
        }
        
        PlayAudioClip(healSound);
    }
    
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
    
    public void TakeDamage(int damage)
    {
        healthSystem.TakeDamage(damage);
    }
    
    public void Heal(int amount)
    {
        healthSystem.Heal(amount);
    }
    
    public void RestoreFullHealth()
    {
        healthSystem.RestoreFullHealth();
    }
    
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
    
    public int GetCurrentHealth()
    {
        return healthSystem.CurrentHealth;
    }
    
    public int GetMaxHealth()
    {
        return healthSystem.maxHealth;
    }
    
    public bool IsAlive()
    {
        return !healthSystem.IsDead;
    }
    
    public float GetHealthPercentage()
    {
        return healthSystem.HealthPercentage;
    }


    private void StartDamageFlash()
    {
        if (spriteRenderer != null)
        {
            StartCoroutine(DamageFlashCoroutine());
        }
    }
    
    private System.Collections.IEnumerator DamageFlashCoroutine()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(damageFlashDuration);
        spriteRenderer.color = originalColor;
    }
    
    private void PlayAudioClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, audioVolume);
        }
    }
}
