using UnityEngine;
// Comentario: Controla música/ambiente de fondo de manera sencilla
// Comentario: Permite reproducir/pausar y ajustar volumen

public class AudioManager : MonoBehaviour
{
    [Header("Ambient Audio")]
    public AudioClip ambientClip;
    public float ambientVolume = 0.5f;
    public bool playOnStart = true;
    
    private AudioSource ambientSource;
    
    // Comentario: Crea y configura el AudioSource para el ambiente
    void Awake()
    {
        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.clip = ambientClip;
        ambientSource.volume = ambientVolume;
        ambientSource.loop = true;
        ambientSource.playOnAwake = false;
    }
    
    // Comentario: Reproduce al iniciar si está habilitado y hay clip
    void Start()
    {
        if (playOnStart && ambientClip != null)
        {
            PlayAmbient();
        }
    }
    
    // Comentario: Comienza a reproducir el audio ambiente
    public void PlayAmbient()
    {
        if (ambientSource != null && ambientClip != null)
        {
            ambientSource.Play();
        }
    }
    
    // Comentario: Detiene la reproducción del audio ambiente
    public void StopAmbient()
    {
        if (ambientSource != null)
        {
            ambientSource.Stop();
        }
    }
    
    // Comentario: Ajusta el volumen del ambiente (0-1)
    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        if (ambientSource != null)
        {
            ambientSource.volume = ambientVolume;
        }
    }
}
