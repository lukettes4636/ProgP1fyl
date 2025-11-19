using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int vidaMaxima = 50;

    [Header("Loot (Objetos que suelta)")]
    public GameObject objetoQueSuelta;
    public int cantidadASoltar = 3;

    [Header("Sonidos")]
    public AudioClip sonidoDaño;
    public AudioClip sonidoMuerte;

    private int vidaActual;
    private bool estaMuerto = false;
    private AudioSource audioSource;

    private void Start()
    {
        vidaActual = vidaMaxima;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // MÉTODO CORREGIDO - Ahora se llama TakeDamage
    public void TakeDamage(int cantidad)
    {
        if (estaMuerto) return;

        vidaActual -= cantidad;
        if (vidaActual < 0) vidaActual = 0;

        Debug.Log($"{gameObject.name} recibió {cantidad} de daño. Vida: {vidaActual}/{vidaMaxima}");
        ReproducirSonido(sonidoDaño);

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        estaMuerto = true;
        Debug.Log($"{gameObject.name} ha muerto!");

        ReproducirSonido(sonidoMuerte);
        SoltarObjetos();

        if (GameManager.GetInstance() != null)
        {
            GameManager.GetInstance().EnemyDefeated();
        }

        Destroy(gameObject);
    }

    private void SoltarObjetos()
    {
        if (objetoQueSuelta != null)
        {
            for (int i = 0; i < cantidadASoltar; i++)
            {
                Vector3 posicionAleatoria = transform.position + new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f),
                    0f
                );

                Instantiate(objetoQueSuelta, posicionAleatoria, Quaternion.identity);
            }
        }
    }

    private void ReproducirSonido(AudioClip sonido)
    {
        if (audioSource != null && sonido != null)
        {
            audioSource.PlayOneShot(sonido);
        }
    }

    public int ObtenerVidaActual()
    {
        return vidaActual;
    }

    public bool EstaVivo()
    {
        return !estaMuerto;
    }
}