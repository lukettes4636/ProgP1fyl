using UnityEngine;

/// <summary>
/// Controla la visualización del cultivo (Sprite, Color) y aplica la lógica de sorting por posición Y.
/// Va adjunto al Prefab de Cultivo.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class CropVisualController : MonoBehaviour
{
    // Esta referencia DEBE SER PÚBLICA (o internal) para que PlowManager la use
    [HideInInspector] public CropTile cropDataState;

    private SpriteRenderer spriteRenderer;
    private bool isInitialized = false;

    // --- Configuración de Sorting (Igual que PlayerSorting.cs) ---
    [Header("Sorting Config")]
    [Tooltip("Debe coincidir con el Sorting Factor del Player.")]
    [SerializeField] private int sortingFactor = 100;

    // CLAVE: Offset positivo para competir con el Player. Debe ser el mismo valor que en PlayerSorting.
    // Esto asegura que tanto el jugador como el cultivo dibujen en un rango de Sorting Order alto y positivo (~50000).
    private const int BaseDrawOffset = 50000;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Llamado por PlowManager para inicializar el cultivo con su data/estado.
    /// </summary>
    public void Initialize(CropTile dataStateInstance)
    {
        cropDataState = dataStateInstance;
        isInitialized = true;
        UpdateVisuals(); // Establecer el sprite inicial
    }

    private void Update()
    {
        if (!isInitialized) return;

        UpdateSorting();
    }

    /// <summary>
    /// Actualiza el Sprite y el color basado en el estado (crecimiento y humedad).
    /// </summary>
    public void UpdateVisuals()
    {
        if (cropDataState == null || spriteRenderer == null) return;

        // 1. Sprite
        spriteRenderer.sprite = cropDataState.GetCurrentSprite();

        // 2. Color/Humedad
        // El color se oscurece si no está mojado
        spriteRenderer.color = cropDataState.IsMoist() ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);
    }

    /// <summary>
    /// Aplica la lógica de ordenación por posición Y:
    /// Menor Y -> Mayor Sorting Order (Dibujar encima).
    /// </summary>
    private void UpdateSorting()
    {
        float currentY = transform.position.y;

        // 1. Invertir Y. 
        int invertedSorting = Mathf.RoundToInt(-currentY * sortingFactor);

        // 2. Aplicar el Offset positivo.
        int newSortingOrder = invertedSorting + BaseDrawOffset;

        spriteRenderer.sortingOrder = newSortingOrder;
    }
}