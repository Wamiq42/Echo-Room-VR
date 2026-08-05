using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class VRScreenFade : MonoBehaviour
{
    [SerializeField, Min(0f)] private float fadeDuration = 0.45f;
    [SerializeField] private Color fadeColor = Color.black;

    private Canvas _canvas;
    private CanvasGroup _canvasGroup;

    public float FadeDuration
    {
        get { return fadeDuration; }
    }

    public void SetOpacity(float opacity)
    {
        EnsureCanvas();
        _canvasGroup.alpha = Mathf.Clamp01(opacity);
    }

    public IEnumerator FadeOut()
    {
        yield return FadeTo(1f);
    }

    public IEnumerator FadeIn()
    {
        yield return FadeTo(0f);
    }

    private IEnumerator FadeTo(float targetOpacity)
    {
        EnsureCanvas();
        RefreshCamera();

        float startOpacity = _canvasGroup.alpha;
        if (fadeDuration <= 0f)
        {
            _canvasGroup.alpha = targetOpacity;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startOpacity, targetOpacity, Mathf.Clamp01(elapsed / fadeDuration));
            yield return null;
        }

        _canvasGroup.alpha = targetOpacity;
    }

    private void EnsureCanvas()
    {
        if (_canvasGroup != null)
            return;

        GameObject canvasObject = new GameObject(
            "Maze Transition Fade",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasGroup));

        canvasObject.transform.SetParent(transform, false);

        _canvas = canvasObject.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceCamera;
        _canvas.sortingOrder = short.MaxValue;

        _canvasGroup = canvasObject.GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        GameObject imageObject = new GameObject("Black", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        Image image = imageObject.GetComponent<Image>();
        image.color = fadeColor;
        image.raycastTarget = false;

        EchoRoom.UI.UITKOverlayLayer.Apply(canvasObject);
        RefreshCamera();
    }

    private void RefreshCamera()
    {
        if (_canvas == null)
            return;

        Camera targetCamera = EchoRoom.UI.UITKOverlayLayer.FindRenderingCamera();
        if (targetCamera == null)
            return;

        _canvas.worldCamera = targetCamera;
        _canvas.planeDistance = Mathf.Max(targetCamera.nearClipPlane + 0.02f, 0.05f);
    }
}
