using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 10;
    public Rigidbody2D rb;

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
