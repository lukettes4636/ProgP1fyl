using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject camaraMenu;
    public GameObject camaraJuego;
    public GameObject gameUIContainer; // <<-- NUEVA VARIABLE PARA TU OBJETO EMPTY CON EL HUD

    void Start()
    {
        menuCanvas.SetActive(true);
        camaraMenu.SetActive(true);
        camaraJuego.SetActive(false);

        // OCULTAMOS la UI del juego al iniciar
        if (gameUIContainer != null)
        {
            gameUIContainer.SetActive(false);
        }
    }

    public void BotonJugar()
    {
        menuCanvas.SetActive(false);
        camaraMenu.SetActive(false);
        camaraJuego.SetActive(true);

        // MOSTRAMOS la UI del juego al empezar a jugar
        if (gameUIContainer != null)
        {
            gameUIContainer.SetActive(true);
        }
    }

    public void BotonSalir()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}