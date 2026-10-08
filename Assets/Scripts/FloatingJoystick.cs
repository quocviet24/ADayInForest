using UnityEngine;
using UnityEngine.EventSystems;

public class FloatingJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform background;
    public RectTransform handle;

    [HideInInspector]
    public Vector2 inputVector;

    private RectTransform panelRect;

    private void Awake()
    {
        panelRect = GetComponent<RectTransform>();

        if (background != null)
            background.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 localPoint;

        // Truyền eventData.pressEventCamera vào thay vì null
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelRect,
            eventData.position,
            eventData.pressEventCamera, 
            out localPoint
        );

        background.anchoredPosition = localPoint;
        handle.anchoredPosition = Vector2.zero;
        background.gameObject.SetActive(true);
        inputVector = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 bgPoint;

        // Truyền eventData.pressEventCamera vào thay vì null
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera, 
            out bgPoint))
        {
            float radius = background.rect.width * 0.5f;
            inputVector = Vector2.ClampMagnitude(bgPoint / radius, 1f);
            handle.anchoredPosition = inputVector * radius;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
        background.gameObject.SetActive(false);
    }
}