
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class AchievementsSceneUI : MonoBehaviour
{
    
    [Header("Badge Cards (in order: FirstWin, QuickLearn, WeekStar, 100Score, Bookworm, Mystery)")]
    [SerializeField] private AchievementBadge badgeFirstWin;
    [SerializeField] private AchievementBadge badgeQuickLearn;
    [SerializeField] private AchievementBadge badgeWeekStar;
    [SerializeField] private AchievementBadge badge100Score;
    [SerializeField] private AchievementBadge badgeBookworm;
    [SerializeField] private AchievementBadge badgeMystery;

    // ── Total Points card 
    [Header("Total Points")]
    [SerializeField] private TMP_Text totalPointsText;

    // ── Loading spinner
    [Header("Loading")]
    [SerializeField] private GameObject loadingSpinner;

    // ── Back button
    [Header("Navigation")]
    [SerializeField] private UnityEngine.UI.Button backButton;

    async void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(GoBack);

        // Show loading state while we wait for data
        SetLoading(true);

        // Make sure AchievementManager is ready
        if (AchievementManager.Instance == null)
        {
            Debug.LogError("AchievementsSceneUI: AchievementManager not found! " +
                           "Make sure it exists in your first scene.");
            SetLoading(false);
            return;
        }

        
        await AchievementManager.Instance.RefreshCacheFromFirestore();

        RenderBadges();
        await LoadTotalPoints();

        SetLoading(false);
    }

    void RenderBadges()
    {
        var mgr = AchievementManager.Instance;

        SetupBadge(badgeFirstWin, "first_win", mgr);
        SetupBadge(badgeQuickLearn, "quick_learn", mgr);
        SetupBadge(badgeWeekStar, "week_star", mgr);
        SetupBadge(badge100Score, "100_score", mgr);
        SetupBadge(badgeBookworm, "bookworm", mgr);
        SetupBadge(badgeMystery, "mystery_badge", mgr);
    }

    void SetupBadge(AchievementBadge badge, string badgeId, AchievementManager mgr)
    {
        if (badge == null)
        {
            Debug.LogWarning($"AchievementsSceneUI: badge slot for '{badgeId}' is not wired.");
            return;
        }

        // Find display name from the static list
        var info = AchievementManager.AllBadges.Find(b => b.badgeId == badgeId);
        string displayName = info != null ? info.displayName : badgeId;

        badge.Setup(displayName);
        badge.SetUnlocked(mgr.IsBadgeUnlocked(badgeId));
    }

    // ─────────────────────────────────────────────
    // Load total points directly from Firestore
    // ─────────────────────────────────────────────
    async System.Threading.Tasks.Task LoadTotalPoints()
    {
        if (totalPointsText == null) return;

        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) return;

        try
        {
            var doc = await Firebase.Firestore.FirebaseFirestore.DefaultInstance
                .Collection("users").Document(user.UserId)
                .GetSnapshotAsync();

            if (doc.Exists && doc.TryGetValue("TotalPoints", out long pts))
                totalPointsText.text = pts.ToString("N0");
            else
                totalPointsText.text = "0";
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"AchievementsSceneUI: failed to load points — {ex.Message}");
            totalPointsText.text = "0";
        }
    }


    void SetLoading(bool on)
    {
        if (loadingSpinner != null) loadingSpinner.SetActive(on);
    }

  
    void GoBack()
    {
        AudioManager.Instance?.PlayButtonClick();

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene("DashboardScene");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("DashboardScene");
    }
}