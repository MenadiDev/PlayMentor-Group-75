using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class AdaptiveLearningEngine : MonoBehaviour
{
    public static AdaptiveLearningEngine Instance { get; private set; }

    public enum Difficulty { Easy, Medium, Hard }

    [Header("Decision Tree Thresholds")]
    [Tooltip("Correct answers in a row needed to increase difficulty")]
    [SerializeField] private int correctStreakToUpgrade = 3;
    [Tooltip("Wrong answers in a row needed to decrease difficulty")]
    [SerializeField] private int wrongStreakToDowngrade = 2;

  
    private Difficulty currentDifficulty = Difficulty.Medium;
    private int correctStreak = 0;
    private int wrongStreak = 0;
    private string currentTopicId = "";

    
    private FirebaseFirestore db;
    private string uid;

   
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            db = FirebaseFirestore.DefaultInstance;
            uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId ?? "";
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
    // Load saved state for a topic before quiz starts
    
    public async Task LoadStateForTopic(string topicId)
    {
        currentTopicId = topicId;

        // Guests always start on medium 
        if (SessionManager.IsGuest || string.IsNullOrEmpty(uid))
        {
            ResetToMedium();
            return;
        }

        try
        {
            var snap = await db
                .Collection("users").Document(uid)
                .Collection("adaptiveState").Document(topicId)
                .GetSnapshotAsync();

            if (!snap.Exists)
            {
                ResetToMedium();
                Debug.Log($"AdaptiveLearning: no saved state for {topicId}, starting Medium.");
                return;
            }

            string saved = snap.TryGetValue("currentDifficulty", out string d) ? d : "medium";
            currentDifficulty = ParseDifficulty(saved);
            correctStreak = snap.TryGetValue("correctStreak", out long cs) ? (int)cs : 0;
            wrongStreak = snap.TryGetValue("wrongStreak", out long ws) ? (int)ws : 0;

            Debug.Log($"AdaptiveLearning: loaded {topicId} → {currentDifficulty} " +
                      $"(correct streak {correctStreak}, wrong streak {wrongStreak})");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"AdaptiveLearning: load failed — {ex.Message}. Defaulting to Medium.");
            ResetToMedium();
        }
    }

    
    // Record one answer and runs the decision tree
    
    public void RecordAnswer(bool wasCorrect)
    {
        if (wasCorrect)
        {
            correctStreak++;
            wrongStreak = 0;   // reset wrong streak on correct

            if (correctStreak >= correctStreakToUpgrade)
            {
                TryUpgrade();
                correctStreak = 0; // reset after shift
            }
        }
        else
        {
            wrongStreak++;
            correctStreak = 0; // reset correct streak on wrong

            if (wrongStreak >= wrongStreakToDowngrade)
            {
                TryDowngrade();
                wrongStreak = 0; // reset after shift
            }
        }

        Debug.Log($"AdaptiveLearning: difficulty={currentDifficulty} " +
                  $"correctStreak={correctStreak} wrongStreak={wrongStreak}");
    }

 
    // Save state after quiz ends
    public async Task SaveStateForTopic(string topicId)
    {
        if (SessionManager.IsGuest || string.IsNullOrEmpty(uid)) return;

        try
        {
            await db
                .Collection("users").Document(uid)
                .Collection("adaptiveState").Document(topicId)
                .SetAsync(new Dictionary<string, object>
                {
                    { "currentDifficulty", DifficultyToString(currentDifficulty) },
                    { "correctStreak",     correctStreak                          },
                    { "wrongStreak",       wrongStreak                            },
                    { "lastUpdated",       FieldValue.ServerTimestamp             }
                });

            Debug.Log($"AdaptiveLearning: saved state for {topicId} → {currentDifficulty}");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"AdaptiveLearning: save failed — {ex.Message}");
        }
    }

    
    // Public getters
    public Difficulty GetCurrentDifficulty() => currentDifficulty;
    public string GetCurrentDifficultyString() => DifficultyToString(currentDifficulty);

    /// Returning the Firestore difficulty filter value for the current level
    public string GetFirestoreDifficultyValue() => DifficultyToString(currentDifficulty);

    // Decision tree helpers
    void TryUpgrade()
    {
        if (currentDifficulty == Difficulty.Hard) return;
        var before = currentDifficulty;
        currentDifficulty = currentDifficulty == Difficulty.Easy
            ? Difficulty.Medium : Difficulty.Hard;
        Debug.Log($"AdaptiveLearning: ↑ upgraded {before} → {currentDifficulty}");
    }

    void TryDowngrade()
    {
        if (currentDifficulty == Difficulty.Easy) return;
        var before = currentDifficulty;
        currentDifficulty = currentDifficulty == Difficulty.Hard
            ? Difficulty.Medium : Difficulty.Easy;
        Debug.Log($"AdaptiveLearning: ↓ downgraded {before} → {currentDifficulty}");
    }

    void ResetToMedium()
    {
        currentDifficulty = Difficulty.Medium;
        correctStreak = 0;
        wrongStreak = 0;
    }

    
    // String Difficulty converters
    public static Difficulty ParseDifficulty(string s)
    {
        switch (s?.ToLower())
        {
            case "easy": return Difficulty.Easy;
            case "hard": return Difficulty.Hard;
            default: return Difficulty.Medium;
        }
    }

    public static string DifficultyToString(Difficulty d)
    {
        switch (d)
        {
            case Difficulty.Easy: return "easy";
            case Difficulty.Hard: return "hard";
            default: return "medium";
        }
    }
}
