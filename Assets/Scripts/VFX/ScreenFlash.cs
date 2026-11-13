using UnityEngine;
using UnityEngine.UI;

public class ScreenFlash : MonoBehaviour
{
    [SerializeField] private float peakAlpha = 0.75f;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private int sortingOrder = 100;

    private Image image;
    private float remaining;

    public float CurrentAlpha => image != null ? image.color.a : 0f;

    private void Awake()
    {
        GameObject canvasObject = new GameObject("FlashCanvas");
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;

        GameObject imageObject = new GameObject("Flash");
        imageObject.transform.SetParent(canvasObject.transform, false);
        image = imageObject.AddComponent<Image>();
        image.raycastTarget = false;
        image.color = new Color(1f, 1f, 1f, 0f);

        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void Update()
    {
        if (remaining <= 0f)
        {
            return;
        }

        remaining = Mathf.Max(0f, remaining - Time.unscaledDeltaTime);
        float t = remaining / duration;
        image.color = new Color(1f, 1f, 1f, peakAlpha * t * t);
    }

    public void Play()
    {
        remaining = duration;
        image.color = new Color(1f, 1f, 1f, peakAlpha);
    }
}
