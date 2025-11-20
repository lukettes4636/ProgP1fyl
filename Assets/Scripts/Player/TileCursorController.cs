using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(SpriteRenderer))]
public class TileCursorController : MonoBehaviour
{
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerActionController playerActionController;

    [SerializeField] private float tileSize = 1.0f;

    [SerializeField] private int minTileDistance = 1;

    [SerializeField] private int maxTileDistance = 3;

    [SerializeField] private float maxDistanceThreshold = 0.8f;

    [SerializeField] private Vector3 offset = new Vector3(0, 0.05f, 0);


    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
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
