using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    public float cycleDuration = 120f;

    [Range(0, 24)]
    public float initialHour = 6f;

    public Color nightColor = new Color(0.1f, 0.1f, 0.25f, 1f); 
    public Color dayColor = Color.white;

    public Light2D globalLight;
    public float minIntensity = 0.2f;
    public float maxIntensity = 1.0f;

    public GameObject[] nightLights;
    public Transform lightsContainer;

    private float currentTime;

    [Header("Ambient Sound")]
    public AudioClip daySound;
    public AudioClip nightSound;
    [Range(0f, 1f)] public float ambientVolume = 0.5f;

    private AudioSource dayAudioSource;
    private AudioSource nightAudioSource;

    void Start()
    {
        currentTime = initialHour;

        if (lightsContainer != null)
        {
            int childCount = lightsContainer.childCount;
            nightLights = new GameObject[childCount];
            for (int i = 0; i < childCount; i++)
            {
                nightLights[i] = lightsContainer.GetChild(i).gameObject;
            }
        }

        SetupAudio();
    }

    private void SetupAudio()
    {
        if (daySound != null)
        {
            dayAudioSource = gameObject.AddComponent<AudioSource>();
            dayAudioSource.clip = daySound;
            dayAudioSource.loop = true;
            dayAudioSource.volume = 0f;
            dayAudioSource.playOnAwake = false;
            dayAudioSource.Play();
        }

        if (nightSound != null)
        {
            nightAudioSource = gameObject.AddComponent<AudioSource>();
            nightAudioSource.clip = nightSound;
            nightAudioSource.loop = true;
            nightAudioSource.volume = 0f;
            nightAudioSource.playOnAwake = false;
            nightAudioSource.Play();
        }
    }

    void Update()
    {
        currentTime += (24f / cycleDuration) * Time.deltaTime;
        if (currentTime >= 24f) currentTime = 0f;

        float t = 0f;
        
        if (currentTime >= 4f && currentTime < 8f) 
        {
            t = 1f - Mathf.Clamp01((currentTime - 4f) / 4f);
        }
        else if (currentTime >= 8f && currentTime < 16f) 
        {
            t = 0f;
        }
        else if (currentTime >= 16f && currentTime < 20f) 
        {
            t = Mathf.Clamp01((currentTime - 16f) / 4f);
        }
        else 
        {
            t = 1f;
        }

        if (globalLight != null)
        {
            globalLight.color = Color.Lerp(dayColor, nightColor, t);
            globalLight.intensity = Mathf.Lerp(maxIntensity, minIntensity, t);
        }

        UpdateAudioVolumes(t);

        bool isNight = currentTime < 4f || currentTime >= 20f;
        if (nightLights != null)
        {
            for (int i = 0; i < nightLights.Length; i++)
            {
                GameObject lightObj = nightLights[i];
                if (lightObj != null && lightObj.activeSelf != isNight) lightObj.SetActive(isNight);
            }
        }
    }

    private void UpdateAudioVolumes(float nightFactor)
    {
        if (dayAudioSource != null)
        {
            dayAudioSource.volume = (1f - nightFactor) * ambientVolume;
        }

        if (nightAudioSource != null)
        {
            nightAudioSource.volume = nightFactor * ambientVolume;
        }
    }

    public bool IsNight()
    {
        return currentTime < 4f || currentTime >= 20f;
    }

    public string GetCurrentTime()
    {
        int hours = Mathf.FloorToInt(currentTime);
        int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);
        return string.Format("{0:00}:{1:00}", hours, minutes);
    }
}
