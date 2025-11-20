using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public AudioClip damageSound;
    public AudioClip deathSound;
    public float damageFlashDuration = 0.1f;
    public Color damageFlashColor = Color.red;

    private int currentHealth;
    private bool isDead = false;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public float deathFadeDuration = 1.2f;

    void Start()
    {
        currentHealth = maxHealth;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

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

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

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

        PlaySound(deathSound);

        var am = FindObjectOfType<AudioManager>();
        if (am != null) am.FadeOutAmbient(deathFadeDuration);

        var enemies = FindObjectsOfType<EnemyAI>();
        for (int i = 0; i < enemies.Length; i++) enemies[i].enabled = false;

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

        StartCoroutine(FadeOutOnDeath());
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

    private System.Collections.IEnumerator FadeOutOnDeath()
    {
        var canvasGO = new GameObject("DeathFadeCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var overlayGO = new GameObject("Overlay");
        overlayGO.transform.SetParent(canvasGO.transform);
        var img = overlayGO.AddComponent<Image>();
        var rt = overlayGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        img.color = new Color(0f, 0f, 0f, 0f);

        float t = 0f;
        while (t < deathFadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / deathFadeDuration);
            var c = img.color;
            img.color = new Color(c.r, c.g, c.b, a);
            yield return null;
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
    }
}
