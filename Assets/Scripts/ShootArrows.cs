using UnityEngine;

public class ShootArrows : MonoBehaviour
{
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float arrowForce = 20f;
    public AudioClip arrowSFX;

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (arrowPrefab == null)
        {
            Debug.LogError("Arrow prefab is not assigned!");
            return;
        }
        
        if (firePoint == null)
        {
            Debug.LogError("Fire point is not assigned!");
            return;
        }
        
        AudioSource.PlayClipAtPoint(arrowSFX, transform.position);
        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        
        // If the arrow doesn't have a Rigidbody2D, add one
        if (rb == null)
        {
            rb = arrow.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0; // Arrows shouldn't fall due to gravity
        }
        
        rb.AddForce(firePoint.up * arrowForce, ForceMode2D.Impulse);
    }
}