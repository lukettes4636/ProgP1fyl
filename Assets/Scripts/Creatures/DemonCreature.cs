using UnityEngine;

public class DemonCreature : SummonedCreature
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.8f;
    [SerializeField] private int projectileDamage = 35;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float projectileLifetime = 3f;
    
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private LayerMask enemyLayers = -1;
    [SerializeField] private string[] enemyTags = { "Enemy", "Hostile", "Boss" };
    
    private Transform currentTarget;
    private float lastFireTime;
    private AudioSource audioSource;
    private GameObject activeProjectile;
    
    protected override void Awake()
    {
        base.Awake();
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (firePoint == null)
        {
            Transform existingFirePoint = transform.Find("FirePoint");
            if (existingFirePoint != null)
            {
                firePoint = existingFirePoint;
            }
            else
            {
                GameObject firePointObj = new GameObject("FirePoint");
                firePointObj.transform.SetParent(transform);
                firePointObj.transform.localPosition = Vector3.right * 0.5f;
                firePoint = firePointObj.transform;
            }
        }
    }
    
    protected override void Start()
    {
        base.Start();
        SetCreatureType(CreatureType.Demon);
    }
    
    protected override void Update()
    {
        base.Update();
        
        FindTarget();
        
        if (activeProjectile == null && currentTarget != null && Time.time - lastFireTime >= 1f / fireRate)
        {
            AttackTarget();
        }
    }
    
    protected override void HandleCombat()
    {
    }
    
    private void FindTarget()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRange, enemyLayers);
        
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;
        
        foreach (Collider2D enemy in enemies)
        {
            if (IsEnemy(enemy.gameObject))
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance && distance <= attackRange)
                {
                    closestDistance = distance;
                    closestEnemy = enemy.transform;
                }
            }
        }
        
        currentTarget = closestEnemy;
    }
    
    private bool IsEnemy(GameObject target)
    {
        if (target.CompareTag("Player") || target == gameObject)
            return false;
            
        foreach (string enemyTag in enemyTags)
        {
            if (target.CompareTag(enemyTag))
                return true;
        }
        
        if (target.GetComponent<EnemyHealth>() != null || target.GetComponent<EnemyAI>() != null)
            return true;
            
        return false;
    }
    
    private void AttackTarget()
    {
        if (currentTarget == null || projectilePrefab == null) return;
        
        Vector2 shootDirection = CalculatePredictiveShootDirection();
        
        activeProjectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        Projectile projScript = activeProjectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.SetupProjectile(shootDirection, projectileDamage, projectileSpeed, projectileLifetime);
            projScript.SetEnemyLayers(enemyLayers);
        }
        
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }
        
        lastFireTime = Time.time;
    }
    
    private Vector2 CalculatePredictiveShootDirection()
    {
        if (currentTarget == null) return transform.right;
        
        Vector2 targetVelocity = Vector2.zero;
        Rigidbody2D targetRb = currentTarget.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            targetVelocity = targetRb.velocity;
        }
        
        Vector2 toTarget = currentTarget.position - transform.position;
        float distance = toTarget.magnitude;
        float timeToTarget = distance / projectileSpeed;
        
        Vector2 predictedPosition = (Vector2)currentTarget.position + targetVelocity * timeToTarget;
        Vector2 shootDirection = (predictedPosition - (Vector2)transform.position).normalized;
        
        return shootDirection;
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        if (currentTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
    
    public bool HasTarget()
    {
        return currentTarget != null;
    }
}