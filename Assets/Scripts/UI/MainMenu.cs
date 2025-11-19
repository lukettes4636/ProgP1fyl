using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Sprites para Botones")]
    public Sprite startButtonSprite;
    public Sprite exitButtonSprite;
    public Sprite creditsButtonSprite;
    public Sprite backgroundSprite;
    
    [Header("Referencias a Botones")]
    public Button startButton;
    public Button exitButton;
    public Button creditsButton;
    public Image backgroundImage;
    public CanvasGroup menuCanvasGroup;
    
    [Header("Configuración de Transiciones")]
    public float fadeInDuration = 1.2f;
    public float buttonFadeDuration = 1.2f;
    public float fadeOutDuration = 1.0f;
    public float delayBeforeLoad = 0.5f;
    public AnimationCurve menuFadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Range(1f,4f)] public float menuFadeExponent = 2.4f;
    public AnimationCurve buttonFadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Range(1f,4f)] public float buttonFadeExponent = 2.4f;
    
    private bool isTransitioning = false;
    
    [Header("Sonido")]
    public AudioClip ambientClip;
    [Range(0f,1f)] public float ambientVolume = 0.5f;
    private AudioSource ambientSource;
    public float ambientFadeOutDuration = 0.6f;

    private void Start()
    {
        ConfigureMenuSprites();
        SetupAmbientAudio();
        
        StartCoroutine(FadeIn());
        
        if (startButton != null) StartCoroutine(FadeInButton(startButton, buttonFadeDuration));
        if (creditsButton != null) StartCoroutine(FadeInButton(creditsButton, buttonFadeDuration));
        if (exitButton != null) StartCoroutine(FadeInButton(exitButton, buttonFadeDuration));
    }

    private void ConfigureMenuSprites()
    {
        if (backgroundSprite != null && backgroundImage != null)
        {
            backgroundImage.sprite = backgroundSprite;
        }
        
        if (startButtonSprite != null && startButton != null)
        {
            startButton.image.sprite = startButtonSprite;
        }
        
        if (exitButtonSprite != null && exitButton != null)
        {
            exitButton.image.sprite = exitButtonSprite;
        }
        
        if (creditsButtonSprite != null && creditsButton != null)
        {
            creditsButton.image.sprite = creditsButtonSprite;
        }
    }

    public void PlayGame()
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionToGame());
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowCredits()
    {
        Debug.Log("Mostrando créditos del juego");
    }
    
    private IEnumerator FadeIn()
    {
        if (menuCanvasGroup == null)
        {
            menuCanvasGroup = GetComponent<CanvasGroup>();
            if (menuCanvasGroup == null)
            {
                menuCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        menuCanvasGroup.alpha = 0f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeInDuration);
            float v = menuFadeCurve != null ? menuFadeCurve.Evaluate(t) : t;
            v = Mathf.Pow(v, menuFadeExponent);
            menuCanvasGroup.alpha = v;
            yield return null;
        }
        
        menuCanvasGroup.alpha = 1f;
        menuCanvasGroup.interactable = true;
        menuCanvasGroup.blocksRaycasts = true;
    }
    
    private IEnumerator FadeInButton(Button button, float duration)
    {
        Image buttonImage = button.GetComponent<Image>();
        Text buttonText = button.GetComponentInChildren<Text>();
        
        if (buttonImage == null || buttonText == null) yield break;
        
        Color imageColor = buttonImage.color;
        Color textColor = buttonText.color;
        
        buttonImage.color = new Color(imageColor.r, imageColor.g, imageColor.b, 0f);
        buttonText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
        button.interactable = false;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float v = buttonFadeCurve != null ? buttonFadeCurve.Evaluate(t) : t;
            v = Mathf.Pow(v, buttonFadeExponent);
            buttonImage.color = new Color(imageColor.r, imageColor.g, imageColor.b, v);
            buttonText.color = new Color(textColor.r, textColor.g, textColor.b, v);
            yield return null;
        }
        
        buttonImage.color = new Color(imageColor.r, imageColor.g, imageColor.b, 1f);
        buttonText.color = new Color(textColor.r, textColor.g, textColor.b, 1f);
        button.interactable = true;
    }
    
    private IEnumerator FadeOut()
    {
        if (menuCanvasGroup == null)
        {
            menuCanvasGroup = GetComponent<CanvasGroup>();
            if (menuCanvasGroup == null)
            {
                menuCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            menuCanvasGroup.alpha = Mathf.Clamp01(1 - (elapsedTime / fadeOutDuration));
            yield return null;
        }
        
        menuCanvasGroup.alpha = 0f;
    }
    
    private IEnumerator TransitionToGame()
    {
        isTransitioning = true;
        
        SetButtonsInteractable(false);
        
        yield return StartCoroutine(FadeOut());
        
        yield return StartCoroutine(FadeOutAmbient());
        yield return new WaitForSeconds(delayBeforeLoad);
        
        SceneManager.LoadScene("E1");
    }
    
    private void SetButtonsInteractable(bool interactable)
    {
        if (startButton != null) startButton.interactable = interactable;
        if (exitButton != null) exitButton.interactable = interactable;
        if (creditsButton != null) creditsButton.interactable = interactable;
    }
    
    private void SetupAmbientAudio()
    {
        if (ambientClip == null) return;
        ambientSource = GetComponent<AudioSource>();
        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
        }
        ambientSource.clip = ambientClip;
        ambientSource.volume = ambientVolume;
        ambientSource.loop = true;
        ambientSource.playOnAwake = false;
        ambientSource.spatialBlend = 0f;
        ambientSource.Play();
    }
    
    private IEnumerator FadeOutAmbient()
    {
        if (ambientSource == null) yield break;
        float startVol = ambientSource.volume;
        float t = 0f;
        while (t < ambientFadeOutDuration)
        {
            t += Time.deltaTime;
            ambientSource.volume = Mathf.Lerp(startVol, 0f, Mathf.Clamp01(t / ambientFadeOutDuration));
            yield return null;
        }
        ambientSource.Stop();
        ambientSource.volume = startVol;
    }
}