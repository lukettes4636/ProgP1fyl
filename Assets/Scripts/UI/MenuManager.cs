using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
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
        menuCanvas.SetActive(true);
        camaraMenu.SetActive(true);
        camaraJuego.SetActive(false);

        if (gameUIContainer != null) gameUIContainer.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(false);
        if (panelControles != null) panelControles.SetActive(false);
        
        ApplyButtonEffects();
        EnsurePauseOverlay(false);
        menuGroup = menuCanvas.GetComponent<CanvasGroup>();
        if (menuGroup == null) menuGroup = menuCanvas.AddComponent<CanvasGroup>();
        menuGroup.alpha = 1f;
        menuGroup.interactable = true;
        menuGroup.blocksRaycasts = true;
    }

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

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

    public void BotonJugar()
    {
        menuGroup.alpha = 0f;
        menuGroup.interactable = false;
        menuGroup.blocksRaycasts = false;
        camaraMenu.SetActive(false);
        camaraJuego.SetActive(true);

        if (gameUIContainer != null) gameUIContainer.SetActive(true);
        EnsurePauseOverlay(false);
        isPaused = false;
        Time.timeScale = 1f;
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

    private GameObject pauseOverlay;
    private CanvasGroup overlayGroup;

    private void EnsurePauseOverlay(bool enable)
    {
        if (menuCanvas == null) return;
        if (pauseOverlay == null)
        {
            pauseOverlay = new GameObject("PauseOverlay");
            pauseOverlay.transform.SetParent(menuCanvas.transform, false);
            var rt = pauseOverlay.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = pauseOverlay.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 1f);
            pauseOverlay.transform.SetSiblingIndex(0);
            overlayGroup = pauseOverlay.AddComponent<CanvasGroup>();
            overlayGroup.alpha = 0f;
        }
        pauseOverlay.SetActive(enable);
    }

    private void TogglePause()
    {
        if (!isPaused)
        {
            menuCanvas.SetActive(true);
            camaraMenu.SetActive(true);
            camaraJuego.SetActive(false);
            if (gameUIContainer != null) gameUIContainer.SetActive(false);
            EnsurePauseOverlay(true);
            StartCoroutine(FadePauseIn());
            isPaused = true;
            Time.timeScale = 0f;
        }
        else
        {
            StartCoroutine(FadePauseOut());
        }
    }

    private System.Collections.IEnumerator FadePauseIn()
    {
        if (menuGroup != null)
        {
            menuGroup.alpha = 0f;
            menuGroup.interactable = true;
            menuGroup.blocksRaycasts = true;
        }
        if (overlayGroup != null) overlayGroup.alpha = 0f;
        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / 0.15f);
            if (menuGroup != null) menuGroup.alpha = a;
            if (overlayGroup != null) overlayGroup.alpha = a * 0.5f;
            yield return null;
        }
        if (menuGroup != null) menuGroup.alpha = 1f;
        if (overlayGroup != null) overlayGroup.alpha = 0.5f;
    }

    private System.Collections.IEnumerator FadePauseOut()
    {
        float t = 0f;
        float startMenu = menuGroup != null ? menuGroup.alpha : 1f;
        float startOverlay = overlayGroup != null ? overlayGroup.alpha : 0.5f;
        while (t < 0.15f)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(1f - (t / 0.15f));
            if (menuGroup != null) menuGroup.alpha = startMenu * a;
            if (overlayGroup != null) overlayGroup.alpha = startOverlay * a;
            yield return null;
        }
        menuGroup.alpha = 0f;
        menuGroup.interactable = false;
        menuGroup.blocksRaycasts = false;
        EnsurePauseOverlay(false);
        isPaused = false;
        Time.timeScale = 1f;
        camaraMenu.SetActive(false);
        camaraJuego.SetActive(true);
        if (gameUIContainer != null) gameUIContainer.SetActive(true);
    }
}
