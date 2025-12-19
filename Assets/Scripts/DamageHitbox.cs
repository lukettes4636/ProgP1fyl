using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageHitbox : MonoBehaviour
{
    [SerializeField] private float hitboxDistance = 0.6f;
    [SerializeField] private float hitboxLifeTime = 0.2f;
    [SerializeField] private float impactRadius = 0.3f;

    private PlayerActionController actionController;
    private PlayerMovement playerMovement;
    private int currentDamage;
    private PlayerActionController.EquipType currentTool;
    private bool damageApplied;

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
        damageApplied = false;

        TryImmediateImpact();

        Invoke(nameof(DeactivateHitbox), hitboxLifeTime);
    }

    public void DeactivateHitbox()
    {
        CancelInvoke(nameof(DeactivateHitbox));
        gameObject.SetActive(false);
        transform.localPosition = Vector3.zero;
        damageApplied = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (damageApplied) return;

        ResourceCollect resource = other.GetComponent<ResourceCollect>();
        if (resource != null)
        {
            resource.ReceiveHit(currentTool, currentDamage);
            damageApplied = true;
            return;
        }

        if (currentTool == PlayerActionController.EquipType.Sword ||
            currentTool == PlayerActionController.EquipType.Axe ||
            currentTool == PlayerActionController.EquipType.Pickaxe)
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(currentDamage);
                damageApplied = true;
            }

            if (!damageApplied)
            {
                AngelBoss boss = other.GetComponent<AngelBoss>();
                if (boss != null)
                {
                    boss.TakeDamage(currentDamage);
                    damageApplied = true;
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
            Gizmos.DrawWireSphere(transform.position, impactRadius);

            if (transform.parent != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.parent.position, transform.position);
            }
        }
    }

    private void TryImmediateImpact()
    {
        var cols = Physics2D.OverlapCircleAll(transform.position, impactRadius);
        for (int i = 0; i < cols.Length; i++)
        {
            var c = cols[i];
            if (actionController != null && c.gameObject == actionController.gameObject) continue;

            var resource = c.GetComponent<ResourceCollect>();
            if (resource != null)
            {
                resource.ReceiveHit(currentTool, currentDamage);
                damageApplied = true;
                continue;
            }

            if (currentTool == PlayerActionController.EquipType.Sword ||
                currentTool == PlayerActionController.EquipType.Axe ||
                currentTool == PlayerActionController.EquipType.Pickaxe)
            {
                var enemy = c.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(currentDamage);
                    damageApplied = true;
                    continue;
                }

                var boss = c.GetComponent<AngelBoss>();
                if (boss != null)
                {
                    boss.TakeDamage(currentDamage);
                    damageApplied = true;
                }
            }
        }
    }
}
