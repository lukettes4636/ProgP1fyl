using UnityEngine;
using System.Collections;

[RequireComponent(typeof(HealthSystem))]
public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Health Settings")]
    public bool showHealthInConsole = true;
    public bool dropLootOnDeath = true;
    public GameObject lootPrefab;
    public int lootAmount = 1;
    public float lootDropForce = 5f;
    public float lootSpreadRadius = 1f;
    
    [Header("Death Effects")]
    public float deathDelay = 0f;
    public bool disableColliderOnDeath = true;
    public bool disableMovementOnDeath = true;
    public bool enableDeathAnimation = false;
    public string deathAnimationTrigger = "Death";
    public bool enableDeathParticles = false;
    public GameObject deathParticlesPrefab;
    public bool enableScreenShake = false;
    public float screenShakeIntensity = 0.5f;
    public float screenShakeDuration = 0.3f;
    
    [Header("Audio Settings")]
    public AudioClip damageSound;
    public AudioClip deathSound;
    public AudioClip healSound;
    [Range(0f, 1f)]
    public float audioVolume = 1f;
    
    [Header("Visual Effects")]
    public bool enableDamageFlash = false;
    public Color damageFlashColor = Color.red;
    public float damageFlashDuration = 0.1f;
    public bool healthBarEnabled = false;
    public bool damageFlashEnabled = false;
    public GameObject healthBarPrefab;
    
    [Header("AI Behavior")]
    public bool disableAIOnDeath = true;
    public bool enableRagdollOnDeath = false;
    public bool fadeOutOnDeath = false;
    public float fadeOutDuration = 2f;
    
    [Header("Respawn Settings")]
    public bool canRespawn = false;
    public float respawnDelay = 5f;
    public Vector3 respawnOffset = Vector3.zero;
    
    private HealthSystem healthSystem;
    private Collider2D enemyCollider;
    private MonoBehaviour movementScript;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private AudioSource audioSource;
    private Rigidbody2D rb;
    private Vector3 originalPosition;
    private GameObject healthBarInstance;
    
    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        enemyCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        
        originalPosition = transform.position;
        
        if (healthBarEnabled && healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform);
            UpdateHealthBar();
        }
        
        healthSystem.OnDamageTaken.AddListener(OnEnemyDamageTaken);
        healthSystem.OnDeath.AddListener(OnEnemyDeath);
        healthSystem.OnHealed.AddListener(OnEnemyHealed);
    }
    
    private void OnEnemyDamageTaken(int damage)
    {
        if (showHealthInConsole)
        {
            Debug.Log($"{gameObject.name} took {damage} damage. Health: {healthSystem.CurrentHealth}/{healthSystem.maxHealth}");
        }
        
        PlayAudioClip(damageSound);
        
        if (enableDamageFlash)
        {
            StartDamageFlash();
        }
        
        UpdateHealthBar();
    }
    
    private void OnEnemyDeath()
    {
        if (showHealthInConsole)
        {
            Debug.Log($"{gameObject.name} has died!");
        }
        
        PlayAudioClip(deathSound);
        
        if (enableDeathAnimation && animator != null)
        {
            animator.SetTrigger(deathAnimationTrigger);
        }
        
        if (enableDeathParticles && deathParticlesPrefab != null)
        {
            Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);
        }
        
        if (disableAIOnDeath)
        {
            EnemyAI enemyAI = GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.enabled = false;
            }
        }
        
        if (disableColliderOnDeath && enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
        
        if (disableMovementOnDeath && movementScript != null)
        {
            movementScript.enabled = false;
        }
        
        if (dropLootOnDeath && lootPrefab != null)
        {
            DropLoot();
        }
        
        if (enableRagdollOnDeath && rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
        }
        
        if (fadeOutOnDeath)
        {
            StartCoroutine(FadeOut());
        }
        
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
        
        if (canRespawn)
        {
            Invoke(nameof(RespawnEnemy), respawnDelay);
        }
        else
        {
            GameManager.GetInstance().EnemyDefeated();
            DestroyEnemy();
        }
    }
    
    private void OnEnemyHealed(int amount)
    {
        if (showHealthInConsole)
        {
            Debug.Log($"{gameObject.name} healed {amount} HP. Health: {healthSystem.CurrentHealth}/{healthSystem.maxHealth}");
        }
        
        PlayAudioClip(healSound);
        UpdateHealthBar();
    }
    
    private void DropLoot()
    {
        for (int i = 0; i < lootAmount; i++)
        {
            Vector3 dropPosition = transform.position + (Vector3)(Random.insideUnitCircle * lootSpreadRadius);
            GameObject loot = Instantiate(lootPrefab, dropPosition, Quaternion.identity);
            
            Rigidbody2D lootRb = loot.GetComponent<Rigidbody2D>();
            if (lootRb != null)
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                lootRb.AddForce(randomDirection * lootDropForce, ForceMode2D.Impulse);
            }
        }
    }
    
    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }
    
    public void TakeDamage(int damage)
    {
        healthSystem.TakeDamage(damage);
    }
    
    public void Heal(int amount)
    {
        healthSystem.Heal(amount);
    }
    
    public void SetMovementScript(MonoBehaviour movement)
    {
        movementScript = movement;
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
        if (spriteRenderer != null && damageFlashEnabled)
        {
            StartCoroutine(DamageFlashCoroutine());
        }
    }
    
    private IEnumerator DamageFlashCoroutine()
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
            audioSource.volume = audioVolume;
            audioSource.PlayOneShot(clip);
        }
    }
    
    private void UpdateHealthBar()
    {
        if (healthBarInstance != null)
        {
            float healthPercentage = (float)healthSystem.CurrentHealth / healthSystem.maxHealth;
            healthBarInstance.transform.localScale = new Vector3(healthPercentage, 1f, 1f);
        }
    }
    
    private void EnableRagdoll()
    {
        if (rb != null && enableRagdollOnDeath)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
        }
    }
    
    private IEnumerator FadeOut()
    {
        if (spriteRenderer != null && fadeOutOnDeath)
        {
            Color originalColor = spriteRenderer.color;
            float fadeTime = 1f;
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }
    }
    
    private void RespawnEnemy()
    {
        transform.position = originalPosition + respawnOffset;
        healthSystem.RestoreFullHealth();
        
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            spriteRenderer.color = new Color(color.r, color.g, color.b, 1f);
        }
        
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }
        
        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().enabled = true;
        }
        
        if (movementScript != null)
        {
            movementScript.enabled = true;
        }
        
        if (healthBarEnabled && healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform);
            UpdateHealthBar();
        }
        
        gameObject.SetActive(true);
    }
}