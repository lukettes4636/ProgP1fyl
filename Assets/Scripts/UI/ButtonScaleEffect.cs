using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Scale Settings")]
    public float scaleFactor = 1.1f; 
    public float animationDuration = 0.2f; 
    
    [Header("Sound")]
    public AudioClip hoverSound;
    [Range(0f,1f)] public float hoverVolume = 0.8f;
    public AudioClip clickSound;
    [Range(0f,1f)] public float clickVolume = 1.0f;
    private AudioSource audioSource;

    private Vector3 originalScale;
    private bool isHoveredOrSelected;


    private void Start()
    {
        originalScale = transform.localScale;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(PlayClickSound);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHoveredOrSelected = true;
        StopAllCoroutines();
        StartCoroutine(ScaleTo(scaleFactor));
        PlayHoverSound();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHoveredOrSelected = false;
        StopAllCoroutines();
        StartCoroutine(ScaleTo(1f));
    }

    public void OnSelect(BaseEventData eventData)
    {
        isHoveredOrSelected = true;
        StopAllCoroutines();
        StartCoroutine(ScaleTo(scaleFactor));
        PlayHoverSound();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isHoveredOrSelected = false;
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
