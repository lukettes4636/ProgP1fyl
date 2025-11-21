using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Configuración de la Flecha")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 15;
    [SerializeField] private float lifetime = 3f; // Tiempo antes de destruirse

    private Vector2 direction;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Destruir la flecha después de un tiempo
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Lanza la flecha en una dirección.
    /// </summary>
    public void Launch(Vector2 shootDirection)
    {
        direction = shootDirection.normalized;

        // Mover la flecha
        rb.velocity = direction * speed;

        // Rotar la flecha para que apunte en la dirección correcta
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Debug.Log($"Flecha disparada en dirección: {direction}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignorar al jugador
        if (other.CompareTag("Player"))
            return;

        // Dañar enemigos
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log($"Flecha impactó a {enemy.gameObject.name}");
            Destroy(gameObject); // Destruir la flecha
            return;
        }

        // Destruir al chocar con paredes u obstáculos
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Debug.Log("Flecha chocó con obstáculo");
            Destroy(gameObject);
            return;
        }
    }
}