using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int vidaMaxima = 50;

    [Header("Loot")]
    public GameObject objetoQueSuelta;
    public int cantidadASoltar = 3;
    [SerializeField] private string dropName = "Mineral";

    [Header("Sonidos")]
    public AudioClip sonidoDaño;
    public AudioClip sonidoMuerte;

    [Header("Animación de Muerte")]
    public float tiempoAntesDeDestruir = 0.5f; // Tiempo de la animación de muerte

    private int vidaActual;
    private bool estaMuerto = false;
    private AudioSource audioSource;
    private Animator animator;

    private void Start()
    {
        vidaActual = vidaMaxima;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int cantidad)
    {
        if (estaMuerto) return;

        vidaActual -= cantidad;
        if (vidaActual < 0) vidaActual = 0;

        Debug.Log($" {gameObject.name} recibió {cantidad} de daño. Vida: {vidaActual}/{vidaMaxima}");

        // Reproducir sonido
        ReproducirSonido(sonidoDaño);

        // Reproducir animación de hit
        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.PlayHitAnimation();
        }

        // Verificar si murió
        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        estaMuerto = true;
        Debug.Log($" {gameObject.name} ha muerto!");

        // Reproducir sonido
        ReproducirSonido(sonidoMuerte);

        // Activar animación de muerte
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Desactivar el AI para que no se mueva
        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }

        // Desactivar el Rigidbody para que no se mueva
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Soltar objetos
        SoltarObjetos();

        // Notificar al GameManager
        if (GameManager.GetInstance() != null)
        {
            GameManager.GetInstance().EnemyDefeated();
        }

        // Destruir después de la animación
        Destroy(gameObject, tiempoAntesDeDestruir);
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

                GameObject obj = Instantiate(objetoQueSuelta, posicionAleatoria, Quaternion.identity);

                LootDrop loot = obj.GetComponent<LootDrop>();
                if (loot != null)
                {
                    loot.SetResourceName(dropName);
                }

                CollectableItem col = obj.GetComponent<CollectableItem>();
                if (col != null)
                {
                    col.Initialize(dropName, 1, null);
                }
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