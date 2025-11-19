using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerSorting : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [Tooltip("Factor de multiplicaci�n para el Sorting Order.")]
    [SerializeField] private int sortingFactor = 100;

    private const int BaseDrawOffset = 50000;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("PlayerSorting requiere un SpriteRenderer.");
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