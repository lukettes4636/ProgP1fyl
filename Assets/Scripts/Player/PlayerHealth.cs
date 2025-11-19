using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxHealth = 100;

    [Header("Sonidos")]
    public AudioClip damageSound;
    public AudioClip deathSound;

    [Header("Efectos visuales")]
    public float damageFlashDuration = 0.1f;
    public Color damageFlashColor = Color.red;

    private int currentHealth;
    private bool isDead = false;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Obtener el SpriteRenderer para efectos visuales
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            spriteRenderer = playerMovement.GetSpriteRenderer();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }
    }

    // Método para recibir daño
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"Jugador recibió {amount} de daño. Vida: {currentHealth}/{maxHealth}");

        PlaySound(damageSound);
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("¡El jugador ha muerto!");

        PlaySound(deathSound);

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

        // Aquí puedes agregar:
        // - Mostrar pantalla de Game Over
        // - Reiniciar nivel después de unos segundos
        // Invoke("RestartLevel", 2f);
    }

    // Efecto visual cuando recibe daño
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

    // Métodos útiles
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

        Debug.Log($"Jugador curado +{amount}. Vida: {currentHealth}/{maxHealth}");
    }

    // Método para reiniciar el nivel (opcional)
    /*
    private void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
    */
}