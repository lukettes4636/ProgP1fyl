using UnityEngine;

public class LootDrop : MonoBehaviour
{
    [SerializeField] private string resourceName = "Unknown Item";

    public void SetResourceName(string name)
    {
        resourceName = name;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerActionController playerAction = other.GetComponent<PlayerActionController>();

        if (playerAction != null && other.CompareTag("Player"))
        {
            playerAction.CollectResource(resourceName, 1);

            Destroy(gameObject);
        }
    }
}
