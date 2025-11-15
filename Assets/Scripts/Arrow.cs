using UnityEngine;
// Comentario: Controla el comportamiento básico de la flecha (velocidad y daño)
// Comentario: Se destruye al impactar y aplica daño a enemigos

public class Arrow : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 10;
    public Rigidbody2D rb;

    // Comentario: Asegura que exista Rigidbody2D y aplica velocidad inicial
    void Start()
    {
        
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            
            
            rb.mass = 0.2f;
            rb.drag = 0.1f;
            rb.angularDrag = 0.01f;
            rb.gravityScale = 0;
        }
        
        rb.velocity = transform.up * speed;
    }

    // Comentario: Al colisionar con un enemigo, aplica daño y destruye la flecha
    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        EnemyHealth enemy = hitInfo.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
