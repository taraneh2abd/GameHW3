using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class UiSound : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    [SerializeField] private float selectGraceTime = 0.2f;

    private Selectable selectable;
    private bool pointerInside;
    private float enabledTime;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();

        Button button = selectable as Button;
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }
    }

    private void OnEnable()
    {
        enabledTime = Time.unscaledTime;
        pointerInside = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (pointerInside || Time.unscaledTime - enabledTime < selectGraceTime)
        {
            return;
        }

        PlayMove();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
        PlayMove();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (selectable is Slider && selectable.IsInteractable() && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUiSelect();
        }
    }

    private void HandleClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUiButton();
        }
    }

    private void PlayMove()
    {
        if (selectable.IsInteractable() && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUiMove();
        }
    }
}
