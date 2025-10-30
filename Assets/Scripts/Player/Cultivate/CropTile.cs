using UnityEngine;
// ELIMINAR: using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Crop Tile Data", menuName = "Crops/Crop Data")]
public class CropTile : ScriptableObject
{
    [Header("Configuración del Cultivo")]
    public string cropName = "NuevoCultivo";
    public Sprite[] growthStages; // Sprites que usará el GameObject visual

    [Header("Tiempos de Crecimiento")]
    public float timePerStage = 60f;

    // --- ESTADO INTERNO DEL CULTIVO (RUNTIME) ---
    [Header("Estado Interno del Cultivo (Runtime)")]
    // Estas variables son gestionadas por la instancia de ScriptableObject creada en PlowManager
    [SerializeField] private int currentStage = 0;
    [SerializeField] private float timeGrown = 0f;
    [SerializeField] private bool isReadyToHarvest = false;

    [Tooltip("Estado de humedad.")]
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

    // Nueva función para que el controlador visual sepa qué Sprite usar
    public Sprite GetCurrentSprite()
    {
        if (growthStages == null || growthStages.Length == 0) return null;
        int stage = Mathf.Clamp(currentStage, 0, growthStages.Length - 1);
        return growthStages[stage];
    }

    public void SetMoisture(bool isWet)
    {
        isMoist = isWet;
    }

    public bool IsMoist() => isMoist;

    public bool IsReadyToHarvest() => isReadyToHarvest;

    public bool AdvanceGrowth(float deltaTime)
    {
        if (isReadyToHarvest || !isMoist)
            return false;

        bool stageChanged = false;
        timeGrown += deltaTime;

        if (timeGrown >= timePerStage)
        {
            timeGrown = 0f;
            currentStage++;
            stageChanged = true;

            if (currentStage >= growthStages.Length - 1)
            {
                // SOLUCIÓN DEL ERROR: Asignación al índice final (Length - 1)
                currentStage = growthStages.Length - 1;
                isReadyToHarvest = true;
            }
        }
        return stageChanged;
    }
}