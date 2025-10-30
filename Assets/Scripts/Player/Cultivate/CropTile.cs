using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Crop Tile", menuName = "Tiles/Crop Tile")]
public class CropTile : TileBase
{
    [Header("Configuración del Cultivo")]
    public string cropName = "NuevoCultivo";
    public Sprite[] growthStages;

    [Header("Tiempos de Crecimiento")]
    public float timePerStage = 60f;

    // --- ESTADO INTERNO DEL CULTIVO (RUNTIME) ---
    [Header("Estado Interno del Cultivo (Runtime)")]
    [SerializeField] private int currentStage = 0;
    [SerializeField] private float timeGrown = 0f;
    [SerializeField] private bool isReadyToHarvest = false;

    [Tooltip("Estado de humedad. ¡Ahora la planta controla esto!")]
    [SerializeField] private bool isMoist = false;

    // =======================================================================
    // MÉTODOS PÚBLICOS DE INTERACCIÓN 
    // =======================================================================

    public void Initialize(bool isInitiallyMoist)
    {
        currentStage = 0;
        timeGrown = 0f;
        isReadyToHarvest = false;
        isMoist = isInitiallyMoist;
    }

    public void SetMoisture(bool isWet)
    {
        isMoist = isWet;
    }

    public bool IsMoist() => isMoist;

    public bool AdvanceGrowth(float deltaTime)
    {
        if (isReadyToHarvest || !isMoist)
            return false;

        timeGrown += deltaTime;

        if (timeGrown >= timePerStage)
        {
            timeGrown = 0f;
            currentStage++;

            if (currentStage >= growthStages.Length - 1)
            {
                currentStage = growthStages.Length - 1;
                isReadyToHarvest = true;
            }

            return true;
        }

        return false;
    }

    // =======================================================================
    // MÉTODOS DEL TILEBASE DE UNITY
    // =======================================================================

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        if (growthStages == null || growthStages.Length == 0) return;

        int stage = Mathf.Clamp(currentStage, 0, growthStages.Length - 1);

        tileData.sprite = growthStages[stage];

        // Indicador visual de humedad (Cambia el color del SPRITE de la planta)
        tileData.color = isMoist ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);

        tileData.transform = Matrix4x4.identity;
        tileData.flags = TileFlags.LockTransform;
        tileData.colliderType = Tile.ColliderType.None;
    }

    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        return true;
    }
}