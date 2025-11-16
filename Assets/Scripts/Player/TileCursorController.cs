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

    [Header("Configuraci�n de distancia")]
    [Tooltip("Tama�o de un tile en unidades del mundo (normalmente 1).")]
    [SerializeField] private float tileSize = 1.0f;

    [Tooltip("M�nimo n�mero de tiles desde el jugador (ej: 1 para no posicionarse en los pies).")]
    [SerializeField] private int minTileDistance = 1;

    [Tooltip("M�ximo n�mero de tiles que puede alcanzar el cursor.")]
    [SerializeField] private int maxTileDistance = 3;

    [Header("Sensibilidad del Cursor")]
    [Tooltip("Umbral (0.0 a 1.0) para que el cursor salte a la distancia m�xima.")]
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
        // El Update llama a la misma funci�n p�blica para obtener la posici�n y actualizar la visualizaci�n
        Vector3Int targetCell = GetCurrentCellPosition();

        // Muestra u oculta el cursor y lo posiciona.
        if (targetCell.x != 999)
        {
            if (!spriteRenderer.enabled) spriteRenderer.enabled = true;

            // Posiciona el cursor en el centro del tile objetivo
            Vector3 targetWorldPos = targetTilemap.CellToWorld(targetCell);
            targetWorldPos += targetTilemap.cellSize / 2f; // Centrar

            transform.position = targetWorldPos + offset;
        }
        else
        {
            if (spriteRenderer.enabled) spriteRenderer.enabled = false;
        }
    }

    /// <summary>
    ///  M�TODO P�BLICO CORREGIDO. Calcula la posici�n de la celda objetivo y la limita a 4 direcciones.
    /// </summary>
    public Vector3Int GetCurrentCellPosition()
    {
        if (targetTilemap == null || playerTransform == null || playerActionController == null || playerMovement == null)
        {
            return Vector3Int.one * 999;
        }

        var equip = playerActionController.GetCurrentEquip();

        // La compatibilidad con PlayerActionController.EquipType se mantiene:
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

        // -------------------------------------------------------------------
        // L�GICA DE 4 DIRECCIONES CARDINALES (SOLUCI�N)
        // -------------------------------------------------------------------
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        // Normalizar el �ngulo a 0-360
        if (angle < 0) angle += 360;

        // Forzamos la direcci�n a la m�s cercana (Arriba: 45-135, Izquierda: 135-225, Abajo: 225-315, Derecha: 315-45)
        if (angle >= 45 && angle < 135)
        {
            aimDir = Vector2.up;    // Arriba
        }
        else if (angle >= 135 && angle < 225)
        {
            aimDir = Vector2.left;  // Izquierda
        }
        else if (angle >= 225 && angle < 315)
        {
            aimDir = Vector2.down;  // Abajo
        }
        else
        {
            aimDir = Vector2.right; // Derecha 
        }
        // -------------------------------------------------------------------

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

        // Ajustamos la posici�n objetivo usando la direcci�n forzada (aimDir)
        Vector3 targetWorldPos = playerTransform.position + (Vector3)(aimDir * effectiveDistance);

        // Convertir la posici�n del mundo a celda
        Vector3Int cellPosition = targetTilemap.WorldToCell(targetWorldPos);

        return cellPosition;
    }
}
