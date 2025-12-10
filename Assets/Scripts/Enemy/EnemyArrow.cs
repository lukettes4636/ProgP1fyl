using UnityEngine;

public class EnemyArrow : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifetime = 4f;

    private Vector2 direction;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector2 shootDirection)
    {
        direction = shootDirection.normalized;
        rb.velocity = direction * speed;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
            return;

        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (!other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
            return;
        }
    }
}
