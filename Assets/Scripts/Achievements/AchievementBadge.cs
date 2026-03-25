using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using TMPro;

public class AchievementBadge : MonoBehaviour
{
    [Header("Badge Identity")]
    [Tooltip("Must exactly match the badge ID used in Firestore e.g. 'first_win'")]
    [SerializeField] public string badgeId;

    [Header("Visuals")]
    [SerializeField] private Image badgeIcon;
    [SerializeField] private GameObject checkmarkObject;
    [SerializeField] private TMP_Text badgeNameText;

    [Header("Locked State")]
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Color lockedIconColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color unlockedIconColor = Color.white;

    // ── FIX: use dark colours for text so it's visible on light card backgrounds
    [Header("Text Colors")]
    [SerializeField] private Color unlockedTextColor = new Color(0.10f, 0.10f, 0.24f); // dark navy
    [SerializeField] private Color lockedTextColor = new Color(0.60f, 0.60f, 0.60f); // grey

    public void Setup(string displayName)
    {
        if (badgeNameText != null)
        {
            badgeNameText.text = displayName;
            // ── FIX: set text to locked colour immediately so name is visible
            badgeNameText.color = lockedTextColor;
        }

        // Start locked — Firestore read will unlock if earned
        SetUnlocked(false);
    }

    public void SetUnlocked(bool unlocked)
    {
        // Icon sprite + tint
        if (badgeIcon != null)
        {
            if (unlocked && unlockedSprite != null) badgeIcon.sprite = unlockedSprite;
            else if (!unlocked && lockedSprite != null) badgeIcon.sprite = lockedSprite;
            badgeIcon.color = unlocked ? unlockedIconColor : lockedIconColor;
        }

        // Checkmark
        if (checkmarkObject != null)
            checkmarkObject.SetActive(unlocked);

        // ── FIX: use dark visible colours instead of white-on-white
        if (badgeNameText != null)
            badgeNameText.color = unlocked ? unlockedTextColor : lockedTextColor;
    }
}