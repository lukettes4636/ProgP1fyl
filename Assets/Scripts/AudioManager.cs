using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Ambient Audio")]
    public AudioClip ambientClip;
    public float ambientVolume = 0.5f;
    public bool playOnStart = true;
    
    private AudioSource ambientSource;
    
    void Awake()
    {
        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.clip = ambientClip;
        ambientSource.volume = ambientVolume;
        ambientSource.loop = true;
        ambientSource.playOnAwake = false;
    }
    
    void Start()
    {
        if (playOnStart && ambientClip != null)
        {
            PlayAmbient();
        }
    }
    
    public void PlayAmbient()
    {
        if (ambientSource != null && ambientClip != null)
        {
            ambientSource.Play();
        }
    }
    
    public void StopAmbient()
    {
        if (ambientSource != null)
        {
            ambientSource.Stop();
        }
    }
    
    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        if (ambientSource != null)
        {
            ambientSource.volume = ambientVolume;
        }
    }
}