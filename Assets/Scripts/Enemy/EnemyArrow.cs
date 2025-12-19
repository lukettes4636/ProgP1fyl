using UnityEngine;

public class EnemyArrow : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 10;
    public float lifetime = 4f;

    private Rigidbody2D rb2d;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector2 direction)
    {
        if (rb2d == null) rb2d = GetComponent<Rigidbody2D>();
        
        rb2d.velocity = direction.normalized * speed;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject target)
    {
        if (target.CompareTag("Enemy")) return;

        PlayerHealth player = target.GetComponent<PlayerHealth>();
        
        if (player == null)
        {
            player = target.GetComponentInParent<PlayerHealth>();
        }

        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (target.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
