using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthSlider;
    public Image healthFill;
    public Text healthText;
    
    [Header("Health Bar Colors")]
    public Color fullHealthColor = Color.green;
    public Color mediumHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;
    
    [Header("Animation Settings")]
    public bool enableSmoothTransition = true;
    public float transitionSpeed = 5f;
    
    private PlayerHealth playerHealth;
    private float targetHealthPercentage;
    
    private void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        
        if (playerHealth == null)
        {
            Debug.LogError("No se encontró PlayerHealth en la escena");
            return;
        }
        
        if (healthSlider == null)
        {
            Debug.LogError("Health Slider no está asignado");
            return;
        }
        
        InitializeHealthUI();
    }
    
    private void Update()
    {
        if (playerHealth == null) return;
        
        UpdateHealthUI();
    }
    
    private void InitializeHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = 1f;
            healthSlider.value = playerHealth.GetHealthPercentage();
        }
        
        targetHealthPercentage = playerHealth.GetHealthPercentage();
        UpdateHealthDisplay();
    }
    
private void UpdateHealthUI()
    {
        if (playerHealth == null || healthSlider == null) return;
        
        float currentHealthPercentage = playerHealth.GetHealthPercentage();
        
        if (Mathf.Abs(targetHealthPercentage - currentHealthPercentage) > 0.01f)
        {
            targetHealthPercentage = currentHealthPercentage;
        }
        
        if (enableSmoothTransition)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealthPercentage, Time.deltaTime * transitionSpeed);
        }
        else
        {
            healthSlider.value = targetHealthPercentage;
        }
        
        UpdateHealthDisplay();
    }
    
private void UpdateHealthDisplay()
    {
        if (playerHealth == null) return;
        
        if (healthText != null)
        {
            healthText.text = $"{playerHealth.GetCurrentHealth()}/{playerHealth.GetMaxHealth()}";
        }
        
        if (healthFill != null && healthSlider != null)
        {
            float healthPercentage = healthSlider.value;
            
            if (healthPercentage > 0.6f)
            {
                healthFill.color = fullHealthColor;
            }
            else if (healthPercentage > 0.3f)
            {
                healthFill.color = mediumHealthColor;
            }
            else
            {
                healthFill.color = lowHealthColor;
            }
        }
    }
}