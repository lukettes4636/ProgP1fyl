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
        // Verificar dependencias
        if (playerMovement == null || playerTransform == null || targetTilemap == null || playerActionController == null)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            return;
        }

        // Verificar herramienta equipada
        var equip = playerActionController.GetCurrentEquip();
        bool herramientaActiva = equip == PlayerActionController.EquipType.Hacha ||
                                 equip == PlayerActionController.EquipType.Pico ||
                                 equip == PlayerActionController.EquipType.Arado;

        if (!herramientaActiva)
        {
            spriteRenderer.enabled = false;
            return;
        }

        // Obtener dirección de apuntado o última dirección
        Vector2 aimDir = playerMovement.GetLastDirection();
        float aimMag = aimDir.magnitude;

        if (aimMag <= 0.01f)
        {
            spriteRenderer.enabled = false;
            return;
        }

        aimDir.Normalize();

        // Calcular distancia dinámica según magnitud del stick
        int distanceTiles = Mathf.Clamp(
            Mathf.RoundToInt(minTileDistance + (aimMag * (maxTileDistance - minTileDistance))),
            minTileDistance,
            maxTileDistance
        );

        // Posición objetivo en el mundo
        Vector3 targetWorldPos = playerTransform.position + (Vector3)(aimDir * distanceTiles * tileSize);

        // Convertir a coordenadas del Tilemap
        Vector3Int cellPos = targetTilemap.WorldToCell(targetWorldPos);
        Vector3 cellCenter = targetTilemap.GetCellCenterWorld(cellPos);

        // Posición final
        transform.position = (snapToGrid ? cellCenter : targetWorldPos) + offset;

        // Mostrar cursor solo si tiene herramienta válida y hay dirección
        spriteRenderer.enabled = true;
    }
}
