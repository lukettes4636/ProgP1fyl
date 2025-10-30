using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Gestiona la lógica de arado, riego y siembra, usando 4 capas de Tilemap.
/// Los cultivos ahora son GameObjects individuales para mejor sorting.
/// </summary>
public class PlowManager : MonoBehaviour
{
    public static PlowManager Instance { get; private set; }

    // Layer 1
    [Header("1. Tilemap de Base (Ground)")]
    [Tooltip("La capa más baja (ej: hierba/tierra común).")]
    [SerializeField] private Tilemap groundTilemap;

    // Layer 2
    [Header("2. Tilemap de Arado (Plow)")]
    [Tooltip("Capa de cultivo. Aquí se coloca la tierra arada seca.")]
    [SerializeField] private Tilemap plowTilemap;
    [SerializeField] private TileBase plowedTile; // El Tile de tierra arada seca

    // Layer 3
    [Header("3. Tilemap de Agua/Mojado")]
    [Tooltip("Capa que muestra el efecto de agua/mojado.")]
    [SerializeField] private Tilemap waterTilemap;
    [SerializeField] private TileBase wateredTile; // El Tile de agua/mojado
    [SerializeField] private TileBase groundTile; // El Tile de tierra original (para secado)

    // Layer 4 (MODIFICADO)
    [Header("4. Tilemap de Coordenadas de Cultivo")]
    [Tooltip("Capa superior usada SOLO para obtener coordenadas y posición del mundo.")]
    [SerializeField] private Tilemap cropTilemap;

    // NUEVA REFERENCIA
    [Header("Prefabs y Data")]
    [Tooltip("El Prefab del cultivo que será instanciado (debe tener CropVisualController).")]
    [SerializeField] private GameObject cropPrefab;

    // REFERENCIA: Array de ScriptableObject CropTile (Data)
    [Tooltip("Array de CropTile ScriptableObjects que contienen la data de cada semilla.")]
    [SerializeField] private CropTile[] cropTiles;

    // CLAVE: El diccionario ahora guarda el GameObject instanciado, NO el CropTile.
    [Tooltip("Cultivos activos. Mapea la posición de la celda al GAME OBJECT instanciado.")]
    private Dictionary<Vector3Int, GameObject> activeCrops = new Dictionary<Vector3Int, GameObject>();

    [Header("Control de Secado")]
    [Tooltip("Tiempo (en segundos) que tarda el suelo/cultivo en secarse por completo.")]
    [SerializeField] private float dryingCycleIntervalSeconds = 60f;
    private float dryingTimer = 0f;

    // =======================================================================
    // CICLO DE VIDA
    // =======================================================================

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
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        AdvanceAllCropsGrowth(deltaTime);
        DryingCycle(deltaTime);
    }

    // =======================================================================
    // LÓGICA DE TIEMPO
    // =======================================================================

    private void DryingCycle(float deltaTime)
    {
        dryingTimer += deltaTime;
        if (dryingTimer >= dryingCycleIntervalSeconds)
        {
            dryingTimer = 0f;
            UnwaterRandomTile();
        }
    }

    private void UnwaterRandomTile()
    {
        List<Vector3Int> wateredCells = new List<Vector3Int>();

        // Recorre solo las celdas donde hay agua visible (Layer 3)
        // Puedes optimizar esto si mantienes una lista separada de celdas mojadas.
        BoundsInt bounds = waterTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                if (waterTilemap.GetTile(cellPosition) == wateredTile)
                {
                    wateredCells.Add(cellPosition);
                }
            }
        }

        if (wateredCells.Count > 0)
        {
            Vector3Int cellToDry = wateredCells[UnityEngine.Random.Range(0, wateredCells.Count)];

            // 1. Quitar el Tile de agua (Layer 3)
            waterTilemap.SetTile(cellToDry, null);

            // 2. Notificar al cultivo si existe
            if (activeCrops.ContainsKey(cellToDry))
            {
                GameObject cropGO = activeCrops[cellToDry];
                CropVisualController visualController = cropGO.GetComponent<CropVisualController>();
                if (visualController != null && visualController.cropDataState != null)
                {
                    visualController.cropDataState.SetMoisture(false);
                    visualController.UpdateVisuals(); // Actualizar color a seco
                    Debug.Log($"[PlowManager] El cultivo en {cellToDry} se ha secado.");
                }
            }
            else // Si es solo tierra arada mojada, se convierte en tierra arada seca
            {
                // Si la capa de agua estaba sobre la capa de tierra arada
                if (plowTilemap.GetTile(cellToDry) == plowedTile)
                {
                    // No hace falta poner un tile, solo quitar el de agua.
                }
                else // Si la capa de agua estaba sobre el tile base (río, etc.)
                {
                    // Si el tile base es el mismo que el tile original, dejarlo en null (agua desaparece)
                    // Si manejas un tile de tierra base para todo, podrías poner groundTilemap.SetTile(cellToDry, groundTile);
                }
            }
        }
    }

    // =======================================================================
    // ACCIONES DEL JUGADOR
    // =======================================================================

    public void PlowAt(Vector3Int cellPosition)
    {
        // Solo siembra si no hay ya algo arado o si ya es tierra arada.
        if (plowTilemap.GetTile(cellPosition) == null)
        {
            plowTilemap.SetTile(cellPosition, plowedTile);
        }
        else if (plowTilemap.GetTile(cellPosition) == plowedTile)
        {
            // Quitar tierra arada
            plowTilemap.SetTile(cellPosition, null);
            waterTilemap.SetTile(cellPosition, null); // Quitar agua por si acaso

            // Si hay un cultivo, se pierde al des-arar
            if (activeCrops.ContainsKey(cellPosition))
            {
                GameObject lostCrop = activeCrops[cellPosition];
                activeCrops.Remove(cellPosition);
                Destroy(lostCrop);
            }
        }
    }

    public void WaterAt(Vector3Int cellPosition)
    {
        // Solo riega si es tierra arada (Layer 2)
        if (plowTilemap.GetTile(cellPosition) == plowedTile)
        {
            // 1. Pone el Tile de agua (Layer 3)
            waterTilemap.SetTile(cellPosition, wateredTile);

            // 2. Notificar al cultivo si existe
            if (activeCrops.ContainsKey(cellPosition))
            {
                GameObject cropGO = activeCrops[cellPosition];
                CropVisualController visualController = cropGO.GetComponent<CropVisualController>();
                if (visualController != null && visualController.cropDataState != null)
                {
                    visualController.cropDataState.SetMoisture(true);
                    visualController.UpdateVisuals(); // Actualizar color a mojado
                    Debug.Log($"[PlowManager] El cultivo en {cellPosition} ha sido regado.");
                }
            }
        }
    }

    // MODIFICACIÓN MAYOR
    /// <summary>
    /// Planta la semilla: crea el GameObject del cultivo y le pasa el ScriptableObject instanciado.
    /// </summary>
    public bool PlantSeedAt(Vector3Int cellPosition, int seedIndex)
    {
        if (cropTiles == null || seedIndex < 0 || seedIndex >= cropTiles.Length || cropTiles[seedIndex] == null) return false;
        if (plowTilemap.GetTile(cellPosition) != plowedTile) return false;

        bool isInitiallyMoist = waterTilemap.GetTile(cellPosition) == wateredTile;
        if (!isInitiallyMoist) return false;
        if (activeCrops.ContainsKey(cellPosition)) return false;
        if (cropPrefab == null) { Debug.LogError("Crop Prefab no asignado en PlowManager!"); return false; }


        // 1. Crear la INSTANCIA del CropTile (Data/Estado)
        CropTile originalCropTile = cropTiles[seedIndex];
        // Es esencial instanciar el ScriptableObject para que cada cultivo tenga su propio estado.
        CropTile newCropInstance = ScriptableObject.Instantiate(originalCropTile);
        newCropInstance.Initialize(isInitiallyMoist);

        // 2. Instanciar el GameObject Visual en la posición central de la celda
        Vector3 worldPos = cropTilemap.CellToWorld(cellPosition);
        // Ajustamos la posición al centro del tile para un mejor sorting (importante para top-down)
        worldPos += (Vector3)cropTilemap.cellSize / 2f;

        GameObject newCropGO = Instantiate(cropPrefab, worldPos, Quaternion.identity);

        // 3. Pasar el ScriptableObject instanciado al controlador visual del GO
        CropVisualController visualController = newCropGO.GetComponent<CropVisualController>();
        if (visualController != null)
        {
            visualController.Initialize(newCropInstance);
        }
        else
        {
            Debug.LogError("El Crop Prefab no tiene el componente CropVisualController!");
            Destroy(newCropGO);
            return false;
        }

        // 4. Registrar el GameObject en la lista de cultivos activos
        activeCrops.Add(cellPosition, newCropGO);
        Debug.Log($"[PlowManager] Semilla de {newCropInstance.cropName} plantada en {cellPosition}. (GO)");

        return true;
    }

    // MODIFICACIÓN MENOR
    public bool IsCropReadyToHarvest(Vector3Int cellPosition)
    {
        if (activeCrops.ContainsKey(cellPosition))
        {
            GameObject cropGO = activeCrops[cellPosition];
            // Accedemos al estado a través del controlador visual
            CropVisualController visualController = cropGO.GetComponent<CropVisualController>();

            if (visualController != null && visualController.cropDataState != null && visualController.cropDataState.IsReadyToHarvest())
            {
                Debug.Log($"[PlowManager] Cultivo {visualController.cropDataState.cropName} listo para cosechar en {cellPosition}.");
                return true;
            }
        }
        return false;
    }

    // MODIFICACIÓN MAYOR
    /// <summary>
    /// Ejecuta la acción de cosecha: destruye el GameObject.
    /// </summary>
    public void HarvestAt(Vector3Int cellPosition)
    {
        if (!activeCrops.ContainsKey(cellPosition)) return;

        GameObject harvestedCropGO = activeCrops[cellPosition];
        CropVisualController visualController = harvestedCropGO.GetComponent<CropVisualController>();

        string cropName = (visualController != null && visualController.cropDataState != null)
                          ? visualController.cropDataState.cropName
                          : "Cultivo Desconocido";

        // 1. Remover de la lista de cultivos activos
        activeCrops.Remove(cellPosition);

        // 2. Destruir el GameObject instanciado
        Destroy(harvestedCropGO);

        // 3. Opcional: limpiar la tierra (si ya estaba lista para re-usarse)
        // waterTilemap.SetTile(cellPosition, null);
        // plowTilemap.SetTile(cellPosition, plowedTile); // Dejar solo la tierra arada (seca)

        Debug.Log($"[PlowManager] **¡COSECHADO!** {cropName} en la celda {cellPosition}.");
    }

    // =======================================================================
    // CRECIMIENTO
    // =======================================================================

    private void AdvanceAllCropsGrowth(float deltaTime)
    {
        // Iteramos sobre todos los GameObjects activos
        foreach (var entry in activeCrops)
        {
            GameObject cropGO = entry.Value;
            CropVisualController visualController = cropGO.GetComponent<CropVisualController>();

            if (visualController != null && visualController.cropDataState != null)
            {
                // Si la función de crecimiento devuelve true, la planta cambió de stage
                if (visualController.cropDataState.AdvanceGrowth(deltaTime))
                {
                    // Pedimos al controlador visual que actualice el Sprite
                    visualController.UpdateVisuals();
                }
            }
        }
    }
}