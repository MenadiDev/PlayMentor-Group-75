using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonScaleEffect : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Settings")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float pressScale = 0.95f;
    [SerializeField] private float animationSpeed = 10f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isInteractable = true;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        if (isInteractable)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                targetScale,
                Time.deltaTime * animationSpeed
            );
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isInteractable)
            targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isInteractable)
            targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isInteractable)
            targetScale = originalScale * pressScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isInteractable)
            targetScale = originalScale * hoverScale;
    }
}