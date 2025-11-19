using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Data", menuName = "Crops/Crop Data")]
public class CropTile : ScriptableObject
{
    [Header("Configuraci�n del Cultivo")]
    public string cropName = "NuevoCultivo";
    public Sprite[] growthStages;

    [Header("Tiempos de Crecimiento")]
    public float timePerStage = 60f;

    [Header("Configuraci�n de Cosecha (Loot)")]
    [Tooltip("El Sprite del �tem final que se mostrar� al caer en el mundo.")]
    public Sprite harvestItemSprite; 

    [Tooltip("Nombre del recurso final que se a�ade al inventario del jugador.")]
    public string harvestItemName = "Tomate";

    [Tooltip("Cantidad m�nima de �tems que caen al cosechar.")]
    public int minDropAmount = 1;
    [Tooltip("Cantidad m�xima de �tems que caen al cosechar.")]
    public int maxDropAmount = 3;

    [Header("Estado Interno del Cultivo (Runtime)")]
    [SerializeField] private int currentStage = 0;
    [SerializeField] private float timeGrown = 0f;
    [SerializeField] private bool isReadyToHarvest = false;
    [SerializeField] private bool isMoist = false;

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

    public bool IsMoist() { return isMoist; }
    public bool IsReadyToHarvest() { return isReadyToHarvest; }
    public int GetCurrentStage() { return currentStage; }

    public Sprite GetCurrentSprite()
    {
        if (growthStages == null || growthStages.Length == 0) return null;
        int stage = Mathf.Clamp(currentStage, 0, growthStages.Length - 1);
        return growthStages[stage];
    }

    public bool AdvanceGrowth(float deltaTime)
    {
        if (isReadyToHarvest || !isMoist) return false;

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
}