using UnityEngine;

[RequireComponent(typeof(Collider2D))]

public class BossAttackHitbox : MonoBehaviour
{
    public int damage = 10;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
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
