using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashScreen : MonoBehaviour
{
[Header("Splash Screen Settings")]
[Tooltip("Drag the logo image for the splash screen")]
    [SerializeField] private Image splashImage;
    
[Tooltip("Logo fade-in duration (seconds)")]
    [SerializeField] private float fadeInDuration = 2f;
    
[Tooltip("Time the logo is displayed (seconds)")]
    [SerializeField] private float displayDuration = 3f;
    
[Tooltip("Logo fade-out duration (seconds)")]
    [SerializeField] private float fadeOutDuration = 2f;
    
[Tooltip("Main menu scene name to load")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    private CanvasGroup canvasGroup;
    
    private void Start()
    {
        if (splashImage == null)
        {
            Debug.LogError("Splash image not assigned");
            return;
        }
        
        canvasGroup = splashImage.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = splashImage.gameObject.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 0f;
        
        StartCoroutine(ShowSplashScreen());
    }
    
    private IEnumerator ShowSplashScreen()
    {
        yield return new WaitForSeconds(0.5f);
        
        yield return StartCoroutine(FadeIn());
        
        yield return new WaitForSeconds(displayDuration);
        
        yield return StartCoroutine(FadeOut());
        
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1 - (elapsedTime / fadeOutDuration));
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
    }
}
