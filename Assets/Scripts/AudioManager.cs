using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
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

    public void FadeOutAmbient(float duration)
    {
        if (ambientSource == null) return;
        StopAllCoroutines();
        StartCoroutine(AmbientFadeOut(duration));
    }

    private IEnumerator AmbientFadeOut(float duration)
    {
        float startVol = ambientSource.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            ambientSource.volume = Mathf.Lerp(startVol, 0f, k);
            yield return null;
        }
        ambientSource.Stop();
        ambientSource.volume = ambientVolume;
    }
}

