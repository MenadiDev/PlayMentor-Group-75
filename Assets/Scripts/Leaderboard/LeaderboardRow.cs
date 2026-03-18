using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using static System.Net.Mime.MediaTypeNames;

/// <summary>
/// Attached to the LeaderboardRow prefab.
/// LeaderboardManager calls Setup() on each instantiated row
/// to populate it with one player's data.
/// </summary>
public class LeaderboardRow : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // Inspector References
    // Select the LeaderboardRow PREFAB and drag
    // each child object into these fields
    // ─────────────────────────────────────────────

    [Header("Rank")]
    [SerializeField] private TMP_Text rankText;

    [Header("Avatar")]
    [SerializeField] private TMP_Text avatarInitialsText;
    [SerializeField] private Image avatarCircleImage;

    [Header("Player Info")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private GameObject youTagObject; // "YOU" label — activated only for current user

    [Header("Points")]
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private TMP_Text rankChangeText;

    [Header("Row Background")]
    [SerializeField] private Image rowBackgroundImage;
    [SerializeField] private Sprite normalRowSprite;   // row_leaderboard_normal.png
    [SerializeField] private Sprite meRowSprite;       // row_leaderboard_me.png

    // ─────────────────────────────────────────────
    // Predefined Colors
    // ─────────────────────────────────────────────
    private static readonly Color GoldColor = new Color(1.00f, 0.85f, 0.00f); // #FFD700
    private static readonly Color SilverColor = new Color(0.75f, 0.75f, 0.75f); // #C0C0C0
    private static readonly Color BronzeColor = new Color(0.80f, 0.50f, 0.20f); // #CD7F32
    private static readonly Color NormalColor = Color.white;
    private static readonly Color UpColor = new Color(0.07f, 0.73f, 0.51f); // #10B981 green
    private static readonly Color DownColor = new Color(0.94f, 0.27f, 0.27f); // #EF4444 red
    private static readonly Color NeutralColor = new Color(1f, 1f, 1f, 0.3f);    // dimmed white
    private static readonly Color MeAvatarColor = new Color(0.49f, 0.23f, 0.93f, 1f);  // #7C3AED
    private static readonly Color OtherAvatarColor = new Color(0.65f, 0.55f, 0.98f, 0.3f);

    // ─────────────────────────────────────────────
    // Public Setup — called by LeaderboardManager
    // ─────────────────────────────────────────────
    public void Setup(int rank, PlayerData data, bool isMe)
    {
        SetRank(rank);
        SetAvatar(data.name, isMe);
        SetPlayerInfo(data.name, isMe);
        SetPoints(data.points, data.rankChange);
        SetRowStyle(isMe);
    }

    // ─────────────────────────────────────────────
    // Private Helpers
    // ─────────────────────────────────────────────

    private void SetRank(int rank)
    {
        // Always display as #rank
        rankText.text = $"#{rank}";

        // Special colors for top 3
        rankText.color = rank switch
        {
            1 => GoldColor,
            2 => SilverColor,
            3 => BronzeColor,
            _ => NormalColor
        };

        // Slightly larger font for top 3
        rankText.fontSize = rank <= 3 ? 52 : 40;
    }

    private void SetAvatar(string name, bool isMe)
    {
        avatarInitialsText.text = GetInitials(name);

        if (avatarCircleImage != null)
            avatarCircleImage.color = isMe ? MeAvatarColor : OtherAvatarColor;
    }

    private void SetPlayerInfo(string name, bool isMe)
    {
        playerNameText.text = name;

        if (youTagObject != null)
            youTagObject.SetActive(isMe);
    }

    private void SetPoints(int points, int rankChange)
    {
        
        pointsText.text = points.ToString("N0");

        if (rankChange > 0)
        {
            rankChangeText.text = $"+{rankChange}";
            rankChangeText.color = UpColor;
        }
        else if (rankChange < 0)
        {
            rankChangeText.text = rankChange.ToString();
            rankChangeText.color = DownColor;
        }
        else
        {
            rankChangeText.text = "same";
            rankChangeText.color = NeutralColor;
        }
    }

    private void SetRowStyle(bool isMe)
    {
        if (rowBackgroundImage == null) return;

        if (isMe && meRowSprite != null)
            rowBackgroundImage.sprite = meRowSprite;
        else if (normalRowSprite != null)
            rowBackgroundImage.sprite = normalRowSprite;
    }

    private string GetInitials(string fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return "??";
        string[] parts = fullName.Trim().Split(' ');
        return parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
            : fullName.Substring(0, Mathf.Min(2, fullName.Length)).ToUpper();
    }
}