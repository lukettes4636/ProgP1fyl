using UnityEngine;
using UnityEngine.UI;

public class CicloDiaNoche : MonoBehaviour
{
    public float duracionCiclo = 120f;

    [Range(0, 24)]
    public float horaInicial = 6f;

    public Color colorNoche = new Color(0f, 0f, 0f, 0.5f); 

    public SpriteRenderer overlayNoche;

    public string sortingLayerName = "Roofs";
    
    public int orderInLayer = 1000;

    public LayerMask affectedLayers = -1;

    public GameObject[] lucesNocturnas;

    public AudioClip sonidoDia;

    public AudioClip sonidoNoche;

    private float tiempoActual;
    [SerializeField] private bool controlAudio = false;
    private AudioSource audioSource;
    public float vignetteDuration = 0.6f;
    public float vignetteAlpha = 0.4f;
    public int vignetteSize = 256;
    public float vignettePower = 2f;
    private Image vignetteImage;
    private bool lastNightState;
    private Coroutine vignetteCo;

    void Start()
    {
        if (overlayNoche == null)
        {
            CrearOverlay();
        }

        if (controlAudio)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.loop = true;
            }
        }

        tiempoActual = horaInicial;
        lastNightState = EsDeNoche();
        var canvasGO = new GameObject("VignetteCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32766;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        var overlayGO = new GameObject("VignetteOverlay");
        overlayGO.transform.SetParent(canvasGO.transform);
        vignetteImage = overlayGO.AddComponent<Image>();
        var rt = overlayGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Texture2D tex = new Texture2D(vignetteSize, vignetteSize);
        int s = vignetteSize;
        float half = s * 0.5f;
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float nx = (x - half) / half;
                float ny = (y - half) / half;
                float r = Mathf.Sqrt(nx * nx + ny * ny);
                float a = Mathf.Clamp01(Mathf.Pow(r, vignettePower));
                tex.SetPixel(x, y, new Color(0f, 0f, 0f, a));
            }
        }
        tex.Apply();
        vignetteImage.sprite = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 1f);
        var a0 = lastNightState ? vignetteAlpha : 0f;
        vignetteImage.color = new Color(0f, 0f, 0f, a0);
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

        if (vignetteImage != null)
        {
            if (lastNightState != esNoche)
            {
                if (vignetteCo != null) StopCoroutine(vignetteCo);
                float target = esNoche ? vignetteAlpha : 0f;
                vignetteCo = StartCoroutine(FadeVignetteTo(target));
                lastNightState = esNoche;
            }
        }

        if (controlAudio && audioSource != null)
        {
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

    }

    public string ObtenerHoraActual()
    {
        int horas = Mathf.FloorToInt(tiempoActual);
        int minutos = Mathf.FloorToInt((tiempoActual - horas) * 60f);
        return string.Format("{0:00}:{1:00}", horas, minutos);
    }

    private System.Collections.IEnumerator FadeVignetteTo(float targetAlpha)
    {
        float startA = vignetteImage.color.a;
        float t = 0f;
        while (t < vignetteDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / vignetteDuration);
            float a = Mathf.Lerp(startA, targetAlpha, k);
            var c = vignetteImage.color;
            vignetteImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }
    }
}
