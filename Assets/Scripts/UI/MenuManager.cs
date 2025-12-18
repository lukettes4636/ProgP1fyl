using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement; // Necesario para el cambio de escenas

public class MenuManager : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    public string nombreEscenaGameplay = "Gameplay";
    public string nombreEscenaMenu = "MenuPrincipal";

    [Header("Referencias Originales")]
    public GameObject menuCanvas;
    public GameObject gameUIContainer;
    public GameObject panelCreditos;
    public GameObject panelControles;
    public GameObject camaraMenu;
    public GameObject camaraJuego;

    public AudioClip clickSound;
    private CanvasGroup menuGroup;

    void Start()
    {
        // Mantiene tu lógica original de inicio
        if (menuCanvas != null) menuCanvas.SetActive(true);
        if (camaraMenu != null) camaraMenu.SetActive(true);
        if (camaraJuego != null) camaraJuego.SetActive(false);

        if (gameUIContainer != null) gameUIContainer.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(false);
        if (panelControles != null) panelControles.SetActive(false);

        ApplyButtonEffects();

        if (menuCanvas != null)
        {
            menuGroup = menuCanvas.GetComponent<CanvasGroup>();
            if (menuGroup == null) menuGroup = menuCanvas.AddComponent<CanvasGroup>();
            menuGroup.alpha = 1f;
            menuGroup.interactable = true;
            menuGroup.blocksRaycasts = true;
        }
    }

    // --- NUEVA FUNCIÓN: JUGAR (Cambia a la escena de Gameplay) ---
    public void BotonJugar()
    {
        // Mantiene la lógica de ocultar el menú por si la transición tarda un poco
        if (menuGroup != null)
        {
            menuGroup.alpha = 0f;
            menuGroup.interactable = false;
            menuGroup.blocksRaycasts = false;
        }

        Time.timeScale = 1f;
        // Cambia a la escena de juego
        SceneManager.LoadScene(nombreEscenaGameplay);
    }

    // --- NUEVA FUNCIÓN: REINTENTAR (Para la escena de Victoria aparte) ---
    public void BotonReintentarDesdeVictoria()
    {
        Time.timeScale = 1f;
        // Carga la escena de juego limpia desde el principio
        SceneManager.LoadScene(nombreEscenaGameplay);
    }

    // --- FUNCIÓN PARA VOLVER AL MENÚ ---
    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    public void BotonSalir()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- MÉTODOS DE PANELES (Mantenidos igual) ---

    public void AbrirCreditos()
    {
        if (panelCreditos != null) panelCreditos.SetActive(true);
    }

    public void CerrarCreditos()
    {
        if (panelCreditos != null) panelCreditos.SetActive(false);
    }

    public void AbrirControles()
    {
        if (panelControles != null) panelControles.SetActive(true);
    }

    public void CerrarControles()
    {
        if (panelControles != null) panelControles.SetActive(false);
    }

    // --- EFECTOS DE BOTONES (Mantenidos igual) ---

    private void ApplyButtonEffects()
    {
        if (menuCanvas != null) ApplyToRoot(menuCanvas);
        if (panelCreditos != null) ApplyToRoot(panelCreditos);
        if (panelControles != null) ApplyToRoot(panelControles);
    }

    private void ApplyToRoot(GameObject root)
    {
        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            ButtonScaleEffect effect = btn.GetComponent<ButtonScaleEffect>();
            if (effect == null)
            {
                effect = btn.gameObject.AddComponent<ButtonScaleEffect>();
            }
            effect.ConfigureSounds(clickSound);
            effect.Initialize();
        }
    }
}