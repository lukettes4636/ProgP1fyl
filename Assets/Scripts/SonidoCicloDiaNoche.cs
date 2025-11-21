using UnityEngine;

public class SonidoCicloDiaNoche : MonoBehaviour
{
    public AudioClip sonidoDia;
    public AudioClip sonidoNoche;
    
    private AudioSource audioSource;
    public CicloDiaNoche cicloDiaNoche;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        audioSource.loop = true;
        
        
    }
    
    void Update()
    {
        if (cicloDiaNoche == null) return;
        
        bool esNoche = cicloDiaNoche.EsDeNoche();
        
        if (esNoche)
        {
            if (audioSource.clip != sonidoNoche)
            {
                audioSource.clip = sonidoNoche;
                if (sonidoNoche != null) audioSource.Play();
            }
        }
        else
        {
            if (audioSource.clip != sonidoDia)
            {
                audioSource.clip = sonidoDia;
                if (sonidoDia != null) audioSource.Play();
            }
        }
    }
}
