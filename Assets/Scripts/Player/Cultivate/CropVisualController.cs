using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CropVisualController : MonoBehaviour
{
    [HideInInspector] public CropTile cropDataState;

    private SpriteRenderer spriteRenderer;
    private bool isInitialized = false;

    [Header("Sorting Config")]
    [Tooltip("Must match Player Sorting Factor.")]
    [SerializeField] private int sortingFactor = 100;

    private const int BaseDrawOffset = 50000;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(CropTile dataStateInstance)
    {
        cropDataState = dataStateInstance;
        isInitialized = true;
        UpdateVisuals(); 
    }

    private void Update()
    {
        if (!isInitialized) return;

        UpdateSorting();
    }

    public void UpdateVisuals()
    {
        if (cropDataState == null || spriteRenderer == null) return;

        spriteRenderer.sprite = cropDataState.GetCurrentSprite();

        spriteRenderer.color = cropDataState.IsMoist() ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);

        float s = cropDataState.GetCurrentScale();
        transform.localScale = new Vector3(s, s, 1f);
    }

    private void UpdateSorting()
    {
        float currentY = transform.position.y;

        int invertedSorting = Mathf.RoundToInt(-currentY * sortingFactor);

        int newSortingOrder = invertedSorting + BaseDrawOffset;

        spriteRenderer.sortingOrder = newSortingOrder;
    }
}
