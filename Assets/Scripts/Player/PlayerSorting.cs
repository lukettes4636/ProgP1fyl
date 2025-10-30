using UnityEngine;

/// <summary>
/// Controla el orden de dibujado (Sorting Order) del jugador
/// para que la perspectiva 2D sea correcta (Top-Down Frontal).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerSorting : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [Tooltip("Factor de multiplicación para el Sorting Order.")]
    [SerializeField] private int sortingFactor = 100;

    // CONSTANTE: Debe ser la misma que en CropVisualController.cs para que compitan en el mismo rango.
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

        // 1. Invertir Y. 
        int invertedSorting = Mathf.RoundToInt(-currentY * sortingFactor);

        // 2. Aplicar el Offset positivo para competir con los Crops.
        int newSortingOrder = invertedSorting + BaseDrawOffset;

        spriteRenderer.sortingOrder = newSortingOrder;
    }
}