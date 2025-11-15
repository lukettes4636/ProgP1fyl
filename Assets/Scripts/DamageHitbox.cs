using UnityEngine;
// Comentario: Caja de daño que aplica daño a recursos o enemigos cuando colisiona
// Comentario: Se activa/desactiva desde PlayerActionController

public class DamageHitbox : MonoBehaviour
{
    private PlayerActionController.EquipType toolUsed;
    private int damage;

    // Comentario: Inicializa el tipo de herramienta usada y el daño a aplicar
    public void Initialize(PlayerActionController.EquipType equip, int dmg)
    {
        toolUsed = equip;
        damage = dmg;
    }

    // Comentario: Detecta la colisión y aplica daño correspondiente
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
