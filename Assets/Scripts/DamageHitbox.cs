using UnityEngine;

public class DamageHitbox : MonoBehaviour
{
    private PlayerActionController.EquipType toolUsed;
    private int damage;

    public void Initialize(PlayerActionController.EquipType equip, int dmg)
    {
        toolUsed = equip;
        damage = dmg;
    }

private void OnTriggerEnter2D(Collider2D other)
    {
        Resource_Collect resourceNode = other.GetComponent<Resource_Collect>();
        if (resourceNode != null)
        {
            resourceNode.TakeHit(toolUsed, damage);
            gameObject.SetActive(false);
            return;
        }
        
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null && toolUsed == PlayerActionController.EquipType.Espada)
        {
            Debug.Log($"Jugador hizo {damage} de daño a {enemyHealth.gameObject.name}");
            enemyHealth.TakeDamage(damage);
            gameObject.SetActive(false);
            return;
        }
    }
}
