using UnityEngine;

public class CicloDiaNoche : MonoBehaviour
{
    public float duracionCiclo = 120f;

    [Range(0, 24)]
    public float horaInicial = 6f;

    public Color colorNoche = new Color(0f, 0f, 0f, 0.5f); 

    public SpriteRenderer overlayNoche;

    public GameObject[] lucesNocturnas;
    public Transform contenedorLuces;

    private float tiempoActual;

    [Header("Sonido Ambiente")]
    public AudioClip sonidoDeDia;
    public AudioClip sonidoDeNoche;
    [Range(0f, 1f)] public float volumenAmbiente = 0.5f;

    private AudioSource audioSourceDia;
    private AudioSource audioSourceNoche;

    void Start()
    {
        tiempoActual = horaInicial;

        if (contenedorLuces != null)
        {
            int childCount = contenedorLuces.childCount;
            lucesNocturnas = new GameObject[childCount];
            for (int i = 0; i < childCount; i++)
            {
                lucesNocturnas[i] = contenedorLuces.GetChild(i).gameObject;
            }
        }

        SetupAudio();
    }

    private void SetupAudio()
    {
        if (sonidoDeDia != null)
        {
            audioSourceDia = gameObject.AddComponent<AudioSource>();
            audioSourceDia.clip = sonidoDeDia;
            audioSourceDia.loop = true;
            audioSourceDia.volume = 0f;
            audioSourceDia.playOnAwake = false;
            audioSourceDia.Play();
        }

        if (sonidoDeNoche != null)
        {
            audioSourceNoche = gameObject.AddComponent<AudioSource>();
            audioSourceNoche.clip = sonidoDeNoche;
            audioSourceNoche.loop = true;
            audioSourceNoche.volume = 0f;
            audioSourceNoche.playOnAwake = false;
            audioSourceNoche.Play();
        }
    }

    void Update()
    {
        tiempoActual += (24f / duracionCiclo) * Time.deltaTime;
        if (tiempoActual >= 24f) tiempoActual = 0f;

        // Revisando Iluminacion...

        float t = 0f;
        
        // 04:00 - 08:00: Amanecer (Night -> Day)
        if (tiempoActual >= 4f && tiempoActual < 8f) 
        {
            t = 1f - Mathf.Clamp01((tiempoActual - 4f) / 4f);
        }
        // 08:00 - 16:00: Dia (Day)
        else if (tiempoActual >= 8f && tiempoActual < 16f) 
        {
            t = 0f;
        }
        // 16:00 - 20:00: Atardecer (Day -> Night)
        else if (tiempoActual >= 16f && tiempoActual < 20f) 
        {
            t = Mathf.Clamp01((tiempoActual - 16f) / 4f);
        }
        // 20:00 - 04:00: Noche (Night)
        else 
        {
            t = 1f;
        }

        if (overlayNoche != null)
        {
            Color c = colorNoche;
            c.a = Mathf.Lerp(0f, colorNoche.a, t);
            overlayNoche.color = c;
        }

        UpdateAudioVolumes(t);

        bool noche = tiempoActual < 4f || tiempoActual >= 20f;
        if (lucesNocturnas != null)
        {
            for (int i = 0; i < lucesNocturnas.Length; i++)
            {
                GameObject luz = lucesNocturnas[i];
                if (luz != null && luz.activeSelf != noche) luz.SetActive(noche);
            }
        }
    }

    private void UpdateAudioVolumes(float nightFactor)
    {
        // nightFactor: 0 = Dia completo, 1 = Noche completa

        if (audioSourceDia != null)
        {
            // Volumen dia es inverso a nightFactor
            audioSourceDia.volume = (1f - nightFactor) * volumenAmbiente;
        }

        if (audioSourceNoche != null)
        {
            // Volumen noche es directo a nightFactor
            audioSourceNoche.volume = nightFactor * volumenAmbiente;
        }
    }

    public bool EsDeNoche()
    {
        return tiempoActual < 4f || tiempoActual >= 20f;
    }

    public string ObtenerHoraActual()
    {
        int horas = Mathf.FloorToInt(tiempoActual);
        int minutos = Mathf.FloorToInt((tiempoActual - horas) * 60f);
        return string.Format("{0:00}:{1:00}", horas, minutos);
    }
}
