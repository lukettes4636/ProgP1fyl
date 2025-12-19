using UnityEngine;

public class DayNightCycleSound : MonoBehaviour
{
    public AudioClip daySound;
    public AudioClip nightSound;
    
    private AudioSource audioSource;
    public DayNightCycle dayNightCycle;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        audioSource.loop = true;
    }
    
    void Update()
    {
        if (dayNightCycle == null) return;
        
        bool isNight = dayNightCycle.IsNight();
        
        if (isNight)
        {
            if (audioSource.clip != nightSound)
            {
                audioSource.clip = nightSound;
                if (nightSound != null) audioSource.Play();
            }
        }
        else
        {
            if (audioSource.clip != daySound)
            {
                audioSource.clip = daySound;
                if (daySound != null) audioSource.Play();
            }
        }
    }
}
