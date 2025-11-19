using UnityEngine;

public class PlayerContactDamage : MonoBehaviour
{
    public int contactDamage = 10;
    public float contactCooldown = 0.5f;
    private float lastContactTime;

    void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time < lastContactTime + contactCooldown)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(contactDamage);
                lastContactTime = Time.time;
            }
        }
    }
}