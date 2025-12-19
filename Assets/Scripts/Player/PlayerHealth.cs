using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;

    public Slider healthBar;
    public GameObject deathCanvas;

    public AudioClip damageSound;
    public AudioClip deathSound;

    public float damageFlashDuration = 0.1f;
    public Color damageFlashColor = Color.red;

    public Transform respawnPoint;

    private int currentHealth;
    private bool isDead = false;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Animator animator;
    private Vector3 initialRespawnPoint;

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

        initialRespawnPoint = transform.position;
        if (deathCanvas != null)
        {
            deathCanvas.SetActive(false);
        }

        UpdateHealthBar();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        PlaySound(damageSound);
        StartCoroutine(DamageFlash());
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        PlaySound(deathSound);

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
            ApplyDeathButtonSFX();
        }
    }

    public void Respawn()
    {
        isDead = false;

        currentHealth = maxHealth;
        UpdateHealthBar();

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

        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
        }
        else
        {
            transform.position = initialRespawnPoint;
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
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
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

    private void ApplyDeathButtonSFX()
    {
        var menu = FindObjectOfType<MenuManager>();
        var clip = menu != null ? menu.clickSound : null;
        var hClip = menu != null ? menu.hoverSound : null;
        if (deathCanvas == null) return;
        var buttons = deathCanvas.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            var btn = buttons[i];
            var effect = btn.GetComponent<ButtonScaleEffect>();
            if (effect == null) effect = btn.gameObject.AddComponent<ButtonScaleEffect>();
            effect.ConfigureSounds(hClip, clip);
            effect.Initialize();
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

        UpdateHealthBar();
    }
}
