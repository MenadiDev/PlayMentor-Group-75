using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Optional animation script for podium slots.
/// Animates the platform rising up when the leaderboard loads.
/// </summary>
public class PodiumSlot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform platformRect;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private TMP_Text avatarText;
    [SerializeField] private CanvasGroup slotCanvasGroup; // add CanvasGroup component to the slot

    [Header("Animation")]
    [SerializeField] private float animationDelay = 0f;   // 0 for 1st, 0.1 for 2nd, 0.2 for 3rd
    [SerializeField] private float animationDuration = 0.6f;
    [SerializeField] private float startYOffset = -200f; // how far below it starts

    private Vector2 targetPosition;

    void Awake()
    {
        // Hide slot initially — will fade in during animation
        if (slotCanvasGroup != null)
            slotCanvasGroup.alpha = 0f;
    }

    public void AnimateIn()
    {
        StartCoroutine(AnimateCoroutine());
    }

    private IEnumerator AnimateCoroutine()
    {
        // Wait for the stagger delay
        yield return new WaitForSeconds(animationDelay);

        // Store the target position
        targetPosition = platformRect != null
            ? platformRect.anchoredPosition
            : Vector2.zero;

        float elapsed = 0f;
        Vector2 startPos = targetPosition + new Vector2(0, startYOffset);

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;

            // Ease out cubic — starts fast, slows at end
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            // Move platform upward
            if (platformRect != null)
                platformRect.anchoredPosition = Vector2.Lerp(startPos, targetPosition, eased);

            // Fade in the whole slot
            if (slotCanvasGroup != null)
                slotCanvasGroup.alpha = eased;

            yield return null;
        }

        // Snap to final values
        if (platformRect != null)
            platformRect.anchoredPosition = targetPosition;
        if (slotCanvasGroup != null)
            slotCanvasGroup.alpha = 1f;
    }
}