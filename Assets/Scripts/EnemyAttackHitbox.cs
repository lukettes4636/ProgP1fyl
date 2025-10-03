using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    private int damage;
    private EnemyAI enemyAI;

    public void Initialize(int dmg, EnemyAI enemy)
    {
        damage = dmg;
        enemyAI = enemy;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            gameObject.SetActive(false);
        }
    }
}