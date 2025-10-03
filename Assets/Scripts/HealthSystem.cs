using UnityEngine;
using UnityEngine.Events;

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
    
    private void Awake()
    {
        currentHealth = maxHealth;
    }
    
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
    
    public void RestoreFullHealth()
    {
        if (isDead) return;
        
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }
    
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
    
    public void Revive()
    {
        isDead = false;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }
}
