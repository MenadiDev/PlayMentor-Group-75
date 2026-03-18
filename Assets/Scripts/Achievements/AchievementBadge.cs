using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using TMPro;

/// <summary>
/// Attach this to each badge card GameObject in the Achievements scene.
/// AchievementManager calls Setup() and SetUnlocked() on each one.
/// </summary>
public class AchievementBadge : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // Inspector References
    // Drag the child objects of each badge card into these fields
    // ─────────────────────────────────────────────

    [Header("Badge Identity")]
    [Tooltip("Must exactly match the badge ID used in Firestore e.g. 'first_win'")]
    [SerializeField] public string badgeId;

    [Header("Visuals")]
    [SerializeField] private Image badgeIcon;         // the badge image/sprite
    [SerializeField] private GameObject checkmarkObject; // the tick GameObject
    [SerializeField] private TMP_Text badgeNameText;   // the label below the icon

    [Header("Locked State")]
    [SerializeField] private Sprite unlockedSprite;  // normal coloured sprite
    [SerializeField] private Sprite lockedSprite;    // greyscale/lock sprite
    [SerializeField] private Color lockedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color unlockedColor = Color.white;

    // ─────────────────────────────────────────────
    // Public Methods
    // ─────────────────────────────────────────────

    /// <summary>
    /// Called by AchievementManager on Start to set the display name.
    /// </summary>
    public void Setup(string displayName)
    {
        if (badgeNameText != null)
            badgeNameText.text = displayName;

        // Start locked by default — Firestore read will unlock if earned
        SetUnlocked(false);
    }

    /// <summary>
    /// Called by AchievementManager after reading Firestore.
    /// Pass true if the player has earned this badge, false if not.
    /// </summary>
    public void SetUnlocked(bool unlocked)
    {
        // Swap sprite
        if (badgeIcon != null)
        {
            badgeIcon.sprite = unlocked ? unlockedSprite : lockedSprite;
            badgeIcon.color = unlocked ? unlockedColor : lockedColor;
        }

        // Show or hide the checkmark
        if (checkmarkObject != null)
            checkmarkObject.SetActive(unlocked);

        // Dim the badge name if locked
        if (badgeNameText != null)
            badgeNameText.color = unlocked
                ? new Color(1f, 1f, 1f, 1f)
                : new Color(1f, 1f, 1f, 0.4f);
    }
}