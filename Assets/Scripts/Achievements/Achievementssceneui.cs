using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class AchievementsSceneUI : MonoBehaviour
{
    [Header("Badge Cards (order: FirstWin, QuickLearn, WeekStar, 100Score, Bookworm, Mystery)")]
    [SerializeField] private AchievementBadge badgeFirstWin;
    [SerializeField] private AchievementBadge badgeQuickLearn;
    [SerializeField] private AchievementBadge badgeWeekStar;
    [SerializeField] private AchievementBadge badge100Score;
    [SerializeField] private AchievementBadge badgeBookworm;
    [SerializeField] private AchievementBadge badgeMystery;

    [Header("Total Points")]
    [SerializeField] private TMP_Text totalPointsText;

    [Header("Loading")]
    [SerializeField] private GameObject loadingSpinner;

    [Header("Navigation")]
    [SerializeField] private Button backButton;

    async void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(GoBack);

        SetLoading(true);

        // ── FIX: wait for AchievementManager AND for Firebase Auth to have a user
        // This solves the blank-on-first-visit problem
        float timeout = 5f;
        float waited = 0f;

        while (waited < timeout)
        {
            // Check AchievementManager exists and Firebase has a logged-in user
            if (AchievementManager.Instance != null &&
                Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser != null)
                break;

            await Task.Delay(100);
            waited += 0.1f;
        }

        if (AchievementManager.Instance == null)
        {
            Debug.LogError("AchievementsSceneUI: AchievementManager not found!");
            SetLoading(false);
            return;
        }

        if (Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            Debug.LogWarning("AchievementsSceneUI: no user logged in after waiting.");
            SetLoading(false);
            return;
        }

        // ── FIX: always re-initialise to pick up the current user
        // This ensures the cache is loaded for the logged-in user on every visit
        await AchievementManager.Instance.InitializeAsync();

        // Fresh pull from Firestore so newly earned badges show immediately
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
            Debug.LogWarning($"AchievementsSceneUI: badge slot '{badgeId}' not wired.");
            return;
        }

        var info = AchievementManager.AllBadges.Find(b => b.badgeId == badgeId);
        string displayName = info != null ? info.displayName : badgeId;

        badge.Setup(displayName);
        badge.SetUnlocked(mgr.IsBadgeUnlocked(badgeId));
    }

    async Task LoadTotalPoints()
    {
        if (totalPointsText == null) return;

        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) return;

        try
        {
            var doc = await Firebase.Firestore.FirebaseFirestore.DefaultInstance
                .Collection("users").Document(user.UserId)
                .GetSnapshotAsync();

            totalPointsText.text = doc.Exists && doc.TryGetValue("TotalPoints", out long pts)
                ? pts.ToString("N0")
                : "0";
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