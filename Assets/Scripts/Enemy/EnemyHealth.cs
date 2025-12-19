using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 50;

    [Header("Loot Settings")]
    public GameObject dropPrefab;
    public int dropAmount = 3;
    [SerializeField] private string dropName = "Mineral";

    [Header("Audio Settings")]
    public AudioClip hitSound;
    public AudioClip deathSound;

    [Header("Death Settings")]
    public float destructionDelay = 0.5f;

    private int currentHealth;
    private bool isDead = false;
    private AudioSource audioSource;
    private Animator animator;
    private EnemyShooter enemyShooter;
    private EnemyAI enemyAI;

    private void Start()
    {
        currentHealth = maxHealth;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        animator = GetComponent<Animator>();
        enemyShooter = GetComponent<EnemyShooter>();
        enemyAI = GetComponent<EnemyAI>();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        PlaySound(hitSound);

        if (enemyShooter != null)
        {
            enemyShooter.PlayImpactAnimation();
        }
        else if (enemyAI != null)
        {
            enemyAI.PlayImpactAnimation();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        PlaySound(deathSound);

        if (enemyShooter != null)
        {
            enemyShooter.PlayDeathAnimation();
        }
        else if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }

        if (enemyShooter != null)
        {
            enemyShooter.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        DropLoot();

        if (GameManager.GetInstance() != null)
        {
            GameManager.GetInstance().EnemyDefeated();
        }

        Destroy(gameObject, destructionDelay);
    }

    private void DropLoot()
    {
        if (dropPrefab != null)
        {
            for (int i = 0; i < dropAmount; i++)
            {
                Vector3 randomPos = transform.position + new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f),
                    0f
                );

                GameObject obj = Instantiate(dropPrefab, randomPos, Quaternion.identity);

                LootDrop loot = obj.GetComponent<LootDrop>();
                if (loot != null)
                {
                    loot.SetResourceName(dropName);
                }

                CollectableItem col = obj.GetComponent<CollectableItem>();
                if (col != null)
                {
                    col.Initialize(dropName, 1, null);
                }
            }
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

    public bool IsAlive()
    {
        return !isDead;
    }
}
