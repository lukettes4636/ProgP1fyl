using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;

    public Slider healthSlider;
    public GameObject deathCanvas;

    public AudioClip damageSound;
    public AudioClip deathSound;

    public float damageFlashDuration = 0.1f;
    public Color damageFlashColor = Color.red;

    public Transform spawnPoint;

    private int currentHealth;
    private bool isDead = false;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Animator animator;
    private Vector3 initialSpawnPoint;

    void Start()
    {
        currentHealth = maxHealth;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        animator = GetComponent<Animator>();

        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            spriteRenderer = playerMovement.GetSpriteRenderer();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        initialSpawnPoint = transform.position;
        if (deathCanvas != null)
        {
            deathCanvas.SetActive(false);
        }

        UpdateHealthSlider();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($" Jugador recibi� {amount} de da�o. Vida: {currentHealth}/{maxHealth}");

        PlaySound(damageSound);
        StartCoroutine(DamageFlash());
        UpdateHealthSlider();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("El jugador ha muerto!");

        PlaySound(deathSound);

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].enabled = false;
        }

        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(false);
        }

        PlayerActionController actionController = GetComponent<PlayerActionController>();
        if (actionController != null)
        {
            actionController.enabled = false;
        }

        Time.timeScale = 0f;
        if (deathCanvas != null)
        {
            deathCanvas.SetActive(true);
        }
    }

    public void Respawn()
    {
        Debug.Log(" Reviviendo jugador...");

        isDead = false;

        currentHealth = maxHealth;
        UpdateHealthSlider();

        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(true);
        }

        PlayerActionController actionController = GetComponent<PlayerActionController>();
        if (actionController != null)
        {
            actionController.enabled = true;
        }

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
        }
        else
        {
            transform.position = initialSpawnPoint;
        }

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].enabled = true;
        }

        Time.timeScale = 1f;
        if (deathCanvas != null)
        {
            deathCanvas.SetActive(false);
        }

        Debug.Log(" Jugador revivido!");
    }

    private void UpdateHealthSlider()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    private System.Collections.IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(damageFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsAlive()
    {
        return !isDead;
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        Debug.Log($" Jugador curado +{amount}. Vida: {currentHealth}/{maxHealth}");

        UpdateHealthSlider();
    }
}