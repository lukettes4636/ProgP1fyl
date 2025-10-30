using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Gestiona la lógica de arado, riego y siembra, usando 4 capas de Tilemap.
/// </summary>
public class PlowManager : MonoBehaviour
{
    // Layer 1
    [Header("1. Tilemap de Base (Ground)")]
    [Tooltip("La capa más baja (ej: hierba/tierra común).")]
    [SerializeField] private Tilemap groundTilemap;

    // Layer 2
    [Header("2. Tilemap de Arado (Plow)")]
    [Tooltip("Capa de cultivo. Aquí se coloca la tierra arada seca.")]
    [SerializeField] private Tilemap plowTilemap;

    // Layer 3 (Abajo)
    [Header("3. Tilemap de Agua/Mojado")]
    [Tooltip("Capa que muestra el efecto de agua/mojado (DEBAJO del cultivo).")]
    [SerializeField] private Tilemap waterTilemap;

    // Layer 4 (Arriba)
    [Header("4. Tilemap de Cultivo/Planta")]
    [Tooltip("Capa superior donde se colocan los CropTiles (semillas y plantas).")]
    [SerializeField] private Tilemap cropTilemap;

    [Header("Control de Secado")]
    [Tooltip("Tiempo (en segundos) que tarda el suelo/cultivo en secarse por completo. Este es el INTERVALO entre ciclos de secado.")]
    [SerializeField] private float dryingCycleIntervalSeconds = 60.0f;

    [Header("Configuración de Tiles")]
    [SerializeField] private TileBase plowedTile;
    [SerializeField] private TileBase wateredTile;

    [Tooltip("Los 8 CropTiles (Scriptable Tiles) para los cultivos.")]
    [SerializeField] private CropTile[] cropTiles = new CropTile[8];

    // Almacena instancias de los CropTiles en tiempo de ejecución.
    private Dictionary<Vector3Int, CropTile> activeCrops = new Dictionary<Vector3Int, CropTile>();

    public static PlowManager Instance { get; private set; }

    // NUEVO TIMER PARA EL CICLO DE SECADO
    private float dryingCycleTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        // Inicializar el temporizador de secado
        dryingCycleTimer = dryingCycleIntervalSeconds;
    }

    private void Update()
    {
        // ===================================================================
        // BUCLE DE SECADO
        // ===================================================================
        dryingCycleTimer -= Time.deltaTime;

        if (dryingCycleTimer <= 0f)
        {
            TryDryTiles();
            dryingCycleTimer = dryingCycleIntervalSeconds; // Reinicia el timer
        }

        // ===================================================================
        // BUCLE DE CRECIMIENTO
        // ===================================================================

        List<Vector3Int> cellsToRefresh = new List<Vector3Int>();

        foreach (var entry in activeCrops)
        {
            Vector3Int cellPos = entry.Key;
            CropTile crop = entry.Value;

            // La planta solo crece si su estado interno "isMoist" es verdadero.
            bool grew = crop.AdvanceGrowth(Time.deltaTime);

            if (grew)
            {
                cellsToRefresh.Add(cellPos);
            }
        }

        // Refrescar los tiles que cambiaron de etapa o de estado visual (humedad).
        foreach (Vector3Int pos in cellsToRefresh)
        {
            cropTilemap.RefreshTile(pos);
        }
    }

    /// <summary>
    /// Simula el secado de los Tiles (tanto tierra como cultivos).
    /// </summary>
    private void TryDryTiles()
    {
        // 1. Secar tierra arada mojada (Quitar el Tile de Agua)
        List<Vector3Int> wetPlowedTiles = new List<Vector3Int>();
        // Solo necesitamos chequear el área que ya tiene Tile de Agua.
        foreach (Vector3Int pos in waterTilemap.cellBounds.allPositionsWithin)
        {
            // Solo secamos si hay tierra mojada VISUAL y NO hay un cultivo activo en esa posición.
            if (waterTilemap.GetTile(pos) == wateredTile && !activeCrops.ContainsKey(pos))
            {
                wetPlowedTiles.Add(pos);
            }
        }
        foreach (Vector3Int pos in wetPlowedTiles)
        {
            waterTilemap.SetTile(pos, null); // Quita el tile visual de agua.
        }

        // 2. Secar cultivos activos (Cambiar el estado interno del CropTile)
        List<Vector3Int> cropsToRefresh = new List<Vector3Int>();
        foreach (var entry in activeCrops)
        {
            // Solo secamos si está actualmente mojado
            if (entry.Value.IsMoist())
            {
                entry.Value.SetMoisture(false); // La planta ya no está mojada
                cropsToRefresh.Add(entry.Key);
            }
        }
        // Refrescar para que el color/sprite de sequía se muestre
        foreach (Vector3Int pos in cropsToRefresh)
        {
            cropTilemap.RefreshTile(pos);
        }

        Debug.Log("¡Ciclo de secado completado! Próximo secado en " + dryingCycleIntervalSeconds + " segundos.");
    }

    // =======================================================================
    // MÉTODOS DE ACCIÓN 
    // =======================================================================

    public void PlowAt(Vector3Int cellPosition)
    {
        if (plowTilemap == null || plowedTile == null) return;

        if (activeCrops.ContainsKey(cellPosition)) return;

        waterTilemap.SetTile(cellPosition, null);
        cropTilemap.SetTile(cellPosition, null);

        plowTilemap.SetTile(cellPosition, plowedTile);
    }

    /// <summary>
    /// Riega el Tile. Ahora maneja la tierra arada y los cultivos de forma diferente.
    /// </summary>
    public void WaterAt(Vector3Int cellPosition)
    {
        if (plowTilemap == null || waterTilemap == null || wateredTile == null) return;

        if (plowTilemap.GetTile(cellPosition) != plowedTile) return;

        // A. CASO CULTIVO: Si hay un cultivo activo
        if (activeCrops.TryGetValue(cellPosition, out CropTile crop))
        {
            if (crop.IsMoist()) return;

            crop.SetMoisture(true);
            cropTilemap.RefreshTile(cellPosition);
            return;
        }

        // B. CASO TIERRA ARADA VACÍA: Colocamos el Tile de Agua en waterTilemap
        if (waterTilemap.GetTile(cellPosition) == wateredTile) return;

        waterTilemap.SetTile(cellPosition, wateredTile);
    }

    /// <summary>
    /// Planta la semilla: deja el Tile de Agua y coloca la INSTANCIA del CropTile en la capa superior.
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

        // 3. Crear la INSTANCIA del CropTile (CLAVE)
        CropTile originalCropTile = cropTiles[seedIndex];
        CropTile newCropInstance = ScriptableObject.Instantiate(originalCropTile);
        newCropInstance.Initialize(isInitiallyMoist);

        // 4. Colocar la INSTANCIA del CropTile en la capa de CULTIVO
        activeCrops.Add(cellPosition, newCropInstance);
        cropTilemap.SetTile(cellPosition, newCropInstance);

        Debug.Log($"[PlowManager] {newCropInstance.cropName} plantada en {cellPosition}.");

        return true;
    }
}