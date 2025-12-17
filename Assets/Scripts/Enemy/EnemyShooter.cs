using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform targetPlayer;
    public Transform firePoint;

    public float projectileSpeed = 8f;
    public float fireRate = 2f;
    public float detectionRange = 8f;

    private float nextFireTime;

    private void Start()
    {
        if (targetPlayer == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                targetPlayer = player.transform;
            }
        }
    }

    private void Update()
    {
        if (targetPlayer == null) return;

        float distance = Vector2.Distance(transform.position, targetPlayer.position);

        if (distance <= detectionRange)
        {
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null) return;

        Vector3 spawnPosition = transform.position;
        if (firePoint != null)
        {
            spawnPosition = firePoint.position;
        }

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        Vector2 direction = (targetPlayer.position - transform.position).normalized;

        EnemyArrow arrowScript = projectile.GetComponent<EnemyArrow>();
        if (arrowScript != null)
        {
            arrowScript.Launch(direction);

            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direction * projectileSpeed;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
