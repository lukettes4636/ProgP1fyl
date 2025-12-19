using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Data", menuName = "Crops/Crop Data")]
public class CropTile : ScriptableObject
{
    public string cropName = "NewCrop";
    public Sprite[] growthStages;
    public float[] stageScaleMultipliers;

    public float timePerStage = 60f;

    public Sprite harvestItemSprite; 

    public string harvestItemName = "Tomato";

    public int minDropAmount = 1;
    public int maxDropAmount = 3;

    [SerializeField] private int currentStage = 0;
    [SerializeField] private float grownTime = 0f;
    [SerializeField] private bool isReadyToHarvest = false;
    [SerializeField] private bool isMoist = false;

    public void Initialize(bool isInitiallyMoist)
    {
        currentStage = 0;
        grownTime = 0f;
        isReadyToHarvest = false;
        isMoist = isInitiallyMoist;
    }

    public void SetMoisture(bool isWatered)
    {
        isMoist = isWatered;
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

        grownTime += deltaTime;

        if (grownTime >= timePerStage)
        {
            grownTime = 0f;
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
