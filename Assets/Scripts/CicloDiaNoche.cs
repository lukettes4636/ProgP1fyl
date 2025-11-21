using UnityEngine;

public class CicloDiaNoche : MonoBehaviour
{
    public float duracionCiclo = 120f;

    [Range(0, 24)]
    public float horaInicial = 6f;

    public Color colorNoche = new Color(0f, 0f, 0f, 0.5f); 

    public SpriteRenderer overlayNoche;

    public GameObject[] lucesNocturnas;

    private float tiempoActual;

    void Start()
    {
        tiempoActual = horaInicial;
    }

    void Update()
    {
        tiempoActual += (24f / duracionCiclo) * Time.deltaTime;
        if (tiempoActual >= 24f) tiempoActual = 0f;
        float t = 0f;
        if (tiempoActual >= 4f && tiempoActual < 8f) t = 1f - Mathf.Clamp01((tiempoActual - 4f) / 4f);
        else if (tiempoActual >= 8f && tiempoActual < 16f) t = 0f;
        else if (tiempoActual >= 16f && tiempoActual < 20f) t = Mathf.Clamp01((tiempoActual - 16f) / 4f);
        else t = 1f;
        if (overlayNoche != null)
        {
            Color c = colorNoche;
            c.a = Mathf.Lerp(0f, colorNoche.a, t);
            overlayNoche.color = c;
        }
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
