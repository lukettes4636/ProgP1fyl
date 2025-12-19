using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Scene Configuration")]
    public string gameplaySceneName = "Gameplay";
    public string menuSceneName = "MenuPrincipal";

    [Header("Original References")]
    public GameObject menuCanvas;
    public GameObject gameUIContainer;
    public GameObject creditsPanel;
    public GameObject controlsPanel;
    public GameObject menuCamera;
    public GameObject gameCamera;

    public AudioClip clickSound;
    public AudioClip hoverSound;
    private CanvasGroup menuGroup;

    void Start()
    {
        if (menuCanvas != null) menuCanvas.SetActive(true);
        if (menuCamera != null) menuCamera.SetActive(true);
        if (gameCamera != null) gameCamera.SetActive(false);

        if (gameUIContainer != null) gameUIContainer.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);

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

    public void PlayButton()
    {
        if (menuGroup != null)
        {
            menuGroup.alpha = 0f;
            menuGroup.interactable = false;
            menuGroup.blocksRaycasts = false;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void RetryFromVictoryButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    public void QuitButton()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OpenCredits()
    {
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void OpenControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    public void CloseControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    private void ApplyButtonEffects()
    {
        if (menuCanvas != null) ApplyToRoot(menuCanvas);
        if (creditsPanel != null) ApplyToRoot(creditsPanel);
        if (controlsPanel != null) ApplyToRoot(controlsPanel);
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
            effect.ConfigureSounds(hoverSound, clickSound);
            effect.Initialize();
        }
    }
}
