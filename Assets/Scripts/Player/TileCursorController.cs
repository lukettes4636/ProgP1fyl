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
        {
            Debug.LogError("TileCursorController requiere un SpriteRenderer.");
        }
    }

    private void Update()
    {
        Vector3Int targetCell = GetCurrentCellPosition();

        if (targetCell.x != 999)
        {
            if (!spriteRenderer.enabled) spriteRenderer.enabled = true;

            Vector3 targetWorldPos = targetTilemap.CellToWorld(targetCell);
            targetWorldPos += targetTilemap.cellSize / 2f; 

            transform.position = targetWorldPos + offset;
        }
        else
        {
            if (spriteRenderer.enabled) spriteRenderer.enabled = false;
        }
    }

    public Vector3Int GetCurrentCellPosition()
    {
        if (targetTilemap == null || playerTransform == null || playerActionController == null || playerMovement == null)
        {
            return Vector3Int.one * 999;
        }

        var equip = playerActionController.GetCurrentEquip();

        bool isToolActive = equip == PlayerActionController.EquipType.Hacha ||
                            equip == PlayerActionController.EquipType.Pico ||
                            equip == PlayerActionController.EquipType.Arado ||
                            equip == PlayerActionController.EquipType.Regadera ||
                            (equip >= PlayerActionController.EquipType.Semilla1 && equip <= PlayerActionController.EquipType.Semilla2);

        if (!isToolActive)
        {
            return Vector3Int.one * 999;
        }

        Vector2 aimDir = playerMovement.GetLastDirection();
        float aimMag = aimDir.magnitude;

        if (aimMag <= 0.01f)
        {
            return Vector3Int.one * 999;
        }

        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        if (angle < 0) angle += 360;

        if (angle >= 45 && angle < 135)
        {
            aimDir = Vector2.up;    
        }
        else if (angle >= 135 && angle < 225)
        {
            aimDir = Vector2.left;  
        }
        else if (angle >= 225 && angle < 315)
        {
            aimDir = Vector2.down;  
        }
        else
        {
            aimDir = Vector2.right; 
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

        float effectiveDistance = distanceTiles * tileSize;

        Vector3 targetWorldPos = playerTransform.position + (Vector3)(aimDir * effectiveDistance);

        Vector3Int cellPosition = targetTilemap.WorldToCell(targetWorldPos);

        return cellPosition;
    }
}