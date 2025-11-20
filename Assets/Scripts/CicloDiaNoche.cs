using UnityEngine;

public class CicloDiaNoche : MonoBehaviour
{
    [Header("Configuración del Ciclo")]
    [Tooltip("Duración del ciclo completo en segundos")]
    public float duracionCiclo = 120f;

    [Tooltip("Hora inicial (0-24)")]
    [Range(0, 24)]
    public float horaInicial = 6f;

    [Header("Colores")]
    public Color colorNoche = new Color(0f, 0f, 0f, 0.5f); 

    [Header("Referencias")]
    [Tooltip("Asigna el SpriteRenderer que hará de overlay")]
    public SpriteRenderer overlayNoche;

    [Header("Configuración de Capas")]
    [Tooltip("Sorting Layer del overlay")]
    public string sortingLayerName = "Roofs";
    
    [Tooltip("Order in Layer del overlay")]
    public int orderInLayer = 1000;

    [Tooltip("Layers que serán afectadas por el ciclo día/noche")]
    public LayerMask affectedLayers = -1;

    [Header("Luces Nocturnas")]
    [Tooltip("Luces que se activarán durante la noche")]
    public GameObject[] lucesNocturnas;

    [Header("Sonidos Ambientales")]
    [Tooltip("Sonido que se reproduce durante el día")]
    public AudioClip sonidoDia;

    [Tooltip("Sonido que se reproduce durante la noche (grillos, lobos, etc.)")]
    public AudioClip sonidoNoche;

    private float tiempoActual;
    private AudioSource audioSource;

    void Start()
    {
        if (overlayNoche == null)
        {
            CrearOverlay();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
        }

        tiempoActual = horaInicial;
    }

    void Update()
    {
        tiempoActual += (24f / duracionCiclo) * Time.deltaTime;

        if (tiempoActual >= 24f)
        {
            tiempoActual = 0f;
        }

        ActualizarColor();
        ActualizarLucesYSonidos();
    }

    void ActualizarColor()
    {
        float t = 0f;

        if (tiempoActual >= 4f && tiempoActual < 8f)
        {
            t = 1f - Mathf.Clamp01((tiempoActual - 4f) / 4f);
        }
        else if (tiempoActual >= 8f && tiempoActual < 16f)
        {
            t = 0f;
        }
        else if (tiempoActual >= 16f && tiempoActual < 20f)
        {
            t = Mathf.Clamp01((tiempoActual - 16f) / 4f);
        }
        else
        {
            t = 1f;
        }

        if (overlayNoche != null)
        {
            Color newColor = colorNoche;
            newColor.a = Mathf.Lerp(0f, colorNoche.a, t);
            overlayNoche.color = newColor;
        }
    }

    void ActualizarLucesYSonidos()
    {
        bool esNoche = (tiempoActual < 4f || tiempoActual >= 20f);

        foreach (GameObject luz in lucesNocturnas)
        {
            if (luz != null && luz.activeSelf != esNoche)
            {
                luz.SetActive(esNoche);
            }
        }

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

        if (audioSource.clip == null)
        {
            audioSource.Stop();
        }
    }

    public bool EsDeNoche()
    {
        return tiempoActual < 4f || tiempoActual >= 20f;
    }

    void CrearOverlay()
    {
        GameObject overlayObj = new GameObject("Overlay Noche");
        overlayObj.transform.SetParent(transform);
        overlayObj.transform.localPosition = Vector3.zero;

        overlayNoche = overlayObj.AddComponent<SpriteRenderer>();

        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        overlayNoche.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);

        Camera cam = Camera.main;
        float altura = cam.orthographicSize * 2f;
        float ancho = altura * cam.aspect;
        overlayObj.transform.localScale = new Vector3(ancho * 2f, altura * 2f, 1f);

        overlayNoche.sortingLayerName = sortingLayerName;
        overlayNoche.sortingOrder = orderInLayer;

        Debug.Log("Overlay de noche creado automáticamente");
    }

    public string ObtenerHoraActual()
    {
        int horas = Mathf.FloorToInt(tiempoActual);
        int minutos = Mathf.FloorToInt((tiempoActual - horas) * 60f);
        return string.Format("{0:00}:{1:00}", horas, minutos);
    }
}