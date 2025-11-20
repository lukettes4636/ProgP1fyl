using UnityEngine;

public class DustParticles : MonoBehaviour
{
    [SerializeField] private ParticleSystem dustParticles;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private float minEmissionRate = 5f;
    [SerializeField] private float maxEmissionRate = 20f;
    [SerializeField] private float minSpeedForDust = 0.1f;
    
    private ParticleSystem.EmissionModule emissionModule;
    private Rigidbody2D rb;
    
    private void Start()
    {
        if (dustParticles == null)
        {
            dustParticles = GetComponentInChildren<ParticleSystem>();
        }
        
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
        
        rb = GetComponent<Rigidbody2D>();
        
        if (dustParticles != null)
        {
            emissionModule = dustParticles.emission;
            emissionModule.rateOverTime = 0f;
        }
    }
    
    private void Update()
    {
        if (dustParticles == null || playerMovement == null || rb == null)
            return;
        
        UpdateDustEmission();
    }
    
    private void UpdateDustEmission()
    {
        float speed = rb.velocity.magnitude;
        bool isRunning = playerMovement.IsRunning();
        bool isMoving = speed > minSpeedForDust;
        
        if (isRunning && isMoving)
        {
            float emissionRate = Mathf.Lerp(minEmissionRate, maxEmissionRate, speed / playerMovement.GetRunSpeed());
            emissionModule.rateOverTime = emissionRate;
            
            if (!dustParticles.isPlaying)
                dustParticles.Play();
        }
        else
        {
            emissionModule.rateOverTime = 0f;
            if (dustParticles.isPlaying)
                dustParticles.Stop();
        }
    }
    
    public void PlayDust()
    {
        if (dustParticles != null)
        {
            dustParticles.Play();
        }
    }
    
    public void StopDust()
    {
        if (dustParticles != null)
        {
            dustParticles.Stop();
        }
    }
}