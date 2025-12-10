using UnityEngine;

[RequireComponent(typeof(Collider2D))]

public class DamageHitbox : MonoBehaviour
{
    [SerializeField] private float hitboxDistance = 0.6f;
    [SerializeField] private float hitboxLifetime = 0.2f;
    [SerializeField] private float hitRadius = 0.3f;

    private PlayerActionController actionController;
    private PlayerMovement playerMovement;
    private int currentDamage;
    private PlayerActionController.EquipType currentTool;
    private bool appliedDamage;

    private void Awake()
    {
        actionController = GetComponentInParent<PlayerActionController>();
        playerMovement = GetComponentInParent<PlayerMovement>();
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
        gameObject.SetActive(false);
    }

    public void ActivateHitbox()
    {
        if (actionController == null || playerMovement == null) return;

        currentDamage = actionController.GetBaseDamage();
        currentTool = actionController.GetCurrentEquip();
        Vector2 attackDirection = playerMovement.GetLastDirection();

        transform.localPosition = attackDirection * hitboxDistance;

        gameObject.SetActive(true);
        appliedDamage = false;

        TryImmediateHit();

        Invoke(nameof(DeactivateHitbox), hitboxLifetime);
    }

    public void DeactivateHitbox()
    {
        CancelInvoke(nameof(DeactivateHitbox));
        gameObject.SetActive(false);
        transform.localPosition = Vector3.zero;
        appliedDamage = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (appliedDamage) return;

        Resource_Collect resource = other.GetComponent<Resource_Collect>();
        if (resource != null)
        {
            resource.TakeHit(currentTool, currentDamage);
            appliedDamage = true;
            return;
        }

        if (currentTool == PlayerActionController.EquipType.Espada ||
            currentTool == PlayerActionController.EquipType.Hacha ||
            currentTool == PlayerActionController.EquipType.Pico)
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(currentDamage);
                appliedDamage = true;
            }

            if (!appliedDamage)
            {
                AngelBoss boss = other.GetComponent<AngelBoss>();
                if (boss != null)
                {
                    boss.TakeDamage(currentDamage);
                    appliedDamage = true;
                }
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
            Gizmos.DrawWireSphere(transform.position, hitRadius);

            if (transform.parent != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.parent.position, transform.position);
            }
        }
    }

    private void TryImmediateHit()
    {
        var cols = Physics2D.OverlapCircleAll(transform.position, hitRadius);
        for (int i = 0; i < cols.Length; i++)
        {
            var c = cols[i];
            if (actionController != null && c.gameObject == actionController.gameObject) continue;

            var resource = c.GetComponent<Resource_Collect>();
            if (resource != null)
            {
                resource.TakeHit(currentTool, currentDamage);
                appliedDamage = true;
                continue;
            }

            if (currentTool == PlayerActionController.EquipType.Espada ||
                currentTool == PlayerActionController.EquipType.Hacha ||
                currentTool == PlayerActionController.EquipType.Pico)
            {
                var enemy = c.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(currentDamage);
                    appliedDamage = true;
                    continue;
                }

                var boss = c.GetComponent<AngelBoss>();
                if (boss != null)
                {
                    boss.TakeDamage(currentDamage);
                    appliedDamage = true;
                }
            }
        }
    }
}
