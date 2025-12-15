using UnityEngine;

public class AngelCreature : SummonedCreature
{
    [SerializeField] private int healAmount = 30;
    [SerializeField] private float healRange = 5f;
    [SerializeField] private float healCooldown = 3f;
    [SerializeField] private KeyCode healKey = KeyCode.Q;
    
    [SerializeField] private GameObject healEffect;
    [SerializeField] private AudioClip healSound;
    
    private PlayerHealth playerHealth;
    private float lastHealTime;
    private bool isOnCooldown = false;
    private AudioSource audioSource;
    
    protected override void Awake()
    {
        base.Awake();
        
        playerHealth = FindObjectOfType<PlayerHealth>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    protected override void Start()
    {
        base.Start();
        SetCreatureType(CreatureType.Angel);
        
        attackDamage = 0;
        attackRange = 0f;
    }
    
    protected override void Update()
    {
        if (isOnCooldown)
        {
            if (Time.time - lastHealTime >= healCooldown)
            {
                isOnCooldown = false;
            }
        }
        
        if (Input.GetKeyDown(healKey) && !isOnCooldown)
        {
            TryHealPlayer();
        }
        
        base.Update();
    }
    
    protected override void HandleCombat()
    {
    }
    
    protected override void Attack(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            if (CanHeal() && !IsOnCooldown())
            {
                ForceHeal();
            }
        }
    }
    
    private void TryHealPlayer()
    {
        if (playerHealth == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerHealth.transform.position);
        if (distanceToPlayer > healRange) return;
        
        ExecuteHeal();
    }
    
    private void ExecuteHeal()
    {
        if (playerHealth == null) return;
        
        playerHealth.Heal(healAmount);
        
        if (healEffect != null)
        {
            GameObject effect = Instantiate(healEffect, playerHealth.transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        if (healSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(healSound);
        }
        
        lastHealTime = Time.time;
        isOnCooldown = true;
    }
    
    public bool CanHeal()
    {
        if (isOnCooldown) return false;
        if (playerHealth == null) return false;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerHealth.transform.position);
        return distanceToPlayer <= healRange;
    }
    
    public bool IsOnCooldown()
    {
        return isOnCooldown;
    }
    
    public void ForceHeal()
    {
        if (CanHeal())
        {
            ExecuteHeal();
        }
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, healRange);
    }
}