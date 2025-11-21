using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxHealth = 100;

    [Header("UI")]
    public Slider healthSlider;
    public GameObject deathCanvas; // Canvas que aparece al morir

    [Header("Sonidos")]
    public AudioClip damageSound;
    public AudioClip deathSound;

    [Header("Efectos")]
    public float damageFlashDuration = 0.1f;
    public Color damageFlashColor = Color.red;

    [Header("Respawn")]
    public Transform spawnPoint; // Punto donde reaparece

    private int currentHealth;
    private bool isDead = false;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Animator animator;
    private Vector3 initialSpawnPoint; // Guardar posición inicial

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

        // Guardar posición inicial
        initialSpawnPoint = transform.position;

        // Asegurarse que el canvas de muerte está desactivado
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

        Debug.Log($" Jugador recibió {amount} de daño. Vida: {currentHealth}/{maxHealth}");

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

        // Reproducir animación de muerte
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Desactivar enemigos
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].enabled = false;
        }

        // Desactivar movimiento del jugador
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(false);
        }

        // Desactivar acciones del jugador
        PlayerActionController actionController = GetComponent<PlayerActionController>();
        if (actionController != null)
        {
            actionController.enabled = false;
        }

        // Pausar el juego
        Time.timeScale = 0f;

        // Mostrar canvas de muerte
        if (deathCanvas != null)
        {
            deathCanvas.SetActive(true);
        }
    }

    /// <summary>
    /// Revivir al jugador. Llamar desde el botón.
    /// </summary>
    public void Respawn()
    {
        Debug.Log(" Reviviendo jugador...");

        isDead = false;

        // Restaurar vida completa
        currentHealth = maxHealth;
        UpdateHealthSlider();

        // Reactivar el jugador
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

        // Volver a la posición de spawn
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
        }
        else
        {
            transform.position = initialSpawnPoint;
        }

        // Resetear animador (volver a idle)
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        // Restaurar color del sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        // Reactivar enemigos
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].enabled = true;
        }

        // Despausar el juego
        Time.timeScale = 1f;

        // Ocultar canvas de muerte
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