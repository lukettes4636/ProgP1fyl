using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public float scaleFactor = 1.1f; 
    public float animationDuration = 0.2f; 
    
    public AudioClip hoverSound;
    public AudioClip clickSound;
    [Range(0f,1f)] public float hoverVolume = 0.4f;
    [Range(0f,1f)] public float clickVolume = 0.7f;
    
    private AudioSource audioSource;
    private bool isInitialized = false;

    private Vector3 originalScale;

    private void Start()
    {
        if(!isInitialized) Initialize();
    }

    public void Initialize()
    {
        originalScale = transform.localScale;
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveListener(PlayClickSound);
            btn.onClick.AddListener(PlayClickSound);
        }
        
        isInitialized = true;
    }

    public void ConfigureSounds(AudioClip hSound, AudioClip cSound)
    {
        hoverSound = hSound;
        clickSound = cSound;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(scaleFactor));
        PlayHoverSound();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(1f));
    }

    public void OnSelect(BaseEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(scaleFactor));
        PlayHoverSound();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(1f));
    }

    private System.Collections.IEnumerator ScaleTo(float targetScale)
    {
        Vector3 target = originalScale * targetScale;
        float elapsedTime = 0f;
        Vector3 startingScale = transform.localScale;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startingScale, target, elapsedTime / animationDuration);
            yield return null;
        }

        transform.localScale = target;
    }
    
    private void PlayHoverSound()
    {
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound, hoverVolume);
        }
    }

    private void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound, clickVolume);
        }
    }
}

