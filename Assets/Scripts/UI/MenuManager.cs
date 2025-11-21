using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject gameUIContainer;
    public GameObject panelCreditos;
    public GameObject panelControles;
    public GameObject camaraMenu;
    public GameObject camaraJuego;

    void Start()
    {
        menuCanvas.SetActive(true);
        camaraMenu.SetActive(true);
        camaraJuego.SetActive(false);

        if (gameUIContainer != null) gameUIContainer.SetActive(false);

        if (panelCreditos != null) panelCreditos.SetActive(false);
        if (panelControles != null) panelControles.SetActive(false);
    }

    public void BotonJugar()
    {
        menuCanvas.SetActive(false);
        camaraMenu.SetActive(false);
        camaraJuego.SetActive(true);

        if (gameUIContainer != null) gameUIContainer.SetActive(true);
    }

    public void BotonSalir()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void AbrirCreditos()
    {
        panelCreditos.SetActive(true);
    }

    public void CerrarCreditos()
    {
        panelCreditos.SetActive(false);
    }

    public void AbrirControles()
    {
        panelControles.SetActive(true);
    }

    public void CerrarControles()
    {
        panelControles.SetActive(false);
    }
}