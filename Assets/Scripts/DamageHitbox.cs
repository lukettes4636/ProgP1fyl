using UnityEngine;

public class DamageHitbox : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float hitboxDistance = 0.6f;
    [SerializeField] private float hitboxLifetime = 0.2f;

    private PlayerActionController actionController;
    private PlayerMovement playerMovement;
    private int currentDamage;
    private PlayerActionController.EquipType currentTool;

    private void Awake()
    {
        actionController = GetComponentInParent<PlayerActionController>();
        playerMovement = GetComponentInParent<PlayerMovement>();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Activa el hitbox en la dirección del ataque.
    /// Llamado desde Animation Event.
    /// </summary>
    public void ActivateHitbox()
    {
        if (actionController == null || playerMovement == null) return;

        // Obtener datos actuales
        currentDamage = actionController.GetBaseDamage();
        currentTool = actionController.GetCurrentEquip();
        Vector2 attackDirection = playerMovement.GetLastDirection();

        // Posicionar hitbox (sin problemas de flip porque ya no hay flip por código)
        transform.localPosition = attackDirection * hitboxDistance;

        Debug.Log($"✓ Hitbox activado - Dirección: {attackDirection}, Posición: {transform.localPosition}");

        // Activar
        gameObject.SetActive(true);

        // Auto-desactivar
        Invoke(nameof(DeactivateHitbox), hitboxLifetime);
    }

    /// <summary>
    /// Desactiva el hitbox.
    /// Llamado desde Animation Event o automáticamente.
    /// </summary>
    public void DeactivateHitbox()
    {
        CancelInvoke(nameof(DeactivateHitbox));
        gameObject.SetActive(false);
        transform.localPosition = Vector3.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Daño a recursos (árboles, rocas, etc)
        Resource_Collect resource = other.GetComponent<Resource_Collect>();
        if (resource != null)
        {
            resource.TakeHit(currentTool, currentDamage);
            return;
        }

        // Daño a enemigos (solo con espada)
        if (currentTool == PlayerActionController.EquipType.Espada)
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                Debug.Log($"⚔️ ¡Golpe! {currentDamage} de daño a {enemy.gameObject.name}");
                enemy.TakeDamage(currentDamage);
            }
        }
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(DeactivateHitbox));
    }

    // Visualización para debug (solo en Scene View)
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && gameObject.activeSelf)
        {
            // Esfera roja donde está el hitbox
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.3f);

            // Línea amarilla desde el jugador
            if (transform.parent != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.parent.position, transform.position);
            }
        }
    }
}