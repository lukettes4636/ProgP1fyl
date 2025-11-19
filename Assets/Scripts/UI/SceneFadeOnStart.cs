using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFadeOnStart : MonoBehaviour
{
    public float fadeDuration = 1.2f;
    public Color overlayColor = Color.black;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Range(1f,4f)] public float curveExponent = 2.4f;

    private Image overlayImage;

    private void Start()
    {
        CreateOverlayIfNeeded();
        StartCoroutine(FadeIn());
    }

    private void CreateOverlayIfNeeded()
    {
        var canvasGO = new GameObject("SceneFadeCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var overlayGO = new GameObject("Overlay");
        overlayGO.transform.SetParent(canvasGO.transform);
        overlayImage = overlayGO.AddComponent<Image>();
        var rt = overlayGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        overlayImage.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 1f);
        overlayImage.raycastTarget = false;
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float tt = Mathf.Clamp01(t / fadeDuration);
            float e = curve.Evaluate(tt);
            e = Mathf.Pow(e, curveExponent);
            float a = 1f - e;
            var c = overlayImage.color;
            overlayImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }
        var final = overlayImage.color;
        overlayImage.color = new Color(final.r, final.g, final.b, 0f);
    }
}