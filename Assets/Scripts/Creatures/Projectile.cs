using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private int damage = 25;
    [SerializeField] private bool destroyOnImpact = true;
    [SerializeField] private LayerMask enemyLayers = -1;
    
    [SerializeField] private float lifeStealPercentage = 0.5f;
    [SerializeField] private GameObject healEffect;
    [SerializeField] private Color healColor = Color.green;
    
    [SerializeField] private Color orbColor = Color.red;
    [SerializeField] private float glowIntensity = 2f;
    
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private GameObject trailEffect;
    
    private Rigidbody2D rb;
    private bool hasImpacted = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        SetupAppearance();
        Destroy(gameObject, lifeTime);
    }
    
    private void Start()
    {
        if (trailEffect != null)
        {
            Instantiate(trailEffect, transform.position, Quaternion.identity, transform);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasImpacted) return;
        
        if (((1 << collision.gameObject.layer) & enemyLayers) != 0)
        {
            ProcessImpact(collision.gameObject);
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasImpacted) return;
        
        if (((1 << collision.gameObject.layer) & enemyLayers) != 0)
        {
            ProcessImpact(collision.gameObject);
        }
        else
        {
            if (destroyOnImpact)
            {
                OnImpactEffect();
                Destroy(gameObject);
            }
        }
    }
    
    private void ProcessImpact(GameObject target)
    {
        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
            
            int healingAmount = Mathf.RoundToInt(damage * lifeStealPercentage);
            if (healingAmount > 0)
            {
                HealPlayer(healingAmount);
                CreateHealEffect();
            }
        }
        
        if (destroyOnImpact)
        {
            OnImpactEffect();
            Destroy(gameObject);
        }
        else
        {
            hasImpacted = true;
            OnImpactEffect();
        }
    }
    
    private void OnImpactEffect()
    {
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }
        
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
    }
    
    public void SetupProjectile(Vector2 shootDirection, int projectileDamage, float projectileSpeed, float projectileLifeTime)
    {
        damage = projectileDamage;
        speed = projectileSpeed;
        lifeTime = projectileLifeTime;
        
        CancelInvoke("DestroyProjectile");
        Invoke("DestroyProjectile", lifeTime);
        
        if (rb != null)
        {
            rb.velocity = shootDirection.normalized * speed;
            
            float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    
    public void SetEnemyLayers(LayerMask layerMask)
    {
        enemyLayers = layerMask;
    }
    
    private void SetupAppearance()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = orbColor;
        }
    }
    
    private void HealPlayer(int amount)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Heal(amount);
            }
        }
    }
    
    private void CreateHealEffect()
    {
        if (healEffect != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Instantiate(healEffect, player.transform.position, Quaternion.identity);
            }
        }
    }
    
    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
