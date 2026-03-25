using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Image = UnityEngine.UI.Image;

public class DashboardManager : MonoBehaviour
{
   
    // User Info
    [Header("User Info")]
    [SerializeField] private TextMeshProUGUI userNameText;
    [SerializeField] private TextMeshProUGUI userLevelText;
    [SerializeField] private TextMeshProUGUI greetingText;

    // Stats Cards
    [Header("Stats Cards")]
    [SerializeField] private TextMeshProUGUI levelValueText;
    [SerializeField] private TextMeshProUGUI streakValueText;
    [SerializeField] private TextMeshProUGUI badgesValueText;

   
    // Topic Progress
    [Header("Topic Progress Labels")]
    [SerializeField] private TextMeshProUGUI humanBodyPercent;
    [SerializeField] private TextMeshProUGUI cellBiologyPercent;
    [SerializeField] private TextMeshProUGUI topicName1Text;
    [SerializeField] private TextMeshProUGUI topicName2Text;

    [Header("Topic Progress Bars")]
    [SerializeField] private RectTransform humanBodyFill;
    [SerializeField] private RectTransform humanBodyTrack;
    [SerializeField] private RectTransform cellBiologyFill;
    [SerializeField] private RectTransform cellBiologyTrack;

    
    // Quote Card
    [Header("Quote Card")]
    [SerializeField] private TextMeshProUGUI quoteText;
    [SerializeField] private TextMeshProUGUI quoteAuthorText;


    // Daily Challenge
    [Header("Daily Challenge")]
    [SerializeField] private TextMeshProUGUI challengeTitleText;
    [SerializeField] private TextMeshProUGUI challengeSubText;
    [SerializeField] private TextMeshProUGUI challengeCountText;
    [SerializeField] private TextMeshProUGUI challengeXPText;
    [SerializeField] private RectTransform challengeProgressFill;
    [SerializeField] private RectTransform challengeProgressTrack;

  
    // Bottom Nav
    [Header("Bottom Nav — Buttons")]
    [SerializeField] private Button navHome;
    [SerializeField] private Button navTopics;
    [SerializeField] private Button navLeaderboard;
    [SerializeField] private Button navAchievements;
    [SerializeField] private Button navProfile;

    [Header("Bottom Nav — Active Pills")]
    [SerializeField] private GameObject navHomePill;
    [SerializeField] private GameObject navTopicsPill;
    [SerializeField] private GameObject navLeaderboardPill;
    [SerializeField] private GameObject navAchievementsPill;
    [SerializeField] private GameObject navProfilePill;

    [Header("Bottom Nav — Labels")]
    [SerializeField] private TextMeshProUGUI navHomeLabel;
    [SerializeField] private TextMeshProUGUI navTopicsLabel;
    [SerializeField] private TextMeshProUGUI navLeaderboardLabel;
    [SerializeField] private TextMeshProUGUI navAchievementsLabel;
    [SerializeField] private TextMeshProUGUI navProfileLabel;

    [Header("Bottom Nav — Colors")]
    [SerializeField] private Color navActiveColor = new Color(0.42f, 0.31f, 0.89f);
    [SerializeField] private Color navInactiveColor = new Color(0.73f, 0.73f, 0.73f);

 
    // Scene Names
    [Header("Scene Names")]
    [SerializeField] private string topicSelectionScene = "TopicSelectionScene";
    [SerializeField] private string leaderboardScene = "LeaderboardScene";
    [SerializeField] private string achievementsScene = "AchievementsScene";
    [SerializeField] private string profileScene = "ProfileScene";

    // Loading
    [Header("Loading")]
    [SerializeField] private GameObject loadingPanel;

 
    private struct ChallengeDefinition
    {
        public int type;
        public string title;
        public string sub;
        public int goal;
        public int xp;
    }

    private readonly ChallengeDefinition[] challengeDefs = new ChallengeDefinition[]
    {
        new ChallengeDefinition { type=0, title="Complete 3 quizzes today",     sub="Biology • Any Topic",  goal=3, xp=50  },
        new ChallengeDefinition { type=1, title="Score 80%+ on any quiz",       sub="Biology • Any Topic",  goal=1, xp=30  },
        new ChallengeDefinition { type=2, title="Keep your streak alive!",      sub="Play at least 1 quiz", goal=1, xp=20  },
    };


    // Quotes pool
    private string[][] quotes = new string[][]
    {
        new string[]{ "The beautiful thing about learning is nobody can take it away from you.", "B.B. King" },
        new string[]{ "Education is the most powerful weapon which you can use to change the world.", "Nelson Mandela" },
        new string[]{ "Live as if you were to die tomorrow. Learn as if you were to live forever.", "Mahatma Gandhi" },
        new string[]{ "An investment in knowledge pays the best interest.", "Benjamin Franklin" },
        new string[]{ "The more that you read, the more things you will know.", "Dr. Seuss" },
        new string[]{ "Learning is not attained by chance. It must be sought for with passion.", "Abigail Adams" },
        new string[]{ "Tell me and I forget. Teach me and I remember. Involve me and I learn.", "Benjamin Franklin" },
    };


    // Private state
    private ChallengeDefinition todayChallenge;
    private int challengeProgress = 0;
    private bool challengeComplete = false;
    private string todayDateKey;   


    // Start
    void Start()
    {
        if (FirebaseManager.Instance == null || FirebaseManager.Instance.CurrentUser == null)
        {
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene("LoginScene");
            return;
        }

        todayDateKey = DateTime.UtcNow.ToString("yyyy-MM-dd");
        todayChallenge = challengeDefs[DateTime.UtcNow.DayOfYear % challengeDefs.Length];

        SetGreeting();
        SetQuote();
        SetNavListeners();
        SetNavActive(0);

    
        RenderChallengeShell();

        LoadDashboardData();
    }

  
    // Greeting
    void SetGreeting()
    {
        if (greetingText == null) return;
        int hour = DateTime.Now.Hour;
        greetingText.text = hour < 12 ? "Good morning" : hour < 17 ? "Good afternoon" : "Good evening";
    }


    // Quote
  
    void SetQuote()
    {
        int idx = DateTime.Now.DayOfYear % quotes.Length;
        if (quoteText != null) quoteText.text = $"\"{quotes[idx][0]}\"";
        if (quoteAuthorText != null) quoteAuthorText.text = $"— {quotes[idx][1]}";
    }


    // Load dashboard data
    void LoadDashboardData()
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);

        if (FirebaseManager.Instance.CurrentUserProfile != null)
        {
            UpdateUI(FirebaseManager.Instance.CurrentUserProfile);
            LoadTopicProgress();
            LoadChallengeProgress();
            if (loadingPanel != null) loadingPanel.SetActive(false);
        }
        else
        {
            FirebaseManager.Instance.OnUserProfileLoaded += OnProfileLoaded;
        }
    }

    void OnProfileLoaded(UserProfile profile)
    {
        FirebaseManager.Instance.OnUserProfileLoaded -= OnProfileLoaded;
        UpdateUI(profile);
        LoadTopicProgress();
        LoadChallengeProgress();
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

 
    // Update UI
    void UpdateUI(UserProfile profile)
    {
        if (userNameText != null) userNameText.text = $"Welcome, {profile.Username}!";
        if (userLevelText != null) userLevelText.text = $"Level {profile.Level}";
        if (streakValueText != null) streakValueText.text = $"{profile.CurrentStreak} Days";

        StartCoroutine(AnimateCountUp(levelValueText, 0, profile.Level, 1f));
        StartCoroutine(AnimateCountUp(badgesValueText, 0, profile.TotalBadges, 1f));
    }


    // Topic Progress
    void LoadTopicProgress()
    {
        string userId = FirebaseManager.Instance.CurrentUser.UserId;
        var db = FirebaseFirestore.DefaultInstance;

        db.Collection("users").Document(userId)
            .Collection("topicProgress")
            .OrderByDescending("LastAttempt")
            .Limit(2)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompleted || task.IsFaulted)
                {
                    Debug.LogWarning("Failed to load topic progress: " + task.Exception);
                    return;
                }

                var docs = new List<DocumentSnapshot>(task.Result.Documents);

                if (docs.Count >= 1)
                {
                    string id = docs[0].Id;
                    float pct = docs[0].ContainsField("Percentage")
                        ? Convert.ToSingle(docs[0].GetValue<object>("Percentage")) : 0f;

                    if (humanBodyPercent != null) humanBodyPercent.text = $"{pct:F0}% Complete";
                    if (topicName1Text != null) topicName1Text.text = FormatTopicName(id);
                    SetProgressBar(humanBodyFill, humanBodyTrack, pct / 100f);
                }

                if (docs.Count >= 2)
                {
                    string id = docs[1].Id;
                    float pct = docs[1].ContainsField("Percentage")
                        ? Convert.ToSingle(docs[1].GetValue<object>("Percentage")) : 0f;

                    if (cellBiologyPercent != null) cellBiologyPercent.text = $"{pct:F0}% Complete";
                    if (topicName2Text != null) topicName2Text.text = FormatTopicName(id);
                    SetProgressBar(cellBiologyFill, cellBiologyTrack, pct / 100f);
                }
            });
    }

    
    void LoadChallengeProgress()
    {
        string uid = FirebaseManager.Instance.CurrentUser.UserId;

        FirebaseFirestore.DefaultInstance
            .Collection("users").Document(uid)
            .Collection("dailyChallenges").Document(todayDateKey)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogWarning("DashboardManager: challenge load failed.");
                    RenderChallengeUI(0, false);
                    return;
                }

                var doc = task.Result;

                if (!doc.Exists)
                {
                    CreateTodayChallenge(uid);
                    return;
                }

                // Check if this doc matches today's challenge type
                int savedType = doc.TryGetValue("challengeType", out long t) ? (int)t : -1;

                if (savedType != todayChallenge.type)
                {
                    // Challenge rotate
                    CreateTodayChallenge(uid);
                    return;
                }

                // Load saved progress
                challengeProgress = doc.TryGetValue("progress", out long p) ? (int)p : 0;
                challengeComplete = doc.TryGetValue("completed", out bool c) ? c : false;

                RenderChallengeUI(challengeProgress, challengeComplete);
            });
    }

    void CreateTodayChallenge(string uid)
    {
        challengeProgress = 0;
        challengeComplete = false;

        var data = new Dictionary<string, object>
        {
            { "challengeType", todayChallenge.type },
            { "progress",      0                   },
            { "completed",     false               },
            { "date",          todayDateKey        },
            { "createdAt",     Timestamp.GetCurrentTimestamp() }
        };

        FirebaseFirestore.DefaultInstance
            .Collection("users").Document(uid)
            .Collection("dailyChallenges").Document(todayDateKey)
            .SetAsync(data)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogWarning("DashboardManager: failed to create challenge doc.");

                RenderChallengeUI(0, false);
            });
    }

  
    public static void RecordQuizForChallenge(float scorePercent)
    {
        // Route through FirebaseManager 
        string uid = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid)) return;

        string dateKey = DateTime.UtcNow.ToString("yyyy-MM-dd");
        int dayType = challengeDefs_Static[DateTime.UtcNow.DayOfYear % 3].type;

        var docRef = FirebaseFirestore.DefaultInstance
            .Collection("users").Document(uid)
            .Collection("dailyChallenges").Document(dateKey);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || !task.Result.Exists) return;

            var doc = task.Result;
            int savedType = doc.TryGetValue("challengeType", out long t) ? (int)t : -1;
            if (savedType != dayType) return;

            int currentProgress = doc.TryGetValue("progress", out long p) ? (int)p : 0;
            bool alreadyDone = doc.TryGetValue("completed", out bool c) ? c : false;
            if (alreadyDone) return;

            bool qualifies = false;
            switch (dayType)
            {
                case 0: qualifies = true; break; 
                case 1: qualifies = scorePercent >= 80f; break; 
                case 2: qualifies = true; break; 
            }

            if (!qualifies) return;

            int goal = challengeDefs_Static[DateTime.UtcNow.DayOfYear % 3].goal;
            int newProgress = Mathf.Min(currentProgress + 1, goal);
            bool nowDone = newProgress >= goal;

            var updates = new Dictionary<string, object>
            {
                { "progress",  newProgress },
                { "completed", nowDone     }
            };

            docRef.UpdateAsync(updates).ContinueWithOnMainThread(t2 =>
            {
                if (t2.IsFaulted)
                    Debug.LogWarning("DashboardManager: failed to update challenge progress.");
                else
                    Debug.Log($"Challenge progress: {newProgress}/{goal}");
            });
        });
    }

    // Static copy of challenge defs for use in the static method above
    private static readonly (int type, int goal)[] challengeDefs_Static = new (int, int)[]
    {
        (0, 3),  // complete 3 quizzes
        (1, 1),  // score 80%+
        (2, 1),  // play 1 quiz (streak)
    };

  
    // Render challenge UI
    void RenderChallengeShell()
    {
        if (challengeTitleText != null) challengeTitleText.text = todayChallenge.title;
        if (challengeSubText != null) challengeSubText.text = todayChallenge.sub;
        if (challengeXPText != null) challengeXPText.text = $"+{todayChallenge.xp} XP";
        if (challengeCountText != null) challengeCountText.text = $"Loading...";
        SetProgressBar(challengeProgressFill, challengeProgressTrack, 0f);
    }

    void RenderChallengeUI(int progress, bool completed)
    {
        int goal = todayChallenge.goal;

        if (challengeTitleText != null) challengeTitleText.text = todayChallenge.title;
        if (challengeSubText != null) challengeSubText.text = todayChallenge.sub;
        if (challengeXPText != null) challengeXPText.text = $"+{todayChallenge.xp} XP";

        if (completed)
        {
            if (challengeCountText != null) challengeCountText.text = "✓ Completed!";
            SetProgressBar(challengeProgressFill, challengeProgressTrack, 1f);
        }
        else
        {
            if (challengeCountText != null)
                challengeCountText.text = goal == 1
                    ? (progress > 0 ? "1 of 1 complete" : "Not started")
                    : $"{progress} of {goal} complete";

            SetProgressBar(challengeProgressFill, challengeProgressTrack, (float)progress / goal);
        }
    }

 
    // Progress bar fill helper
    void SetProgressBar(RectTransform fill, RectTransform track, float pct)
    {
        if (fill == null || track == null) return;
        StartCoroutine(SetBarNextFrame(fill, track, pct));
    }

    IEnumerator SetBarNextFrame(RectTransform fill, RectTransform track, float pct)
    {
        yield return null;
        float width = track.rect.width * Mathf.Clamp01(pct);
        fill.sizeDelta = new Vector2(width, fill.sizeDelta.y);
    }

  
    // Animate count-up
    IEnumerator AnimateCountUp(TextMeshProUGUI text, int from, int to, float duration)
    {
        if (text == null) yield break;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            text.text = Mathf.RoundToInt(Mathf.Lerp(from, to, elapsed / duration)).ToString();
            yield return null;
        }
        text.text = to.ToString();
    }

  
    // Fallback
    void ShowDefaultData()
    {
        if (userNameText != null) userNameText.text = "Welcome, Student!";
        if (levelValueText != null) levelValueText.text = "1";
        if (streakValueText != null) streakValueText.text = "0 Days";
        if (badgesValueText != null) badgesValueText.text = "0";
        if (humanBodyPercent != null) humanBodyPercent.text = "0% Complete";
        if (cellBiologyPercent != null) cellBiologyPercent.text = "0% Complete";
        RenderChallengeUI(0, false);
    }

   
    // Bottom Nav
    void SetNavListeners()
    {
        if (navHome != null) navHome.onClick.AddListener(OnNavHome);
        if (navTopics != null) navTopics.onClick.AddListener(OnNavTopics);
        if (navLeaderboard != null) navLeaderboard.onClick.AddListener(OnNavLeaderboard);
        if (navAchievements != null) navAchievements.onClick.AddListener(OnNavAchievements);
        if (navProfile != null) navProfile.onClick.AddListener(OnNavProfile);
    }

    void OnNavHome() { SetNavActive(0); }
    void OnNavTopics() { SetNavActive(1); LoadScene(topicSelectionScene); }
    void OnNavLeaderboard() { SetNavActive(2); LoadScene(leaderboardScene); }
    void OnNavAchievements() { SetNavActive(3); LoadScene(achievementsScene); }
    void OnNavProfile() { SetNavActive(4); LoadScene(profileScene); }

    void SetNavActive(int index)
    {
        GameObject[] pills = { navHomePill, navTopicsPill, navLeaderboardPill, navAchievementsPill, navProfilePill };
        TextMeshProUGUI[] labels = { navHomeLabel, navTopicsLabel, navLeaderboardLabel, navAchievementsLabel, navProfileLabel };

        for (int i = 0; i < pills.Length; i++)
        {
            bool active = i == index;
            if (pills[i] != null) pills[i].SetActive(active);
            if (labels[i] != null) labels[i].color = active ? navActiveColor : navInactiveColor;
        }
    }

    void LoadScene(string sceneName)
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    string FormatTopicName(string id)
    {
        string[] words = id.Replace("_", " ").Split(' ');
        for (int i = 0; i < words.Length; i++)
            if (words[i].Length > 0)
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
        return string.Join(" ", words);
    }
}
