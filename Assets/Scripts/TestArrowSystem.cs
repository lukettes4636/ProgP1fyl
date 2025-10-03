using UnityEngine;

public class TestArrowSystem : MonoBehaviour
{
    void Start()
    {
        // Test if Player has ShootArrows component
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            ShootArrows shootArrows = player.GetComponent<ShootArrows>();
            if (shootArrows != null)
            {
                Debug.Log("✓ Player has ShootArrows component");
            }
            else
            {
                Debug.LogError("✗ Player missing ShootArrows component");
            }
        }
        else
        {
            Debug.LogError("✗ Player GameObject not found");
        }
        
        // Test if ArrowPrefab exists
        GameObject arrowPrefab = GameObject.Find("ArrowPrefab");
        if (arrowPrefab != null)
        {
            Rigidbody2D rb = arrowPrefab.GetComponent<Rigidbody2D>();
            Arrow arrow = arrowPrefab.GetComponent<Arrow>();
            
            if (rb != null && arrow != null)
            {
                Debug.Log("✓ ArrowPrefab has required components (Rigidbody2D and Arrow)");
            }
            else
            {
                Debug.LogError("✗ ArrowPrefab missing required components");
            }
        }
        else
        {
            Debug.LogError("✗ ArrowPrefab GameObject not found");
        }
        
        Debug.Log("Arrow shooting system test completed!");
    }
}
