using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Gestiona la l�gica de arado, riego y siembra, usando 4 capas de Tilemap.
/// Integra la l�gica de crecimiento y cosecha con loot f�sico CollectableItem.cs.
/// </summary>
public class PlowManager : MonoBehaviour
{
    // === Singleton (Si lo est�s usando) ===
    public static PlowManager Instance { get; private set; }

    // Layers y Tiles
    [Header("Tilemaps")]
    [Tooltip("La capa m�s baja (ej: hierba/tierra com�n).")]
    [SerializeField] private Tilemap groundTilemap;
    [Tooltip("Capa de cultivo. Aqu� se coloca la tierra arada seca.")]
    [SerializeField] private Tilemap plowTilemap;
    [Tooltip("Capa que muestra el efecto de agua/mojado (DEBAJO del cultivo).")]
    [SerializeField] private Tilemap waterTilemap;
    [Tooltip("Capa superior que usamos para mapear los GameObjects de cultivos.")]
    [SerializeField] private Tilemap cropTilemap;
    [SerializeField] private TileBase plowedTile;
    [SerializeField] private TileBase wateredTile;

    [Header("Configuraci�n de Cultivos")]
    [Tooltip("El Prefab que contiene CropVisualController.cs")]
    [SerializeField] private GameObject cropPrefab;
    [Tooltip("ScriptableObjects con la data de cada tipo de semilla.")]
    [SerializeField] private CropTile[] cropTiles;

    // REFERENCIA AL PREFAB DE LOOT CON SCRIPT CollectableItem.cs
    [Header("Configuraci�n de Loot")]
    [Tooltip("El Prefab con el script CollectableItem.cs (debe tener un SpriteRenderer).")]
    [SerializeField] private GameObject lootPrefab; // �Arrastra el Prefab aqu�!

    [Header("Control de Secado")]
    [Tooltip("Tiempo (en segundos) que tarda el suelo/cultivo en secarse por completo.")]
    [SerializeField] private float dryingCycleIntervalSeconds = 60f;
    private float dryTimer = 0f;

    // === ESTADO L�GICO Y DE GAME OBJECTS ===
    // Almacena las instancias de CropTile (el estado).
    private Dictionary<Vector3Int, CropTile> activeCrops = new Dictionary<Vector3Int, CropTile>();
    // Diccionario para rastrear los GameObjects (la visual) del cultivo.
    private Dictionary<Vector3Int, GameObject> activeCropObjects = new Dictionary<Vector3Int, GameObject>();

    // REFERENCIA AL JUGADOR
    [SerializeField] private PlayerActionController playerActionController;


    private void Awake()
    {
        // Implementaci�n Singleton
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
            Debug.LogError("[PlowManager] No se encontr� el PlayerActionController. Asegura que est� activo en la escena.");
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        // Comentario: 1) Avanzar crecimiento si el cultivo está húmedo; actualizar visual si cambió
        foreach (var kvp in activeCrops)
        {
            Vector3Int cell = kvp.Key;
            CropTile cropInstance = kvp.Value;

            // Comentario: Si la planta avanza de etapa, actualizar su visual
            if (cropInstance.AdvanceGrowth(deltaTime))
            {
                if (activeCropObjects.ContainsKey(cell))
                {
                    var visual = activeCropObjects[cell].GetComponent<CropVisualController>();
                    if (visual != null) visual.UpdateVisuals();
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
        // Comentario: 1) Recorre las celdas y elimina tile de agua donde exista
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

            // Comentario: 2) Quitar humedad a los cultivos y actualizar visual
            if (activeCrops.ContainsKey(pos))
            {
                activeCrops[pos].SetMoisture(false);
                var visual = activeCropObjects[pos].GetComponent<CropVisualController>();
                if (visual != null) visual.UpdateVisuals();
            }
        }
    }

    // =======================================================================
    // M�TODOS DE ACCI�N DEL JUGADOR
    // =======================================================================

    public void PlowAt(Vector3Int cellPosition)
    {
        if (activeCrops.ContainsKey(cellPosition)) return;
        waterTilemap.SetTile(cellPosition, null);
        plowTilemap.SetTile(cellPosition, plowedTile);
    }

    // Comentario: Regar en la celda: requiere tierra arada; marca humedad y actualiza visual
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

    /// <summary>
    /// Planta la semilla: deja el Tile de Agua y coloca el GameObject con la instancia del CropTile.
    /// </summary>
    public bool PlantSeedAt(Vector3Int cellPosition, int seedIndex)
    {
        if (cropTiles == null || seedIndex < 0 || seedIndex >= cropTiles.Length || cropTiles[seedIndex] == null) return false;
        if (plowTilemap.GetTile(cellPosition) != plowedTile) return false;

        // 1. Verificar estado de humedad inicial y que la tierra est� mojada para plantar.
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
    /// FUNCI�N DE COSECHA: Suelta el loot f�sico (CollectableItem.cs) y elimina el cultivo.
    /// </summary>
    public void HarvestAt(Vector3Int cellPosition)
    {
        if (!activeCrops.ContainsKey(cellPosition) || !activeCropObjects.ContainsKey(cellPosition) || lootPrefab == null)
        {
            Debug.LogWarning("[PlowManager] Cosecha fallida: Revisa la celda, el GameObject del cultivo o la asignaci�n de lootPrefab.");
            return;
        }

        // 1. Obtener la data del cultivo
        CropTile cropInstance = activeCrops[cellPosition];
        GameObject cropGO = activeCropObjects[cellPosition];

        if (!cropInstance.IsReadyToHarvest())
        {
            Debug.LogWarning($"[PlowManager] La planta {cropInstance.cropName} a�n no est� lista.");
            return;
        }

        // 2. Determinar la cantidad de loot.
        int dropAmount = UnityEngine.Random.Range(cropInstance.minDropAmount, cropInstance.maxDropAmount + 1);

        // 3. Obtener el Sprite del item directamente de la data del cultivo.
        Sprite itemSprite = cropInstance.harvestItemSprite;

        // 4. SOLTAR EL LOOT F�SICO EN EL MUNDO
        for (int i = 0; i < dropAmount; i++)
        {
            // Instanciar el prefab de loot en la posici�n de la planta
            GameObject lootObject = Instantiate(lootPrefab, cropGO.transform.position, Quaternion.identity);

            // OBTENER EL SCRIPT CollectableItem
            CollectableItem collectable = lootObject.GetComponent<CollectableItem>();
            if (collectable != null)
            {
                // PASAMOS LA INFORMACI�N: Nombre, Cantidad (1 por item) y SPRITE
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
