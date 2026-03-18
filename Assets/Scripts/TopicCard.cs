using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Image = UnityEngine.UI.Image;

/// <summary>
/// Attach to each topic card GameObject in the Topic Selection scene.
/// TopicSelectionManager calls Setup() on each card.
/// </summary>
public class TopicCard : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // Inspector References
    // ─────────────────────────────────────────────

    [Header("Card Data")]
    public string topicId;          // e.g. "chemical_basis_of_life"
    public string topicDisplayName; // e.g. "Chemical Basis of Life"
    public int grade;            // 10 or 11

    [Header("UI References")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image topAccentBar;   // 3px bar at top of card
    [SerializeField] private Image iconBackground; // coloured square behind icon
    [SerializeField] private Image topicIcon;      // the PNG icon sprite
    [SerializeField] private TMP_Text topicNameText;
    [SerializeField] private GameObject selectedOverlay; // highlight shown when selected

    [Header("Grade Colors")]
    [SerializeField] private Color grade10AccentA = new Color(0.49f, 0.23f, 0.93f); // #7C3AED
    [SerializeField] private Color grade10AccentB = new Color(0.15f, 0.39f, 0.92f); // #2563EB
    [SerializeField] private Color grade11AccentA = new Color(0.02f, 0.59f, 0.41f); // #059669
    [SerializeField] private Color grade11AccentB = new Color(0.03f, 0.57f, 0.69f); // #0891b2

    // ─────────────────────────────────────────────
    // Private State
    // ─────────────────────────────────────────────
    private bool isSelected = false;
    private TopicSelectionManager manager;

    // ─────────────────────────────────────────────
    // Setup — called by TopicSelectionManager
    // ─────────────────────────────────────────────
    public void Setup(string id, string displayName, int gradeLevel,
                      Sprite icon, TopicSelectionManager mgr)
    {
        topicId = id;
        topicDisplayName = displayName;
        grade = gradeLevel;
        manager = mgr;

        // Set text
        if (topicNameText != null) topicNameText.text = displayName;

        // Set icon sprite
        if (topicIcon != null && icon != null)
            topicIcon.sprite = icon;

        // Apply grade colour theme
        ApplyGradeColors(gradeLevel);

        // Hide selected overlay by default
        if (selectedOverlay != null)
            selectedOverlay.SetActive(false);

        // Wire button click
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnCardClicked);
    }

    // ─────────────────────────────────────────────
    // Selection State
    // ─────────────────────────────────────────────
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selectedOverlay != null)
            selectedOverlay.SetActive(selected);

        // Scale up slightly when selected
        transform.localScale = selected
            ? new Vector3(1.04f, 1.04f, 1f)
            : Vector3.one;
    }

    // ─────────────────────────────────────────────
    // Click Handler
    // ─────────────────────────────────────────────
    private void OnCardClicked()
    {
        if (manager != null)
            manager.OnTopicSelected(this);
    }

    // ─────────────────────────────────────────────
    // Apply Grade Color Theme
    // ─────────────────────────────────────────────
    private void ApplyGradeColors(int gradeLevel)
    {
        Color accentA = gradeLevel == 10 ? grade10AccentA : grade11AccentA;
        Color accentB = gradeLevel == 10 ? grade10AccentB : grade11AccentB;

        // Top accent bar — solid accent color
        if (topAccentBar != null)
            topAccentBar.color = accentA;

        // Icon background — very light tint of accent
        if (iconBackground != null)
        {
            Color iconBg = accentA;
            iconBg.a = 0.12f;
            iconBackground.color = iconBg;
        }

        
    }
}