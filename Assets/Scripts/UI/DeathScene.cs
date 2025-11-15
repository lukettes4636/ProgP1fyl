using UnityEngine;
using UnityEngine.SceneManagement;
// Comentario: Pantalla de muerte con opciones de reinicio o volver al menú

public class DeathScene : MonoBehaviour
{
    public void RestartGame()
    {
        SceneManager.LoadScene(1); 
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0); 
    }
}
