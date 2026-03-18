using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Persistent singleton — lives on a DontDestroyOnLoad GameObject.
/// Only handles unlock logic and Firestore reads/writes.
/// Has NO scene-specific Inspector fields — zero stale reference risk.
/// Place on a standalone "AchievementManager" GameObject in your first scene (MainMenu/Login).
/// </summary>
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    // ── Internal state ────────────────────────────
    private FirebaseFirestore db;
    private string currentUserId;
    private bool isInitialized = false;

    // Cached unlocked badge ids — populated on first load, updated on unlock
    private HashSet<string> unlockedIds = new HashSet<string>();

    // ── Badge definitions (data only — no scene refs) ─
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

    // ─────────────────────────────────────────────
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

    // ─────────────────────────────────────────────
    // Initialise Firebase + load cached unlock state
    // ─────────────────────────────────────────────
    public async Task InitializeAsync()
    {
        if (isInitialized) return;

        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("AchievementManager: no user logged in.");
            return;
        }

        currentUserId = user.UserId;
        db = FirebaseFirestore.DefaultInstance;

        await RefreshCacheFromFirestore();

        isInitialized = true;
        Debug.Log($"AchievementManager ready — {unlockedIds.Count} badges unlocked.");
    }

    // ─────────────────────────────────────────────
    // Pull latest unlock state from Firestore into cache
    // ─────────────────────────────────────────────
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
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"AchievementManager: cache refresh failed — {ex.Message}");
        }
    }

    // ─────────────────────────────────────────────
    // Public query — used by AchievementsSceneUI
    // ─────────────────────────────────────────────
    public bool IsBadgeUnlocked(string badgeId) => unlockedIds.Contains(badgeId);

    // ─────────────────────────────────────────────
    // Check & unlock after a quiz
    // ─────────────────────────────────────────────
    public async Task CheckAndUnlockAchievements(QuizResult result)
    {
        if (!isInitialized) await InitializeAsync();

        await TryUnlock("first_win");                                                   // complete any quiz

        if (result.Percentage >= 100f)
            await TryUnlock("100_score");                                               // perfect score

        if (result.Percentage >= 80f)
            await TryUnlock("quick_learn");                                             // score 80%+

        if (result.CorrectAnswers >= 5)
            await TryUnlock("bookworm");                                                // 5+ correct

        if (result.Percentage >= 100f && result.CorrectAnswers == result.TotalQuestions)
            await TryUnlock("mystery_badge");                                           // full marks
    }

    // ─────────────────────────────────────────────
    // Public manual unlock (e.g. for week_star from streak logic)
    // ─────────────────────────────────────────────
    public async void UnlockBadge(string badgeId)
    {
        if (!isInitialized) await InitializeAsync();
        await TryUnlock(badgeId);
    }

    // ─────────────────────────────────────────────
    // Internal unlock
    // ─────────────────────────────────────────────
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
