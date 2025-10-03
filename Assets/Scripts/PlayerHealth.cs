using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(HealthSystem))]
public class PlayerHealth : MonoBehaviour
{
    public GameObject bloodVFX;
    [Header("Player Health Settings")]
    public bool showHealthInConsole = true;
    public bool invulnerableOnDamage = false;
    public float invulnerabilityDuration = 1f;
    
    [Header("Regeneration Settings")]
    public bool enableHealthRegeneration = false;
    public float regenerationRate = 1f;
    public float regenerationDelay = 3f;
    public int maxRegenerationHealth = 100;
    public bool regenerateToMaxHealth = false;
    
    [Header("Damage Effects")]
    public bool enableDamageFlash = false;
    public Color damageFlashColor = Color.red;
    public float damageFlashDuration = 0.2f;
    
    [Header("Death Settings")]
    public bool enableRespawn = false;
    public float respawnDelay = 3f;
    public Vector3 respawnPosition = Vector3.zero;
    public bool useCheckpointSystem = false;
    
    [Header("Audio Settings")]
    public AudioClip damageSound;
    public AudioClip healSound;
    public AudioClip deathSound;
    public AudioClip respawnSound;
    [Range(0f, 1f)]
    public float audioVolume = 1f;
    
    private HealthSystem healthSystem;
    private PlayerMovement playerMovement;
    private bool isInvulnerable = false;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private float lastDamageTime;
    private float regenerationTimer;
    private Vector3 currentCheckpoint;
    
    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        currentCheckpoint = useCheckpointSystem ? respawnPosition : transform.position;
        
        healthSystem.OnDamageTaken.AddListener(OnPlayerDamageTaken);
        healthSystem.OnDeath.AddListener(OnPlayerDeath);
        healthSystem.OnHealed.AddListener(OnPlayerHealed);
        healthSystem.OnHealthChanged.AddListener(OnPlayerHealthChanged);
    }

private void Update()
    {
        if (enableHealthRegeneration && !healthSystem.IsDead)
        {
            HandleHealthRegeneration();
        }
    }

    
private void OnPlayerDamageTaken(int damage)
    {
        if (showHealthInConsole)
        {
            Debug.Log($"Player took {damage} damage. Health: {healthSystem.CurrentHealth}/{healthSystem.maxHealth}");
        }

        if (bloodVFX != null)
        {
            Instantiate(bloodVFX, transform.position, Quaternion.identity);
        }
        
        if (invulnerableOnDamage && !isInvulnerable)
        {
            StartInvulnerability();
        }
        
        lastDamageTime = Time.time;
        regenerationTimer = 0f;
        
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

        if (enableRespawn)
        {
            Invoke(nameof(RespawnPlayer), respawnDelay);
        }
        else
        {
            SceneManager.LoadScene(2); // Load Death Scene
        }
    }
    
    private void OnPlayerHealed(int amount)
    {
        if (showHealthInConsole)
        {
            Debug.Log($"Player healed {amount} HP. Health: {healthSystem.CurrentHealth}/{healthSystem.maxHealth}");
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
        
        PlayAudioClip(respawnSound);
    }
    
    private void StartInvulnerability()
    {
        isInvulnerable = true;
        Invoke(nameof(EndInvulnerability), invulnerabilityDuration);
    }
    
    private void EndInvulnerability()
    {
        isInvulnerable = false;
    }
    
    public void TakeDamage(int damage)
    {
        if (isInvulnerable) return;
        
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
        
        if (showHealthInConsole)
        {
            Debug.Log("Player has been revived!");
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


private void HandleHealthRegeneration()
    {
        if (Time.time - lastDamageTime < regenerationDelay)
        {
            return;
        }
        
        regenerationTimer += Time.deltaTime;
        
        if (regenerationTimer >= regenerationRate)
        {
            float regenAmount = regenerateToMaxHealth ? healthSystem.maxHealth - healthSystem.CurrentHealth : 1f;
            if (regenAmount > 0)
            {
                healthSystem.Heal((int)regenAmount);
                regenerationTimer = 0f;
            }
        }
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
    
    private void RespawnPlayer()
    {
        transform.position = currentCheckpoint;
        healthSystem.RestoreFullHealth();
        
        if (showHealthInConsole)
        {
            Debug.Log("Player respawned at checkpoint");
        }
    }
    
    public void SetCheckpoint(Vector3 newCheckpoint)
    {
        if (useCheckpointSystem)
        {
            currentCheckpoint = newCheckpoint;
            
            if (showHealthInConsole)
            {
                Debug.Log($"Checkpoint updated to: {newCheckpoint}");
            }
        }
    }
}