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
    [SerializeField] private Color lockedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color unlockedColor = Color.white;

    
    // Public Methods
    
    public void Setup(string displayName)
    {
        if (badgeNameText != null)
            badgeNameText.text = displayName;

        // Start locked by default — Firestore read will unlock if earned
        SetUnlocked(false);
    }


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
