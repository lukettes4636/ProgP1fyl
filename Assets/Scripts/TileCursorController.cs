using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(SpriteRenderer))]
public class TileCursorController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Configuración de distancia")]
    [Tooltip("Tamaño de un tile en unidades del mundo (normalmente 1).")]
    [SerializeField] private float tileSize = 1.0f;

    [Tooltip("Mínimo número de tiles desde el jugador (ej: 1 para no posicionarse en los pies).")]
    [SerializeField] private int minTileDistance = 1;

    [Tooltip("Máximo número de tiles que puede alcanzar el cursor.")]
    [SerializeField] private int maxTileDistance = 3;

    [Header("Visual")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0.05f, 0);
    [SerializeField] private bool snapToGrid = true; // si true se alinea exactamente al tile center
    [SerializeField, Tooltip("Si true, el cursor solo aparece cuando el stick derecho tiene entrada.")]
    private bool onlyShowWhenAiming = false;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogWarning("TileCursorController: falta SpriteRenderer en el GameObject.");
    }

    private void Update()
    {
        if (playerMovement == null || playerTransform == null || targetTilemap == null)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            return;
        }

        // Obtener dirección y magnitud del apuntado (stick derecho)
        Vector2 aimDir = playerMovement.GetLastDirection();
        float aimMag = aimDir.magnitude;

        // Si onlyShowWhenAiming está activado y no hay input de aim, ocultar cursor
        if (onlyShowWhenAiming && aimMag <= 0.01f)
        {
            spriteRenderer.enabled = false;
            return;
        }

        // Si no hay dirección (por seguridad), usar hacia abajo
        if (aimDir == Vector2.zero)
        {
            aimDir = Vector2.down;
            aimMag = 1f; // tratamos como input completo para poder seleccionar distancia si se quisiera
        }

        aimDir.Normalize();

        // ---- USO de minTileDistance y maxTileDistance ----
        // Calculamos cuántos tiles adelante posicionar el cursor en función de la magnitud del stick.
        // Resultado: minTileDistance cuando aimMag cerca de 0, maxTileDistance cuando aimMag cerca de 1.
        int distanceTiles = Mathf.Clamp(
            Mathf.RoundToInt(minTileDistance + (aimMag * (maxTileDistance - minTileDistance))),
            minTileDistance,
            maxTileDistance
        );

        // Posición objetivo en mundo (distancia en unidades = distanceTiles * tileSize)
        Vector3 targetWorldPos = playerTransform.position + (Vector3)(aimDir * distanceTiles * tileSize);

        // Convertir a cell y obtener centro de celda
        Vector3Int cellPos = targetTilemap.WorldToCell(targetWorldPos);
        Vector3 cellCenter = targetTilemap.GetCellCenterWorld(cellPos);

        // Si snapToGrid false, deja la posición exacta calculada (útil si usás tiles no contiguos)
        Vector3 finalPos = snapToGrid ? cellCenter : targetWorldPos;

        transform.position = finalPos + offset;

        // Mostrar cursor
        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }
}
