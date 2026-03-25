using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    private FirebaseFirestore db;
    private string currentUserId;
    private bool isInitialized = false;

    private HashSet<string> unlockedIds = new HashSet<string>();

    public class BadgeInfo
    {
        public string badgeId;
        public string displayName;
    }

    public static readonly List<BadgeInfo> AllBadges = new List<BadgeInfo>
    {
        new BadgeInfo { badgeId = "first_win",     displayName = "First Win"     },
        new BadgeInfo { badgeId = "quick_learn",   displayName = "Quick Learn"   },
        new BadgeInfo { badgeId = "week_star",     displayName = "Week Star"     },
        new BadgeInfo { badgeId = "100_score",     displayName = "100% Score"    },
        new BadgeInfo { badgeId = "bookworm",      displayName = "Bookworm"      },
        new BadgeInfo { badgeId = "mystery_badge", displayName = "Mystery Badge" },
    };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void Start()
    {
        await InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("AchievementManager: no user logged in.");
            return;
        }

        // ── FIX: if user changed (logout/login) re-initialise for new user
        if (isInitialized && currentUserId == user.UserId) return;

        currentUserId = user.UserId;
        db = FirebaseFirestore.DefaultInstance;

        await RefreshCacheFromFirestore();

        isInitialized = true;
        Debug.Log($"AchievementManager ready — {unlockedIds.Count} badges unlocked.");
    }

    public async Task RefreshCacheFromFirestore()
    {
        if (string.IsNullOrEmpty(currentUserId)) return;

        try
        {
            var snapshot = await db
                .Collection("users").Document(currentUserId)
                .Collection("achievements")
                .GetSnapshotAsync();

            unlockedIds.Clear();
            foreach (var doc in snapshot.Documents)
            {
                bool unlocked = true;
                if (doc.TryGetValue("unlocked", out bool val)) unlocked = val;
                if (unlocked) unlockedIds.Add(doc.Id);
            }

            Debug.Log($"AchievementManager: cache refreshed — {unlockedIds.Count} unlocked.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"AchievementManager: cache refresh failed — {ex.Message}");
        }
    }

    public bool IsBadgeUnlocked(string badgeId) => unlockedIds.Contains(badgeId);

    public async Task CheckAndUnlockAchievements(QuizResult result)
    {
        if (!isInitialized) await InitializeAsync();

        await TryUnlock("first_win");

        if (result.Percentage >= 100f)
            await TryUnlock("100_score");

        if (result.Percentage >= 80f)
            await TryUnlock("quick_learn");

        if (result.CorrectAnswers >= 5)
            await TryUnlock("bookworm");

        if (result.Percentage >= 100f && result.CorrectAnswers == result.TotalQuestions)
            await TryUnlock("mystery_badge");
    }

    public async void UnlockBadge(string badgeId)
    {
        if (!isInitialized) await InitializeAsync();
        await TryUnlock(badgeId);
    }

    private async Task TryUnlock(string badgeId)
    {
        if (unlockedIds.Contains(badgeId)) return;
        if (string.IsNullOrEmpty(currentUserId)) return;

        try
        {
            await db
                .Collection("users").Document(currentUserId)
                .Collection("achievements").Document(badgeId)
                .SetAsync(new Dictionary<string, object>
                {
                    { "unlocked",   true },
                    { "unlockedAt", FieldValue.ServerTimestamp }
                });

            unlockedIds.Add(badgeId);
            Debug.Log($"AchievementManager: unlocked '{badgeId}'");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"AchievementManager: unlock '{badgeId}' failed — {ex.Message}");
        }
    }
}