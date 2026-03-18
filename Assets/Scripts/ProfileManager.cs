using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Image = UnityEngine.UI.Image;

public class ProfileManager : MonoBehaviour
{
    // ── Avatar ───────────────────────────────────────
    [Header("Avatar")]
    [SerializeField] private TMP_Text avatarInitialText;
    [SerializeField] private Image avatarCircleImage;

    // ── Name Block ───────────────────────────────────
    [Header("Name Block")]
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text emailText;
    [SerializeField] private TMP_Text levelXPText;

    // ── Stat Cards ───────────────────────────────────
    [Header("Stat Cards")]
    [SerializeField] private TMP_Text levelValueText;
    [SerializeField] private TMP_Text badgesValueText;
    [SerializeField] private TMP_Text streakValueText;

    // ── Quiz Stats Grid ───────────────────────────────
    [Header("Quiz Stats")]
    [SerializeField] private TMP_Text totalQuizzesText;
    [SerializeField] private TMP_Text avgScoreText;
    [SerializeField] private TMP_Text totalCorrectText;
    [SerializeField] private TMP_Text perfectQuizzesText;

    // ── Navigation ───────────────────────────────────
    [Header("Navigation")]
    [SerializeField] private Button backButton;

    // ── Loading ───────────────────────────────────────
    [Header("Loading")]
    [SerializeField] private GameObject loadingOverlay;

    private readonly Color[] levelColors =
    {
        new Color(0.97f, 0.62f, 0.07f),  // amber  — levels 1–3
        new Color(0.42f, 0.30f, 0.89f),  // purple — levels 4–6
        new Color(0.06f, 0.72f, 0.51f),  // green  — levels 7–9
        new Color(0.22f, 0.52f, 0.95f),  // blue   — levels 10–12
        new Color(0.89f, 0.30f, 0.42f),  // pink   — level 13+
    };

    void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(GoToDashboard);

        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogError("ProfileManager: no logged-in user.");
            return;
        }

        // Email comes straight from Firebase Auth — no Firestore read needed
        if (emailText != null)
            emailText.text = user.Email ?? "";

        if (loadingOverlay != null) loadingOverlay.SetActive(true);

        LoadProfileData(user.UserId);
    }

    // ─────────────────────────────────────────────────
    // Step 1 — users/{uid} document
    // ─────────────────────────────────────────────────
    void LoadProfileData(string uid)
    {
        FirebaseFirestore.DefaultInstance
            .Collection("users").Document(uid)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || !task.Result.Exists)
                {
                    Debug.LogWarning("ProfileManager: user doc not found.");
                    if (loadingOverlay != null) loadingOverlay.SetActive(false);
                    return;
                }

                var doc = task.Result;

                string username = doc.TryGetValue("Username", out string u) ? u : "Player";
                int level = doc.TryGetValue("Level", out long l) ? (int)l : 1;
                int xp = doc.TryGetValue("TotalPoints", out long p) ? (int)p : 0;
                int badges = doc.TryGetValue("TotalBadges", out long b) ? (int)b : 0;
                int streak = doc.TryGetValue("CurrentStreak", out long s) ? (int)s : 0;

                // Avatar
                if (avatarInitialText != null)
                    avatarInitialText.text = username.Length > 0
                        ? username[0].ToString().ToUpper() : "?";

                if (avatarCircleImage != null)
                {
                    int colorIndex = Mathf.Clamp((level - 1) / 3, 0, levelColors.Length - 1);
                    avatarCircleImage.color = levelColors[colorIndex];
                }

                // Name block
                if (usernameText != null) usernameText.text = username;
                if (levelXPText != null) levelXPText.text = $"Level {level} , {xp} XP";

                // Stat cards
                if (levelValueText != null) levelValueText.text = level.ToString();
                if (badgesValueText != null) badgesValueText.text = badges.ToString();
                if (streakValueText != null) streakValueText.text = streak.ToString();

                // Step 2 — quiz stats
                LoadQuizStats(uid);
            });
    }

    // ─────────────────────────────────────────────────
    // Step 2 — quiz_results root collection (matches FirebaseManager.SaveQuizResult)
    // Filtered by UserId field since results are stored at root, not subcollection
    // ─────────────────────────────────────────────────
    void LoadQuizStats(string uid)
    {
        FirebaseFirestore.DefaultInstance
            .Collection("quiz_results")
            .WhereEqualTo("UserId", uid)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (loadingOverlay != null) loadingOverlay.SetActive(false);

                if (task.IsFaulted)
                {
                    Debug.LogWarning($"ProfileManager: quiz_results query failed — {task.Exception?.Message}");
                    SetQuizStatsZero();
                    return;
                }

                int totalQuizzes = task.Result.Count;
                int totalCorrect = 0;
                int perfectQuizzes = 0;
                float scoreSum = 0f;

                foreach (var doc in task.Result.Documents)
                {
                    // FirebaseManager saves "Percentage" (0–100) and "CorrectAnswers"
                    float pct = doc.TryGetValue("Percentage", out double p) ? (float)p : 0f;
                    int correct = doc.TryGetValue("CorrectAnswers", out long c) ? (int)c : 0;

                    scoreSum += pct;
                    totalCorrect += correct;
                    if (pct >= 100f) perfectQuizzes++;
                }

                float avgScore = totalQuizzes > 0 ? scoreSum / totalQuizzes : 0f;

                if (totalQuizzesText != null) totalQuizzesText.text = totalQuizzes.ToString();
                if (avgScoreText != null) avgScoreText.text = Mathf.RoundToInt(avgScore) + "%";
                if (totalCorrectText != null) totalCorrectText.text = totalCorrect.ToString();
                if (perfectQuizzesText != null) perfectQuizzesText.text = perfectQuizzes.ToString();
            });
    }

    void SetQuizStatsZero()
    {
        if (totalQuizzesText != null) totalQuizzesText.text = "0";
        if (avgScoreText != null) avgScoreText.text = "0%";
        if (totalCorrectText != null) totalCorrectText.text = "0";
        if (perfectQuizzesText != null) perfectQuizzesText.text = "0";
    }

    void GoToDashboard()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene("DashboardScene");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("DashboardScene");
    }
}