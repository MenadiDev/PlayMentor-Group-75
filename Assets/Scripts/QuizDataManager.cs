using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using Firebase.Auth;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class QuizDataManager : MonoBehaviour
{
    public static QuizDataManager Instance;

    [Header("Quiz Settings")]
    public int questionsPerQuiz = 10;

    public string CurrentTopic { get; private set; } = "";

    private List<QuizQuestion> currentQuizQuestions = new List<QuizQuestion>();
    private bool isLoading = false;

  
    // Singleton
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

   
    // Load questions — adaptive for logged-in users,
    // random pick for guests (no difficulty filter)
    
    public async Task<bool> LoadQuestionsForTopic(string topicId)
    {
        if (isLoading)
        {
            Debug.LogWarning("QuizDataManager: already loading, please wait.");
            return false;
        }

        isLoading = true;
        CurrentTopic = topicId;
        currentQuizQuestions.Clear();

        var auth = FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser == null && !SessionManager.IsGuest)
        {
            Debug.LogWarning("QuizDataManager: no user logged in and not a guest.");
            isLoading = false;
            return false;
        }

        //  GUESTS: skip adaptive, load all questions and pick randomly 
        if (SessionManager.IsGuest)
        {
            Debug.Log($"QuizDataManager: guest — loading all questions for '{topicId}'");
            return await LoadQuestionsRandom(topicId);
        }

        // LOGGED-IN: use adaptive difficulty 
        if (AdaptiveLearningEngine.Instance != null)
            await AdaptiveLearningEngine.Instance.LoadStateForTopic(topicId);

        string difficulty = AdaptiveLearningEngine.Instance != null
            ? AdaptiveLearningEngine.Instance.GetFirestoreDifficultyValue()
            : "medium";

        Debug.Log($"QuizDataManager: loading '{topicId}' at difficulty '{difficulty}'");

        return await LoadQuestionsAdaptive(topicId, difficulty);
    }

    
    // Adaptive load — tries target difficulty first,
    // fills remaining slots from adjacent difficulties
    private async Task<bool> LoadQuestionsAdaptive(string topicId, string difficulty)
    {
        try
        {
            // Primary fetch at target difficulty
            var primary = await FetchByDifficulty(topicId, difficulty);
            Shuffle(primary);

            foreach (var q in primary)
            {
                if (currentQuizQuestions.Count >= questionsPerQuiz) break;
                currentQuizQuestions.Add(q);
            }

            // Fill from adjacent difficulties if needed
            if (currentQuizQuestions.Count < questionsPerQuiz)
            {
                foreach (string fillDiff in GetFillOrder(difficulty))
                {
                    if (currentQuizQuestions.Count >= questionsPerQuiz) break;

                    var fill = await FetchByDifficulty(topicId, fillDiff);
                    Shuffle(fill);

                    foreach (var q in fill)
                    {
                        if (currentQuizQuestions.Count >= questionsPerQuiz) break;
                        if (!currentQuizQuestions.Contains(q))
                            currentQuizQuestions.Add(q);
                    }
                }
            }

            // Final shuffle so fill questions don't cluster at the end
            Shuffle(currentQuizQuestions);

            Debug.Log($"QuizDataManager: ready with {currentQuizQuestions.Count} questions " +
                      $"(difficulty: {difficulty})");

            isLoading = false;
            return currentQuizQuestions.Count > 0;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"QuizDataManager: adaptive load failed — {e.Message}");
            isLoading = false;
            return false;
        }
    }

   
    // Random load for guests 
    private async Task<bool> LoadQuestionsRandom(string topicId)
    {
        try
        {
            var db = FirebaseFirestore.DefaultInstance;
            var snapshot = await db.Collection("questions")
                                   .WhereEqualTo("topic", topicId)
                                   .GetSnapshotAsync();

            var fetched = new List<QuizQuestion>();
            foreach (var doc in snapshot.Documents)
            {
                var q = ParseDoc(doc);
                if (q != null) fetched.Add(q);
            }

            if (fetched.Count == 0)
            {
                Debug.LogError($"QuizDataManager: no questions found for topic '{topicId}'.");
                isLoading = false;
                return false;
            }

            currentQuizQuestions = PickRandom(fetched, questionsPerQuiz);

            Debug.Log($"QuizDataManager: guest — ready with {currentQuizQuestions.Count} questions.");

            isLoading = false;
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"QuizDataManager: guest load failed — {e.Message}");
            isLoading = false;
            return false;
        }
    }

    
    // Fetch all questions for a topic + difficulty
   
    private async Task<List<QuizQuestion>> FetchByDifficulty(string topicId, string difficulty)
    {
        var result = new List<QuizQuestion>();
        var db = FirebaseFirestore.DefaultInstance;
        var snapshot = await db.Collection("questions")
                               .WhereEqualTo("topic", topicId)
                               .WhereEqualTo("difficulty", difficulty)
                               .GetSnapshotAsync();

        foreach (var doc in snapshot.Documents)
        {
            var q = ParseDoc(doc);
            if (q != null) result.Add(q);
        }

        return result;
    }

  
    // Parse a Firestore document into QuizQuestion

    private QuizQuestion ParseDoc(DocumentSnapshot doc)
    {
        try
        {
            return new QuizQuestion
            {
                questionText = doc.ContainsField("questionText") ? doc.GetValue<string>("questionText") : "",
                answerA = doc.ContainsField("answerA") ? doc.GetValue<string>("answerA") : "",
                answerB = doc.ContainsField("answerB") ? doc.GetValue<string>("answerB") : "",
                answerC = doc.ContainsField("answerC") ? doc.GetValue<string>("answerC") : "",
                answerD = doc.ContainsField("answerD") ? doc.GetValue<string>("answerD") : "",
                correctAnswer = doc.ContainsField("correctAnswer") ? doc.GetValue<string>("correctAnswer") : "",
                topic = doc.ContainsField("topicName") ? doc.GetValue<string>("topicName") : "",
                difficulty = doc.ContainsField("difficulty") ? doc.GetValue<string>("difficulty") : "medium",
            };
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"QuizDataManager: skipping malformed doc {doc.Id} — {e.Message}");
            return null;
        }
    }

   
    // Fill order for adjacent difficulties
   
    private List<string> GetFillOrder(string primary)
    {
        switch (primary)
        {
            case "easy": return new List<string> { "medium", "hard" };
            case "hard": return new List<string> { "medium", "easy" };
            default: return new List<string> { "easy", "hard" };
        }
    }

    
    // Called by QuizManager
    public List<QuizQuestion> GetQuizQuestions()
    {
        if (currentQuizQuestions.Count == 0)
            Debug.LogError("QuizDataManager: no questions loaded! " +
                           "Ensure LoadQuestionsForTopic() completed before loading QuizScene.");
        return currentQuizQuestions;
    }

    
    // Fisher-Yates shuffle
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

   
    // Pick N random items without replacement 
    private List<QuizQuestion> PickRandom(List<QuizQuestion> source, int count)
    {
        var pool = new List<QuizQuestion>(source);
        var result = new List<QuizQuestion>();
        int take = Mathf.Min(count, pool.Count);

        for (int i = 0; i < take; i++)
        {
            int idx = Random.Range(0, pool.Count);
            result.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        return result;
    }
}
