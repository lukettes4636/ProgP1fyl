using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(SpriteRenderer))]
public class TileCursorController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerActionController playerActionController;

    [Header("Configuración de distancia")]
    [Tooltip("Tamaño de un tile en unidades del mundo (normalmente 1).")]
    [SerializeField] private float tileSize = 1.0f;

    [Tooltip("Mínimo número de tiles desde el jugador (ej: 1 para no posicionarse en los pies).")]
    [SerializeField] private int minTileDistance = 1;

    [Tooltip("Máximo número de tiles que puede alcanzar el cursor.")]
    [SerializeField] private int maxTileDistance = 3;

    [Header("Sensibilidad del Cursor")]
    [Tooltip("Umbral (0.0 a 1.0) para que el cursor salte a la distancia máxima.")]
    [SerializeField] private float maxDistanceThreshold = 0.8f;

    [Header("Visual")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0.05f, 0);
    [SerializeField] private bool snapToGrid = true;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogWarning("TileCursorController: falta SpriteRenderer en el GameObject.");
    }

    private void Update()
    {
        if (playerMovement == null || playerTransform == null || targetTilemap == null || playerActionController == null)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            return;
        }

        var equip = playerActionController.GetCurrentEquip();

        // Verificar si es una herramienta de acción de Tilemap (Arado, Riego, o cualquiera de las 8 Semillas)
        bool herramientaActiva = equip == PlayerActionController.EquipType.Hacha ||
                                 equip == PlayerActionController.EquipType.Pico ||
                                 equip == PlayerActionController.EquipType.Arado ||
                                 equip == PlayerActionController.EquipType.Regadera ||
                                 (equip >= PlayerActionController.EquipType.Semilla1 && equip <= PlayerActionController.EquipType.Semilla8); // RANGO DE SEMILLAS

        if (!herramientaActiva)
        {
            spriteRenderer.enabled = false;
            return;
        }

        Vector2 aimDir = playerMovement.GetLastDirection();
        float aimMag = aimDir.magnitude;

        if (aimMag <= 0.01f)
        {
            spriteRenderer.enabled = false;
            return;
        }

        aimDir.Normalize();

        // Lógica de sensibilidad discreta
        int distanceTiles;
        if (aimMag >= maxDistanceThreshold)
        {
            distanceTiles = maxTileDistance;
        }
        else
        {
            distanceTiles = minTileDistance;
        }

        Vector3 targetWorldPos = playerTransform.position + (Vector3)(aimDir * distanceTiles * tileSize);
        Vector3Int cellPos = targetTilemap.WorldToCell(targetWorldPos);
        Vector3 cellCenter = targetTilemap.GetCellCenterWorld(cellPos);

        transform.position = (snapToGrid ? cellCenter : targetWorldPos) + offset;
        spriteRenderer.enabled = true;
    }

    /// <summary>
    /// Obtiene la posición de la celda del tilemap donde está el cursor para la acción.
    /// </summary>
    public Vector3Int GetCurrentCellPosition()
    {
        if (playerMovement == null || playerTransform == null || targetTilemap == null || playerActionController == null)
        {
            return Vector3Int.one * 999;
        }

        var equip = playerActionController.GetCurrentEquip();
        bool herramientaActiva = equip == PlayerActionController.EquipType.Hacha ||
                                 equip == PlayerActionController.EquipType.Pico ||
                                 equip == PlayerActionController.EquipType.Arado ||
                                 equip == PlayerActionController.EquipType.Regadera ||
                                 (equip >= PlayerActionController.EquipType.Semilla1 && equip <= PlayerActionController.EquipType.Semilla8); // RANGO DE SEMILLAS

        if (!herramientaActiva)
        {
            return Vector3Int.one * 999;
        }

        Vector2 aimDir = playerMovement.GetLastDirection();
        float aimMag = aimDir.magnitude;

        if (aimMag <= 0.01f)
        {
            return Vector3Int.one * 999;
        }

        aimDir.Normalize();

        int distanceTiles;
        if (aimMag >= maxDistanceThreshold)
        {
            distanceTiles = maxTileDistance;
        }
        else
        {
            distanceTiles = minTileDistance;
        }

        Vector3 targetWorldPos = playerTransform.position + (Vector3)(aimDir * distanceTiles * tileSize);
        Vector3Int cellPos = targetTilemap.WorldToCell(targetWorldPos);

        return cellPos;
    }
}