using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class PlowManager : MonoBehaviour
{
    public static PlowManager Instance { get; private set; }

    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap plowTilemap;
    [SerializeField] private Tilemap waterTilemap;
    [SerializeField] private Tilemap cropTilemap;
    [SerializeField] private TileBase plowedTile;
    [SerializeField] private TileBase wateredTile;

    [SerializeField] private GameObject cropPrefab;
    [SerializeField] private CropTile[] cropTiles;

    [SerializeField] private GameObject lootPrefab; 

    [SerializeField] private float dryingCycleIntervalSeconds = 60f;
    private float dryTimer = 0f;

    private Dictionary<Vector3Int, CropTile> activeCrops = new Dictionary<Vector3Int, CropTile>();
    private Dictionary<Vector3Int, GameObject> activeCropObjects = new Dictionary<Vector3Int, GameObject>();

    [SerializeField] private PlayerActionController playerActionController;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        playerActionController = FindObjectOfType<PlayerActionController>();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        foreach (var kvp in activeCrops)
        {
            Vector3Int cell = kvp.Key;
            CropTile cropInstance = kvp.Value;

            if (cropInstance.AdvanceGrowth(deltaTime))
            {
                if (activeCropObjects.ContainsKey(cell))
                {
                    var visual = activeCropObjects[cell].GetComponent<CropVisualController>();
                    if (visual != null) visual.UpdateVisuals();
                }
            }
        }

        dryTimer += deltaTime;
        if (dryTimer >= dryingCycleIntervalSeconds)
        {
            dryTimer = 0f;
            ProcessDryingCycle();
        }
    }

    private void ProcessDryingCycle()
    {
        List<Vector3Int> wateredCells = new List<Vector3Int>();
        foreach (var pos in waterTilemap.cellBounds.allPositionsWithin)
        {
            if (waterTilemap.GetTile(pos) == wateredTile)
            {
                wateredCells.Add(pos);
            }
        }

        foreach (Vector3Int pos in wateredCells)
        {
            waterTilemap.SetTile(pos, null);

            if (activeCrops.ContainsKey(pos))
            {
                activeCrops[pos].SetMoisture(false);
                var visual = activeCropObjects[pos].GetComponent<CropVisualController>();
                if (visual != null) visual.UpdateVisuals();
            }
        }
    }

    public void PlowAt(Vector3Int cellPosition)
    {
        if (activeCrops.ContainsKey(cellPosition)) return;
        waterTilemap.SetTile(cellPosition, null);
        plowTilemap.SetTile(cellPosition, plowedTile);
    }

    public void WaterAt(Vector3Int cellPosition)
    {
        if (plowTilemap.GetTile(cellPosition) != plowedTile) return;
        waterTilemap.SetTile(cellPosition, wateredTile);

        if (activeCrops.ContainsKey(cellPosition))
        {
            activeCrops[cellPosition].SetMoisture(true);
            var visual = activeCropObjects[cellPosition].GetComponent<CropVisualController>();
            if (visual != null) visual.UpdateVisuals();
        }
    }

    public bool PlantSeedAt(Vector3Int cellPosition, int seedIndex)
    {
        if (cropTiles == null || seedIndex < 0 || seedIndex >= cropTiles.Length || cropTiles[seedIndex] == null) return false;
        if (plowTilemap.GetTile(cellPosition) != plowedTile) return false;

        bool isInitiallyMoist = waterTilemap.GetTile(cellPosition) == wateredTile;
        if (!isInitiallyMoist)
        {
            return false;
        }

        if (activeCrops.ContainsKey(cellPosition)) return false;

        CropTile originalCropTile = cropTiles[seedIndex];
        CropTile newCropInstance = ScriptableObject.Instantiate(originalCropTile);
        newCropInstance.Initialize(isInitiallyMoist);

        GameObject cropGO = Instantiate(cropPrefab);
        CropVisualController visualController = cropGO.GetComponent<CropVisualController>();

        if (visualController != null)
        {
            visualController.Initialize(newCropInstance);

            Vector3 worldPos = cropTilemap.CellToWorld(cellPosition) + cropTilemap.cellSize / 2f;
            cropGO.transform.position = worldPos;
        }

        activeCrops.Add(cellPosition, newCropInstance);
        activeCropObjects.Add(cellPosition, cropGO);

        return true;
    }

    public bool IsCropReadyToHarvest(Vector3Int cellPosition)
    {
        if (!activeCrops.ContainsKey(cellPosition)) return false;
        return activeCrops[cellPosition].IsReadyToHarvest();
    }


    public void HarvestAt(Vector3Int cellPosition)
    {
        if (!activeCrops.ContainsKey(cellPosition) || !activeCropObjects.ContainsKey(cellPosition) || lootPrefab == null)
        {
            return;
        }

        CropTile cropInstance = activeCrops[cellPosition];
        GameObject cropGO = activeCropObjects[cellPosition];

        if (!cropInstance.IsReadyToHarvest())
        {
            return;
        }

        int dropAmount = UnityEngine.Random.Range(cropInstance.minDropAmount, cropInstance.maxDropAmount + 1);

        Sprite itemSprite = cropInstance.harvestItemSprite;

        for (int i = 0; i < dropAmount; i++)
        {
            GameObject lootObject = Instantiate(lootPrefab, cropGO.transform.position, Quaternion.identity);

            CollectableItem collectable = lootObject.GetComponent<CollectableItem>();
            if (collectable != null)
            {
                collectable.Initialize(cropInstance.harvestItemName, 1, itemSprite);
            }
        }


        Destroy(cropGO);

        ScriptableObject.Destroy(cropInstance);

        activeCrops.Remove(cellPosition);
        activeCropObjects.Remove(cellPosition);

        waterTilemap.SetTile(cellPosition, null);
    }
}
