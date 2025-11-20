using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerSorting : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] private int sortingFactor = 100;

    private const int BaseDrawOffset = 50000;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
        }
    }

    private void Update()
    {
        float currentY = transform.position.y;

        int invertedSorting = Mathf.RoundToInt(-currentY * sortingFactor);

        int newSortingOrder = invertedSorting + BaseDrawOffset;

        spriteRenderer.sortingOrder = newSortingOrder;
    }
}
