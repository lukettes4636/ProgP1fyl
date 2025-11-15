using UnityEngine;
using UnityEngine.SceneManagement;
// Comentario: Menú principal simple para iniciar o salir del juego

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
