using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Gestiona la lógica de arado, riego y siembra, usando 4 capas de Tilemap.
/// Integra la lógica de crecimiento y cosecha con loot físico CollectableItem.cs.
/// </summary>
public class PlowManager : MonoBehaviour
{
    // === Singleton (Si lo estás usando) ===
    public static PlowManager Instance { get; private set; }

    // Layers y Tiles
    [Header("Tilemaps")]
    [Tooltip("La capa más baja (ej: hierba/tierra común).")]
    [SerializeField] private Tilemap groundTilemap;
    [Tooltip("Capa de cultivo. Aquí se coloca la tierra arada seca.")]
    [SerializeField] private Tilemap plowTilemap;
    [Tooltip("Capa que muestra el efecto de agua/mojado (DEBAJO del cultivo).")]
    [SerializeField] private Tilemap waterTilemap;
    [Tooltip("Capa superior que usamos para mapear los GameObjects de cultivos.")]
    [SerializeField] private Tilemap cropTilemap;
    [SerializeField] private TileBase plowedTile;
    [SerializeField] private TileBase wateredTile;

    [Header("Configuración de Cultivos")]
    [Tooltip("El Prefab que contiene CropVisualController.cs")]
    [SerializeField] private GameObject cropPrefab;
    [Tooltip("ScriptableObjects con la data de cada tipo de semilla.")]
    [SerializeField] private CropTile[] cropTiles;

    // REFERENCIA AL PREFAB DE LOOT CON SCRIPT CollectableItem.cs
    [Header("Configuración de Loot")]
    [Tooltip("El Prefab con el script CollectableItem.cs (debe tener un SpriteRenderer).")]
    [SerializeField] private GameObject lootPrefab; // ¡Arrastra el Prefab aquí!

    [Header("Control de Secado")]
    [Tooltip("Tiempo (en segundos) que tarda el suelo/cultivo en secarse por completo.")]
    [SerializeField] private float dryingCycleIntervalSeconds = 60f;
    private float dryTimer = 0f;

    // === ESTADO LÓGICO Y DE GAME OBJECTS ===
    // Almacena las instancias de CropTile (el estado).
    private Dictionary<Vector3Int, CropTile> activeCrops = new Dictionary<Vector3Int, CropTile>();
    // Diccionario para rastrear los GameObjects (la visual) del cultivo.
    private Dictionary<Vector3Int, GameObject> activeCropObjects = new Dictionary<Vector3Int, GameObject>();

    // REFERENCIA AL JUGADOR
    [SerializeField] private PlayerActionController playerActionController;


    private void Awake()
    {
        // Implementación Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        playerActionController = FindObjectOfType<PlayerActionController>();
        if (playerActionController == null)
        {
            Debug.LogError("[PlowManager] No se encontró el PlayerActionController. Asegura que está activo en la escena.");
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        // 1. Manejar Crecimiento/Humedad
        List<Vector3Int> cellsToUpdate = new List<Vector3Int>(activeCrops.Keys);
        foreach (Vector3Int cell in cellsToUpdate)
        {
            CropTile cropInstance = activeCrops[cell];

            // Si crece, actualiza visual (llama a UpdateVisuals en CropVisualController)
            if (cropInstance.AdvanceGrowth(deltaTime))
            {
                if (activeCropObjects.ContainsKey(cell))
                {
                    activeCropObjects[cell].GetComponent<CropVisualController>()?.UpdateVisuals();
                }
            }
        }

        // 2. Manejar Secado
        dryTimer += deltaTime;
        if (dryTimer >= dryingCycleIntervalSeconds)
        {
            dryTimer = 0f;
            ProcessDryingCycle();
        }
    }

    private void ProcessDryingCycle()
    {
        // 1. Quitar agua a la tierra
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

            // 2. Quitar humedad a los cultivos
            if (activeCrops.ContainsKey(pos))
            {
                activeCrops[pos].SetMoisture(false);
                // Actualizar visual de la planta (cambio de color/aspecto)
                activeCropObjects[pos].GetComponent<CropVisualController>()?.UpdateVisuals();
            }
        }
    }

    // =======================================================================
    // MÉTODOS DE ACCIÓN DEL JUGADOR
    // =======================================================================

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
            activeCropObjects[cellPosition].GetComponent<CropVisualController>()?.UpdateVisuals();
        }
    }

    /// <summary>
    /// Planta la semilla: deja el Tile de Agua y coloca el GameObject con la instancia del CropTile.
    /// </summary>
    public bool PlantSeedAt(Vector3Int cellPosition, int seedIndex)
    {
        if (cropTiles == null || seedIndex < 0 || seedIndex >= cropTiles.Length || cropTiles[seedIndex] == null) return false;
        if (plowTilemap.GetTile(cellPosition) != plowedTile) return false;

        // 1. Verificar estado de humedad inicial y que la tierra esté mojada para plantar.
        bool isInitiallyMoist = waterTilemap.GetTile(cellPosition) == wateredTile;
        if (!isInitiallyMoist)
        {
            Debug.Log($"[PlowManager] Siembra fallida. La celda {cellPosition} no es tierra mojada.");
            return false;
        }

        // 2. Verificar que no haya un cultivo activo.
        if (activeCrops.ContainsKey(cellPosition)) return false;

        // 3. Crear la INSTANCIA del CropTile (data)
        CropTile originalCropTile = cropTiles[seedIndex];
        CropTile newCropInstance = ScriptableObject.Instantiate(originalCropTile);
        newCropInstance.Initialize(isInitiallyMoist);

        // 4. Instanciar el GameObject de la planta y asignarle la data
        GameObject cropGO = Instantiate(cropPrefab);
        CropVisualController visualController = cropGO.GetComponent<CropVisualController>();

        if (visualController != null)
        {
            visualController.Initialize(newCropInstance);

            // Posicionar el GO en el centro de la celda
            Vector3 worldPos = cropTilemap.CellToWorld(cellPosition) + cropTilemap.cellSize / 2f;
            cropGO.transform.position = worldPos;
        }

        // 5. Registrar la data y el GameObject
        activeCrops.Add(cellPosition, newCropInstance);
        activeCropObjects.Add(cellPosition, cropGO);

        return true;
    }

    public bool IsCropReadyToHarvest(Vector3Int cellPosition)
    {
        if (!activeCrops.ContainsKey(cellPosition)) return false;
        return activeCrops[cellPosition].IsReadyToHarvest();
    }


    /// <summary>
    /// FUNCIÓN DE COSECHA: Suelta el loot físico (CollectableItem.cs) y elimina el cultivo.
    /// </summary>
    public void HarvestAt(Vector3Int cellPosition)
    {
        if (!activeCrops.ContainsKey(cellPosition) || !activeCropObjects.ContainsKey(cellPosition) || lootPrefab == null)
        {
            Debug.LogWarning("[PlowManager] Cosecha fallida: Revisa la celda, el GameObject del cultivo o la asignación de lootPrefab.");
            return;
        }

        // 1. Obtener la data del cultivo
        CropTile cropInstance = activeCrops[cellPosition];
        GameObject cropGO = activeCropObjects[cellPosition];

        if (!cropInstance.IsReadyToHarvest())
        {
            Debug.LogWarning($"[PlowManager] La planta {cropInstance.cropName} aún no está lista.");
            return;
        }

        // 2. Determinar la cantidad de loot.
        int dropAmount = UnityEngine.Random.Range(cropInstance.minDropAmount, cropInstance.maxDropAmount + 1);

        // 3. Obtener el Sprite del item directamente de la data del cultivo.
        Sprite itemSprite = cropInstance.harvestItemSprite;

        // 4. SOLTAR EL LOOT FÍSICO EN EL MUNDO
        for (int i = 0; i < dropAmount; i++)
        {
            // Instanciar el prefab de loot en la posición de la planta
            GameObject lootObject = Instantiate(lootPrefab, cropGO.transform.position, Quaternion.identity);

            // OBTENER EL SCRIPT CollectableItem
            CollectableItem collectable = lootObject.GetComponent<CollectableItem>();
            if (collectable != null)
            {
                // PASAMOS LA INFORMACIÓN: Nombre, Cantidad (1 por item) y SPRITE
                collectable.Initialize(cropInstance.harvestItemName, 1, itemSprite);
            }
        }

        Debug.Log($"[PlowManager] Cosechado {dropAmount} de {cropInstance.harvestItemName}.");

        // 5. Limpiar el suelo y los registros.

        // 5a. Destruir el GameObject visual del cultivo.
        Destroy(cropGO);

        // 5b. Destruir la instancia del ScriptableObject (memoria).
        ScriptableObject.Destroy(cropInstance);

        // 5c. Eliminar los registros.
        activeCrops.Remove(cellPosition);
        activeCropObjects.Remove(cellPosition);

        // 6. Revertir el suelo (quitar la humedad)
        waterTilemap.SetTile(cellPosition, null);
    }
}