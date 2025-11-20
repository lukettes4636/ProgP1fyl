using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Data", menuName = "Crops/Crop Data")]
public class CropTile : ScriptableObject
{
[Header("Crop Settings")]
    public string cropName = "NewCrop";
    public Sprite[] growthStages;
    public float[] stageScaleMultipliers;

[Header("Growth Times")]
    public float timePerStage = 60f;

[Header("Harvest Settings (Loot)")]
[Tooltip("Sprite of the final item shown when it drops.")]
    public Sprite harvestItemSprite; 

[Tooltip("Name of the final resource added to player inventory.")]
    public string harvestItemName = "Tomato";

[Tooltip("Minimum items dropped when harvesting.")]
    public int minDropAmount = 1;
[Tooltip("Maximum items dropped when harvesting.")]
    public int maxDropAmount = 3;

[Header("Internal Crop State (Runtime)")]
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

    public float GetCurrentScale()
    {
        if (stageScaleMultipliers == null || stageScaleMultipliers.Length == 0) return 1f;
        int stage = Mathf.Clamp(currentStage, 0, stageScaleMultipliers.Length - 1);
        return stageScaleMultipliers[stage];
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
