using UnityEngine;
// Comentario: Controla el disparo de flechas desde el jugador
// Comentario: Mantiene validaciones de inventario/equipo actuales

public class ShootArrows : MonoBehaviour
{
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float arrowForce = 20f;
    public AudioClip arrowSFX;
    [Header("Requirements")]
    [SerializeField] private string requiredItemName = "Arco";
    [SerializeField] private bool requireEquippedArco = true;

    private PlayerActionController actionController;

    // Comentario: Obtiene referencia al controlador de acciones para validar equipo/ítems
    private void Awake()
    {
        actionController = GetComponent<PlayerActionController>();
    }

    // Comentario: Detecta input de disparo y verifica requisitos antes de disparar
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (CanShoot())
            {
                Shoot();
            }
        }
    }

    // Comentario: Instancia la flecha y aplica fuerza en la dirección del firePoint
    public void Shoot()
    {
        if (arrowPrefab == null)
        {
            Debug.LogError("Arrow prefab is not assigned!");
            return;
        }
        
        if (firePoint == null)
        {
            Debug.LogError("Fire point is not assigned!");
            return;
        }
        
        AudioSource.PlayClipAtPoint(arrowSFX, transform.position);
        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        
        
        if (rb == null)
        {
            rb = arrow.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0; 
        }
        
        rb.AddForce(firePoint.up * arrowForce, ForceMode2D.Impulse);
    }

    // Comentario: Verifica si el jugador puede disparar según inventario y equipamiento
    private bool CanShoot()
    {
        if (actionController == null)
        {
            Debug.LogWarning("ShootArrows: PlayerActionController no encontrado. El disparo requiere validar que el jugador tenga el objeto 'Arco'.");
            return false;
        }

        
        bool hasArco = actionController.HasItem(requiredItemName);
        if (!hasArco)
        {
            return false;
        }

        
        if (requireEquippedArco && actionController.GetCurrentEquip() != PlayerActionController.EquipType.Arco)
        {
            return false;
        }

        return true;
    }

    
    // Comentario: Método auxiliar para intentar disparar desde eventos del controlador de acciones
    public void TryShootViaActionController()
    {
        if (CanShoot())
        {
            Shoot();
        }
    }
}
