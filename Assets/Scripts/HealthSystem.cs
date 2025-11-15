using UnityEngine;
using UnityEngine.Events;
// Comentario: Sistema de salud genérico con eventos para daño, curación y muerte
// Comentario: Mantiene los mismos parámetros y comportamiento actual

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public bool destroyOnDeath = true;
    
    [Header("Events")]
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnDeath;
    public UnityEvent<int> OnDamageTaken;
    public UnityEvent<int> OnHealed;
    
    private int currentHealth;
    private bool isDead = false;
    
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;
    public float HealthPercentage => (float)currentHealth / maxHealth;
    
    // Comentario: Inicializa la salud al valor máximo al iniciar
    private void Awake()
    {
        currentHealth = maxHealth;
    }
    
    // Comentario: Aplica daño y dispara eventos asociados
    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0) return;
        
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // Comentario: Cura una cantidad y dispara eventos si cambia la salud
    public void Heal(int amount)
    {
        if (isDead || amount <= 0) return;
        
        int oldHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        
        if (currentHealth != oldHealth)
        {
            OnHealed?.Invoke(amount);
            OnHealthChanged?.Invoke(currentHealth);
        }
    }
    
    // Comentario: Establece la salud a un valor concreto dentro de límites
    public void SetHealth(int newHealth)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // Comentario: Restaura salud completa si no está muerto
    public void RestoreFullHealth()
    {
        if (isDead) return;
        
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    // Comentario: Marca el estado de muerte y dispara el evento de muerte
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        OnDeath?.Invoke();
        
        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }
    
    // Comentario: Revive al objeto restaurando salud máxima
    public void Revive()
    {
        isDead = false;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }
}
