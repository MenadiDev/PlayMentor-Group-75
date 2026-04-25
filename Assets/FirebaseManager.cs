using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    [Header("Firebase Status")]
    public bool IsInitialized = false;

    // Firebase References
    private FirebaseAuth auth;
    private FirebaseFirestore db;

    // Current User Data
    public FirebaseUser CurrentUser { get; private set; }
    public UserProfile CurrentUserProfile { get; private set; }

    // Events
    public event Action<bool> OnAuthStateChanged;
    public event Action<UserProfile> OnUserProfileLoaded;

    void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;

                IsInitialized = true;
                UnityEngine.Debug.Log("Firebase initialized successfully!");

                
                auth.StateChanged += AuthStateChanged;
                AuthStateChanged(this, null);
            }
            else
            {
                UnityEngine.Debug.LogError($"Could not resolve Firebase dependencies: {task.Result}");
            }
        });
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != CurrentUser)
        {
            bool signedIn = auth.CurrentUser != null;

            if (!signedIn && CurrentUser != null)
            {
                UnityEngine.Debug.Log("Signed out");
                CurrentUser = null;
                CurrentUserProfile = null;
            }
            else if (signedIn && CurrentUser != auth.CurrentUser)
            {
                UnityEngine.Debug.Log($"Signed in: {auth.CurrentUser.Email}");
                CurrentUser = auth.CurrentUser;
                LoadUserProfile();
            }

            OnAuthStateChanged?.Invoke(signedIn);
        }
    }

    // AUTHENTICATION

    public async Task<bool> SignUpWithEmail(string email, string password, string username)
    {
        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            CurrentUser = result.User;

            UnityEngine.Debug.Log($"User created: {CurrentUser.Email}");

            await CreateUserProfile(CurrentUser.UserId, email, username);

            return true;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"❌ Sign up failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SignInWithEmail(string email, string password)
    {
        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            CurrentUser = result.User;

            UnityEngine.Debug.Log($"✅ User signed in: {CurrentUser.Email}");

            return true;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"❌ Sign in failed: {ex.Message}");
            return false;
        }
    }

    public void SignOut()
    {
        if (auth != null)
        {
            auth.SignOut();
            UnityEngine.Debug.Log(" User signed out");
        }
    }

    //USER PROFILE 

    async Task CreateUserProfile(string userId, string email, string username)
    {
        var userProfile = new UserProfile
        {
            UserId = userId,
            Email = email,
            Username = username,
            Level = 1,
            CurrentStreak = 0,
            TotalBadges = 0,
            TotalPoints = 0,
            CreatedAt = Timestamp.GetCurrentTimestamp(),
            LastLoginAt = Timestamp.GetCurrentTimestamp()
        };

        try
        {
            await db.Collection("users").Document(userId).SetAsync(userProfile.ToDictionary());
            CurrentUserProfile = userProfile;
            UnityEngine.Debug.Log("✅ User profile created");

            OnUserProfileLoaded?.Invoke(CurrentUserProfile);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"❌ Failed to create profile: {ex.Message}");
        }
    }

    public async void LoadUserProfile()
    {
        if (CurrentUser == null) return;

        try
        {
            var docRef = db.Collection("users").Document(CurrentUser.UserId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                CurrentUserProfile = UserProfile.FromDictionary(snapshot.ToDictionary());
                UnityEngine.Debug.Log($"✅ Profile loaded: {CurrentUserProfile.Username}");

               
                await UpdateLastLogin();

                OnUserProfileLoaded?.Invoke(CurrentUserProfile);
            }
            else
            {
                UnityEngine.Debug.LogWarning("⚠️ User profile not found, creating new one");
                await CreateUserProfile(CurrentUser.UserId, CurrentUser.Email, "Player");
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"❌ Failed to load profile: {ex.Message}");
        }
    }

    async Task UpdateLastLogin()
    {
        try
        {
            await db.Collection("users").Document(CurrentUser.UserId).UpdateAsync(
                "LastLoginAt", Timestamp.GetCurrentTimestamp()
            );
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"❌ Failed to update login time: {ex.Message}");
        }
    }

    public async Task<bool> UpdateUserProfile(Dictionary<string, object> updates)
    {
        if (CurrentUser == null) return false;

        try
        {
            await db.Collection("users").Document(CurrentUser.UserId).UpdateAsync(updates);
            LoadUserProfile(); 
            return true;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"❌ Failed to update profile: {ex.Message}");
            return false;
        }
    }

    //  QUIZ RESULTS 

    public async Task<bool> SaveQuizResult(QuizResult result)
    {
        
        if (SessionManager.IsGuest)
        {
            UnityEngine.Debug.Log("Guest mode: quiz result not saved.");
            return false;
        }

        if (CurrentUser == null)
        {
            UnityEngine.Debug.LogWarning("No user signed in, cannot save quiz result");
            return false;
        }

        try
        {
            result.UserId = CurrentUser.UserId;
            result.CompletedAt = Timestamp.GetCurrentTimestamp();

            
            await db.Collection("quiz_results").AddAsync(result.ToDictionary());

            
            await UpdateUserStats(result);

            if (AdaptiveLearningEngine.Instance != null)
            {
                string difficulty = AdaptiveLearningEngine.Instance.GetFirestoreDifficultyValue();
                int correct = AdaptiveLearningEngine.Instance.CorrectStreak;
                int wrong = AdaptiveLearningEngine.Instance.WrongStreak;

                await SaveAdaptiveState(result.Topic, difficulty, correct, wrong);
            }

            Debug.Log("Quiz result and adaptive state saved");
            return true;

        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Failed to save quiz result: {ex.Message}");
            return false;
        }
    }

    async Task UpdateUserStats(QuizResult result)
    {
        
        if (SessionManager.IsGuest) return;

        if (CurrentUserProfile == null) return;

        
        int newPoints = CurrentUserProfile.TotalPoints + result.PointsEarned;
        int newLevel = (newPoints / 1000) + 1;

        
        int currentStreak = CurrentUserProfile.CurrentStreak;
        DateTime today = DateTime.UtcNow.Date;

        
        try
        {
            var snap = await db.Collection("users").Document(CurrentUser.UserId).GetSnapshotAsync();
            if (snap.Exists && snap.ContainsField("LastQuizDate"))
            {
                Timestamp lastTs = snap.GetValue<Timestamp>("LastQuizDate");
                DateTime lastDate = lastTs.ToDateTime().Date;

                if (lastDate == today)
                {
                    
                }
                else if (lastDate == today.AddDays(-1))
                {
                   
                    currentStreak++;
                }
                else
                {
                   
                    currentStreak = 1;
                }
            }
            else
            {
                
                currentStreak = 1;
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"❌ Streak calc error: {ex.Message}");
            currentStreak = 1;
        }

        var updates = new Dictionary<string, object>
        {
            { "TotalPoints", newPoints },
            { "CurrentStreak", currentStreak },
            { "LastQuizDate", Timestamp.GetCurrentTimestamp() }
        };

        if (newLevel > CurrentUserProfile.Level)
        {
            updates["Level"] = newLevel;
            UnityEngine.Debug.Log($"🎉 Level up! Now level {newLevel}");
        }

        await UpdateUserProfile(updates);
    }

    public async Task SaveTopicProgress(string topicId, int correct, int total)
    {
        
        if (SessionManager.IsGuest)
        {
            UnityEngine.Debug.Log("Guest mode: topic progress not saved.");
            return;
        }

        if (CurrentUser == null) return;

        try
        {
            float percentage = ((float)correct / total) * 100f;

            var topicRef = db.Collection("users")
                .Document(CurrentUser.UserId)
                .Collection("topicProgress")
                .Document(topicId);

            var snap = await topicRef.GetSnapshotAsync();
            float bestScore = 0f;

            if (snap.Exists && snap.ContainsField("Percentage"))
                bestScore = Convert.ToSingle(snap.GetValue<double>("Percentage"));

            // Only save if it's the user's best score for this topic
            if (percentage > bestScore)
            {
                await topicRef.SetAsync(new Dictionary<string, object>
                {
                    { "Percentage", percentage },
                    { "LastAttempt", Timestamp.GetCurrentTimestamp() }
                });
                UnityEngine.Debug.Log($"✅ Topic progress saved: {topicId} = {percentage}%");
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"❌ Topic progress error: {ex.Message}");
        }
    }

    public async Task<List<QuizResult>> GetUserQuizHistory(int limit = 10)
    {
        
        if (SessionManager.IsGuest)
        {
            UnityEngine.Debug.Log("Guest mode: no quiz history available.");
            return new List<QuizResult>();
        }

        if (CurrentUser == null) return new List<QuizResult>();

        try
        {
            var query = db.Collection("quiz_results")
                .WhereEqualTo("UserId", CurrentUser.UserId)
                .OrderByDescending("CompletedAt")
                .Limit(limit);

            var snapshot = await query.GetSnapshotAsync();

            var results = new List<QuizResult>();
            foreach (var doc in snapshot.Documents)
            {
                results.Add(QuizResult.FromDictionary(doc.ToDictionary()));
            }

            return results;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"❌ Failed to get quiz history: {ex.Message}");
            return new List<QuizResult>();
        }
    }

    // ADAPTIVE LEARNING STATE

    public async Task<Dictionary<string, object>> LoadAdaptiveState(string topicId)
    {
        if (SessionManager.IsGuest || CurrentUser == null)
        {
            Debug.Log("Guest mode or no user: cannot load adaptive state");
            return null;
        }

        try
        {
            var docRef = db.Collection("users")
                .Document(CurrentUser.UserId)
                .Collection("adaptiveState")
                .Document(topicId);

            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                Debug.Log($"No adaptive state found for topic: {topicId}");
                return null;
            }

            var data = snapshot.ToDictionary();
            Debug.Log($"✅ Loaded adaptive state for {topicId}: difficulty={data["currentDifficulty"]}");
            return data;
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to load adaptive state: {ex.Message}");
            return null;
        }
    }

    
    public async Task<bool> SaveAdaptiveState(string topicId, string difficulty, int correctStreak, int wrongStreak)
    {
        if (SessionManager.IsGuest || CurrentUser == null)
        {
            Debug.Log("Guest mode or no user: cannot save adaptive state");
            return false;
        }

        try
        {
            var docRef = db.Collection("users")
                .Document(CurrentUser.UserId)
                .Collection("adaptiveState")
                .Document(topicId);

            var data = new Dictionary<string, object>
        {
            { "currentDifficulty", difficulty },
            { "correctStreak", correctStreak },
            { "wrongStreak", wrongStreak },
            { "lastUpdated", FieldValue.ServerTimestamp }
        };

            await docRef.SetAsync(data);
            Debug.Log($"✅ Saved adaptive state for {topicId}: difficulty={difficulty}, streak={correctStreak}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to save adaptive state: {ex.Message}");
            return false;
        }
    }

    
    public async Task<List<string>> GetAdaptiveTopics()
    {
        if (SessionManager.IsGuest || CurrentUser == null)
            return new List<string>();

        try
        {
            var snapshot = await db.Collection("users")
                .Document(CurrentUser.UserId)
                .Collection("adaptiveState")
                .GetSnapshotAsync();

            var topics = new List<string>();
            foreach (var doc in snapshot.Documents)
            {
                topics.Add(doc.Id);
            }
            return topics;
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to get adaptive topics: {ex.Message}");
            return new List<string>();
        }
    }

    
    /// adaptive state helper
    public async Task<bool> HasAdaptiveState(string topicId)
    {
        if (SessionManager.IsGuest || CurrentUser == null)
            return false;

        try
        {
            var docRef = db.Collection("users")
                .Document(CurrentUser.UserId)
                .Collection("adaptiveState")
                .Document(topicId);

            var snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists;
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to check adaptive state: {ex.Message}");
            return false;
        }
    }

    void OnDestroy()
    {
        if (auth != null)
        {
            auth.StateChanged -= AuthStateChanged;
        }
    }
}

// DATA MODELS 

[Serializable]
public class UserProfile
{
    public string UserId;
    public string Email;
    public string Username;
    public int Level;
    public int CurrentStreak;
    public int TotalBadges;
    public int TotalPoints;
    public Timestamp CreatedAt;
    public Timestamp LastLoginAt;
    public int TopicsMastered; 
    public float AverageDifficulty; 

    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { "UserId", UserId },
            { "Email", Email },
            { "Username", Username },
            { "Level", Level },
            { "CurrentStreak", CurrentStreak },
            { "TotalBadges", TotalBadges },
            { "TotalPoints", TotalPoints },
            { "CreatedAt", CreatedAt },
            { "LastLoginAt", LastLoginAt },
            { "TopicsMastered", TopicsMastered },
            { "AverageDifficulty", AverageDifficulty }
        };
    }

    public static UserProfile FromDictionary(IDictionary<string, object> dict)
    {
        return new UserProfile
        {
            UserId = dict.ContainsKey("UserId") ? dict["UserId"].ToString() : "",
            Email = dict.ContainsKey("Email") ? dict["Email"].ToString() : "",
            Username = dict.ContainsKey("Username") ? dict["Username"].ToString() : "",
            Level = dict.ContainsKey("Level") ? Convert.ToInt32(dict["Level"]) : 1,
            CurrentStreak = dict.ContainsKey("CurrentStreak") ? Convert.ToInt32(dict["CurrentStreak"]) : 0,
            TotalBadges = dict.ContainsKey("TotalBadges") ? Convert.ToInt32(dict["TotalBadges"]) : 0,
            TotalPoints = dict.ContainsKey("TotalPoints") ? Convert.ToInt32(dict["TotalPoints"]) : 0,
            CreatedAt = dict.ContainsKey("CreatedAt") ? (Timestamp)dict["CreatedAt"] : Timestamp.GetCurrentTimestamp(),
            LastLoginAt = dict.ContainsKey("LastLoginAt") ? (Timestamp)dict["LastLoginAt"] : Timestamp.GetCurrentTimestamp()
        };
    }
}

[Serializable]
public class QuizResult
{
    public string UserId;
    public string Topic;
    public int CorrectAnswers;
    public int WrongAnswers;
    public int TotalQuestions;
    public float Percentage;
    public int PointsEarned;
    public Timestamp CompletedAt;

    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { "UserId", UserId },
            { "Topic", Topic },
            { "CorrectAnswers", CorrectAnswers },
            { "WrongAnswers", WrongAnswers },
            { "TotalQuestions", TotalQuestions },
            { "Percentage", Percentage },
            { "PointsEarned", PointsEarned },
            { "CompletedAt", CompletedAt }
        };
    }

    public static QuizResult FromDictionary(IDictionary<string, object> dict)
    {
        return new QuizResult
        {
            UserId = dict.ContainsKey("UserId") ? dict["UserId"].ToString() : "",
            Topic = dict.ContainsKey("Topic") ? dict["Topic"].ToString() : "",
            CorrectAnswers = dict.ContainsKey("CorrectAnswers") ? Convert.ToInt32(dict["CorrectAnswers"]) : 0,
            WrongAnswers = dict.ContainsKey("WrongAnswers") ? Convert.ToInt32(dict["WrongAnswers"]) : 0,
            TotalQuestions = dict.ContainsKey("TotalQuestions") ? Convert.ToInt32(dict["TotalQuestions"]) : 0,
            Percentage = dict.ContainsKey("Percentage") ? Convert.ToSingle(dict["Percentage"]) : 0f,
            PointsEarned = dict.ContainsKey("PointsEarned") ? Convert.ToInt32(dict["PointsEarned"]) : 0,
            CompletedAt = dict.ContainsKey("CompletedAt") ? (Timestamp)dict["CompletedAt"] : Timestamp.GetCurrentTimestamp()
        };
    }
}