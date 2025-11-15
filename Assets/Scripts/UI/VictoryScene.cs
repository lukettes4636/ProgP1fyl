using UnityEngine;
using UnityEngine.SceneManagement;
// Comentario: Pantalla de victoria que permite volver al menú principal

public class VictoryScene : MonoBehaviour
{
    public void MainMenu()
    {
        SceneManager.LoadScene(0); 
    }
}
