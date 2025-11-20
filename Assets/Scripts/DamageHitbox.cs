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

    
    public void ActivateHitbox()
    {
        if (actionController == null || playerMovement == null) return;


        currentDamage = actionController.GetBaseDamage();
        currentTool = actionController.GetCurrentEquip();
        Vector2 attackDirection = playerMovement.GetLastDirection();


        transform.localPosition = attackDirection * hitboxDistance;

        Debug.Log($"✓ Hitbox activado - Dirección: {attackDirection}, Posición: {transform.localPosition}");


        gameObject.SetActive(true);


        Invoke(nameof(DeactivateHitbox), hitboxLifetime);
    }

    
    public void DeactivateHitbox()
    {
        CancelInvoke(nameof(DeactivateHitbox));
        gameObject.SetActive(false);
        transform.localPosition = Vector3.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        Resource_Collect resource = other.GetComponent<Resource_Collect>();
        if (resource != null)
        {
            resource.TakeHit(currentTool, currentDamage);
            return;
        }


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


    private void OnDrawGizmos()
    {
        if (Application.isPlaying && gameObject.activeSelf)
        {

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.3f);


            if (transform.parent != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.parent.position, transform.position);
            }
        }
    }
}