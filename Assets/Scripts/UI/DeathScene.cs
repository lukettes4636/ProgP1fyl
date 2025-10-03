using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScene : MonoBehaviour
{
    public void RestartGame()
    {
        SceneManager.LoadScene(1); // Assuming the main game scene is at index 1
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0); // Assuming the main menu scene is at index 0
    }
}