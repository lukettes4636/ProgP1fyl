using UnityEngine;
using UnityEngine.SceneManagement;
// Comentario: Administra el conteo de enemigos y cambia a la escena de victoria
// Comentario: Patrón sencillo con instancia única en la escena

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    private int enemyCount;

    // Comentario: Asegura una única instancia activa del GameManager
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Comentario: Cuenta enemigos al inicio buscando por tag
    void Start()
    {
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    // Comentario: Llamado cuando un enemigo es derrotado. Cambia de escena si no quedan.
    public void EnemyDefeated()
    {
        enemyCount--;
        if (enemyCount <= 0)
        {
            SceneManager.LoadScene(3); 
        }
    }

    // Comentario: Devuelve la instancia actual del GameManager
    public static GameManager GetInstance()
    {
        return instance;
    }
}
