using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] private Image splashImage;
    
    [SerializeField] private float fadeInDuration = 2f;
    
    [SerializeField] private float displayDuration = 3f;
    
    [SerializeField] private float fadeOutDuration = 2f;
    
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    private CanvasGroup canvasGroup;
    
    private void Start()
    {
        if (splashImage == null)
        {
            Debug.LogError("SplashImage no está asignado en el inspector. Por favor arrastra una Image al campo splashImage.");
            return;
        }
        
        canvasGroup = splashImage.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = splashImage.gameObject.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 0f;
        
        RectTransform rectTransform = splashImage.GetComponent<RectTransform>();
        rectTransform.localScale = new Vector3(0.5f, 0.5f, 1f);
        
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
