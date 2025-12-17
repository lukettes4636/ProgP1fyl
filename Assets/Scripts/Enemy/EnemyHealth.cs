using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int vidaMaxima = 50;

    [Header("Loot Settings")]
    public GameObject objetoQueSuelta;
    public int cantidadASoltar = 3;
    [SerializeField] private string dropName = "Mineral";

    [Header("Audio Settings")]
    public AudioClip sonidoDaño;
    public AudioClip sonidoMuerte;

    [Header("Death Settings")]
    public float tiempoAntesDeDestruir = 0.5f;

    private int vidaActual;
    private bool estaMuerto = false;
    private AudioSource audioSource;
    private Animator animator;
    private EnemyShooter enemyShooter;
    private EnemyAI enemyAI;

    private void Start()
    {
        vidaActual = vidaMaxima;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        animator = GetComponent<Animator>();
        enemyShooter = GetComponent<EnemyShooter>();
        enemyAI = GetComponent<EnemyAI>();
    }

    public void TakeDamage(int cantidad)
    {
        if (estaMuerto) return;

        vidaActual -= cantidad;
        if (vidaActual < 0) vidaActual = 0;

        ReproducirSonido(sonidoDaño);

        // Reproducir animación de hit
        if (enemyShooter != null)
        {
            enemyShooter.PlayHitAnimation();
        }
        else if (enemyAI != null)
        {
            enemyAI.PlayHitAnimation();
        }

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        estaMuerto = true;

        ReproducirSonido(sonidoMuerte);

        // Reproducir animación de muerte
        if (enemyShooter != null)
        {
            enemyShooter.PlayDeathAnimation();
        }
        else if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Desactivar IA y componentes
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }

        if (enemyShooter != null)
        {
            enemyShooter.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        SoltarObjetos();

        if (GameManager.GetInstance() != null)
        {
            GameManager.GetInstance().EnemyDefeated();
        }

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