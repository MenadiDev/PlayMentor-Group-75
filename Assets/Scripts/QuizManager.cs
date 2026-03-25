using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using Image = UnityEngine.UI.Image;

public class QuizManager : MonoBehaviour
{
    [Header("Question UI")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI questionNumberText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progCountText;
    [SerializeField] private TextMeshProUGUI topicChipText;

    [Header("Answer Buttons")]
    [SerializeField] private Button answerButtonA;
    [SerializeField] private Button answerButtonB;
    [SerializeField] private Button answerButtonC;
    [SerializeField] private Button answerButtonD;

    [Header("Answer Texts")]
    [SerializeField] private TextMeshProUGUI answerTextA;
    [SerializeField] private TextMeshProUGUI answerTextB;
    [SerializeField] private TextMeshProUGUI answerTextC;
    [SerializeField] private TextMeshProUGUI answerTextD;

    [Header("Answer Badges")]
    [SerializeField] private Image badgeA;
    [SerializeField] private Image badgeB;
    [SerializeField] private Image badgeC;
    [SerializeField] private Image badgeD;

    [Header("Colors")]
    [SerializeField] private Color correctColor = new Color(0.20f, 0.83f, 0.60f);
    [SerializeField] private Color wrongColor = new Color(0.97f, 0.44f, 0.44f);
    [SerializeField] private Color normalBtnColor = new Color(1f, 1f, 1f, 0.04f);
    [SerializeField] private Color dimmedBtnColor = new Color(1f, 1f, 1f, 0.03f);
    [SerializeField] private Color normalBadgeColor = new Color(1f, 1f, 1f, 0.07f);
    [SerializeField] private Color normalAnswerTextColor = new Color(1f, 1f, 1f, 0.75f);

    [Header("Results")]
    [SerializeField] private ResultsManager resultsManager;

    [Header("Lives")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private GameObject heart1;
    [SerializeField] private GameObject heart2;
    [SerializeField] private GameObject heart3;
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartEmpty;

    [Header("Hint & Skip")]
    [SerializeField] private Button hintButton;
    [SerializeField] private TextMeshProUGUI hintCountText;
    [SerializeField] private Button skipButton;
    [SerializeField] private int hintsPerQuiz = 2;

    [Header("Feedback Panel")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private Image feedbackPanelBg;
    [SerializeField] private TextMeshProUGUI feedbackTitle;
    [SerializeField] private TextMeshProUGUI feedbackDetail;
    [SerializeField] private Button feedbackContinueButton;

    [Header("Feedback Panel - Chips")]
    [SerializeField] private GameObject xpChip;
    [SerializeField] private TextMeshProUGUI xpChipText;
    [SerializeField] private GameObject streakChip;
    [SerializeField] private TextMeshProUGUI streakChipText;
    [SerializeField] private GameObject lostLifeChip;

    [Header("Feedback Panel - Colors")]
    [SerializeField] private Color feedbackCorrectColor = new Color(0.02f, 0.31f, 0.24f);
    [SerializeField] private Color feedbackWrongColor = new Color(0.27f, 0.04f, 0.04f);

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI goCorrectText;
    [SerializeField] private TextMeshProUGUI goWrongText;
    [SerializeField] private TextMeshProUGUI goXPText;

    [Header("XP Display")]
    [SerializeField] private TextMeshProUGUI headerXPText;

    private List<QuizQuestion> quizQuestions;
    private int currentQuestionIndex = 0;
    private int correctAnswers = 0;
    private int wrongAnswers = 0;
    private bool hasAnswered = false;
    private int currentLives;
    private int hintsRemaining;
    private bool hintUsedThisQuestion = false;
    private int currentStreak = 0;
    private int totalXP = 0;
    private List<Button> eliminatedButtons = new List<Button>();

   
    // Start
    void Start()
    {
        if (QuizDataManager.Instance != null)
        {
            quizQuestions = QuizDataManager.Instance.GetQuizQuestions();
            if (quizQuestions == null || quizQuestions.Count == 0)
            {
                Debug.LogError("QuizManager: No questions loaded!");
                return;
            }
            SetupQuiz();
        }
        else
        {
            Debug.LogError("QuizManager: QuizDataManager not found!");
            return;
        }

        answerButtonA.onClick.AddListener(() => OnAnswerSelected("A", answerButtonA));
        answerButtonB.onClick.AddListener(() => OnAnswerSelected("B", answerButtonB));
        answerButtonC.onClick.AddListener(() => OnAnswerSelected("C", answerButtonC));
        answerButtonD.onClick.AddListener(() => OnAnswerSelected("D", answerButtonD));

        if (hintButton != null) hintButton.onClick.AddListener(OnHintClicked);
        if (skipButton != null) skipButton.onClick.AddListener(OnSkipClicked);

        if (feedbackContinueButton != null)
            feedbackContinueButton.onClick.AddListener(OnFeedbackContinue);

        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    
    // Setup
    void SetupQuiz()
    {
        currentQuestionIndex = 0;
        correctAnswers = 0;
        wrongAnswers = 0;
        currentStreak = 0;
        totalXP = 0;
        currentLives = maxLives;
        hintsRemaining = hintsPerQuiz;

        UpdateLivesUI();
        UpdateHintUI();
        UpdateXPDisplay();
        DisplayQuestion();
    }

    
    // Display Question
    void DisplayQuestion()
    {
        if (currentQuestionIndex >= quizQuestions.Count)
        {
            EndQuiz();
            return;
        }

        hasAnswered = false;
        hintUsedThisQuestion = false;
        eliminatedButtons.Clear();

        QuizQuestion q = quizQuestions[currentQuestionIndex];
        questionText.text = q.questionText;

        if (questionNumberText != null)
            questionNumberText.text = $"Question {currentQuestionIndex + 1}/{quizQuestions.Count}";
        if (progCountText != null)
            progCountText.text = $"{currentQuestionIndex}/{quizQuestions.Count}";
        if (topicChipText != null)
            topicChipText.text = q.topic ?? "Biology";

        answerTextA.text = q.answerA;
        answerTextB.text = q.answerB;
        answerTextC.text = q.answerC;
        answerTextD.text = q.answerD;

        if (progressBar != null)
            progressBar.value = (float)currentQuestionIndex / quizQuestions.Count;

        ResetButtonStyles();
        SetButtonsInteractable(true);

        if (hintButton != null) hintButton.interactable = hintsRemaining > 0;
        if (skipButton != null) skipButton.interactable = true;
    }

    
    // Answer Selected
    void OnAnswerSelected(string answer, Button selectedButton)
    {
        if (hasAnswered) return;

        hasAnswered = true;
        QuizQuestion q = quizQuestions[currentQuestionIndex];
        bool isCorrect = q.IsCorrect(answer);

        DimAllExcept(selectedButton);

        if (isCorrect)
        {
            correctAnswers++;
            currentStreak++;
            int xpEarned = 10 + (currentStreak > 3 ? 5 : 0);
            totalXP += xpEarned;

            SetButtonCorrect(selectedButton);
            UpdateXPDisplay();

            AudioManager.Instance?.PlayCorrect();
            if (currentStreak >= 2)
                AudioManager.Instance?.PlayStreak();

            ShowFeedbackPanel(true, xpEarned, q.correctAnswer);
        }
        else
        {
            wrongAnswers++;
            currentStreak = 0;

            SetButtonWrong(selectedButton);
            HighlightCorrectAnswer(q.correctAnswer);
            LoseLife();

            AudioManager.Instance?.PlayWrong();
            ShowFeedbackPanel(false, 0, q.correctAnswer);
        }

        SetButtonsInteractable(false);
    }

    
    // Hint
    void OnHintClicked()
    {
        if (hasAnswered || hintUsedThisQuestion || hintsRemaining <= 0) return;

        hintUsedThisQuestion = true;
        hintsRemaining--;
        UpdateHintUI();

        AudioManager.Instance?.PlayButtonClick();

        List<Button> candidates = GetWrongAnswerButtons();
        if (candidates.Count == 0) return;

        Button toEliminate = candidates[Random.Range(0, candidates.Count)];
        eliminatedButtons.Add(toEliminate);

        Image img = toEliminate.GetComponent<Image>();
        if (img != null) img.color = new Color(1f, 1f, 1f, 0.015f);
        toEliminate.interactable = false;

        TextMeshProUGUI txt = toEliminate.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null) txt.color = new Color(1f, 1f, 1f, 0.12f);

        Image badge = GetBadge(toEliminate);
        if (badge != null) badge.color = new Color(1f, 1f, 1f, 0.04f);

        if (hintButton != null) hintButton.interactable = false;
    }

    
    // Skip
    void OnSkipClicked()
    {
        if (hasAnswered) return;

        hasAnswered = true;
        wrongAnswers++;
        currentStreak = 0;

        HighlightCorrectAnswer(quizQuestions[currentQuestionIndex].correctAnswer);
        DimAllExcept(null);
        LoseLife();
        SetButtonsInteractable(false);

        AudioManager.Instance?.PlayWrong();
        ShowFeedbackPanel(false, 0, quizQuestions[currentQuestionIndex].correctAnswer, skipped: true);
    }

   
    // Feedback Panel
    void ShowFeedbackPanel(bool isCorrect, int xpEarned, string correctAnswerLetter, bool skipped = false)
    {
        if (feedbackPanel == null) return;

        feedbackPanel.SetActive(true);

        if (feedbackPanelBg != null)
            feedbackPanelBg.color = isCorrect ? feedbackCorrectColor : feedbackWrongColor;

        if (feedbackTitle != null)
        {
            feedbackTitle.text = isCorrect ? GetCorrectTitle() : (skipped ? "Skipped!" : "Not quite!");
            feedbackTitle.color = isCorrect ? correctColor : wrongColor;
        }

        if (feedbackDetail != null)
            feedbackDetail.text = isCorrect
                ? "Keep it up!"
                : $"Correct answer: {GetAnswerText(correctAnswerLetter)}";

        if (xpChip != null)
        {
            xpChip.SetActive(isCorrect);
            if (xpChipText != null && isCorrect)
                xpChipText.text = $"+{xpEarned} XP";
        }

        if (streakChip != null)
        {
            bool show = isCorrect && currentStreak >= 2;
            streakChip.SetActive(show);
            if (streakChipText != null && show)
                streakChipText.text = $"Streak x{currentStreak}";
        }

        if (lostLifeChip != null)
            lostLifeChip.SetActive(!isCorrect);

        StartCoroutine(AnimatePanelIn(feedbackPanel));
    }

    IEnumerator AnimatePanelIn(GameObject panel)
    {
        RectTransform rt = panel.GetComponent<RectTransform>();
        if (rt == null) yield break;

        float startY = -rt.rect.height - 50f;
        float endY = 0f;
        float t = 0f;
        float dur = 0.25f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / dur), 3f);
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, Mathf.Lerp(startY, endY, ease));
            yield return null;
        }
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, endY);
    }

    void OnFeedbackContinue()
    {
        AudioManager.Instance?.PlayButtonClick();
        if (feedbackPanel != null) feedbackPanel.SetActive(false);

        if (currentLives <= 0)
        {
            ShowGameOver();
            return;
        }

        currentQuestionIndex++;
        DisplayQuestion();
    }

 
    // Lives
    void LoseLife()
    {
        currentLives = Mathf.Max(0, currentLives - 1);
        UpdateLivesUI();
    }

    void UpdateLivesUI()
    {
        SetHeart(heart1, currentLives >= 1);
        SetHeart(heart2, currentLives >= 2);
        SetHeart(heart3, currentLives >= 3);
    }

    void SetHeart(GameObject heart, bool alive)
    {
        if (heart == null) return;
        Image img = heart.GetComponent<Image>();
        if (img == null) return;

        if (heartFull != null && heartEmpty != null)
            img.sprite = alive ? heartFull : heartEmpty;
        else
            img.color = alive ? Color.white : new Color(1f, 1f, 1f, 0.2f);
    }

   
    // Game Over
    void ShowGameOver()
    {
        AudioManager.Instance?.PlayGameOver();
        AudioManager.Instance?.PlayDashboardMusic();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (goCorrectText != null) goCorrectText.text = correctAnswers.ToString();
            if (goWrongText != null) goWrongText.text = wrongAnswers.ToString();
            if (goXPText != null) goXPText.text = totalXP.ToString();
        }
        else
        {
            EndQuiz();
        }
    }

  
    // Hint / XP UI
    void UpdateHintUI()
    {
        if (hintCountText != null)
            hintCountText.text = $"x{hintsRemaining}";
        if (hintButton != null)
            hintButton.interactable = hintsRemaining > 0 && !hintUsedThisQuestion;
    }

    void UpdateXPDisplay()
    {
        if (headerXPText != null)
            headerXPText.text = totalXP.ToString();
    }

    
    // Button Style Helpers
    void SetButtonCorrect(Button btn)
    {
        Image img = btn.GetComponent<Image>();
        if (img != null) img.color = new Color(correctColor.r, correctColor.g, correctColor.b, 0.15f);
        Image badge = GetBadge(btn);
        if (badge != null) badge.color = correctColor;
        TextMeshProUGUI txt = GetAnswerTMP(btn);
        if (txt != null) txt.color = new Color(0.43f, 0.91f, 0.60f);
    }

    void SetButtonWrong(Button btn)
    {
        Image img = btn.GetComponent<Image>();
        if (img != null) img.color = new Color(wrongColor.r, wrongColor.g, wrongColor.b, 0.15f);
        Image badge = GetBadge(btn);
        if (badge != null) badge.color = wrongColor;
        TextMeshProUGUI txt = GetAnswerTMP(btn);
        if (txt != null) txt.color = new Color(0.99f, 0.64f, 0.64f);
    }

    void DimAllExcept(Button except)
    {
        Button[] all = { answerButtonA, answerButtonB, answerButtonC, answerButtonD };
        foreach (Button b in all)
        {
            if (b == except || eliminatedButtons.Contains(b)) continue;
            Image img = b.GetComponent<Image>();
            if (img != null) img.color = dimmedBtnColor;
        }
    }

    void HighlightCorrectAnswer(string correctAnswer)
    {
        Button btn = null;
        switch (correctAnswer.ToUpper())
        {
            case "A": btn = answerButtonA; break;
            case "B": btn = answerButtonB; break;
            case "C": btn = answerButtonC; break;
            case "D": btn = answerButtonD; break;
        }
        if (btn != null) SetButtonCorrect(btn);
    }

    void ResetButtonStyles()
    {
        Button[] all = { answerButtonA, answerButtonB, answerButtonC, answerButtonD };
        Image[] badges = { badgeA, badgeB, badgeC, badgeD };

        for (int i = 0; i < all.Length; i++)
        {
            Image img = all[i].GetComponent<Image>();
            if (img != null) img.color = normalBtnColor;
            if (badges[i] != null) badges[i].color = normalBadgeColor;
            TextMeshProUGUI txt = GetAnswerTMP(all[i]);
            if (txt != null) txt.color = normalAnswerTextColor;
            all[i].interactable = true;
        }
    }

    void SetButtonsInteractable(bool on)
    {
        answerButtonA.interactable = on;
        answerButtonB.interactable = on;
        answerButtonC.interactable = on;
        answerButtonD.interactable = on;
        if (hintButton != null) hintButton.interactable = on && hintsRemaining > 0 && !hintUsedThisQuestion;
        if (skipButton != null) skipButton.interactable = on;
    }


    // Reference Helpers
    Image GetBadge(Button btn)
    {
        if (btn == answerButtonA) return badgeA;
        if (btn == answerButtonB) return badgeB;
        if (btn == answerButtonC) return badgeC;
        if (btn == answerButtonD) return badgeD;
        return null;
    }

    TextMeshProUGUI GetAnswerTMP(Button btn)
    {
        if (btn == answerButtonA) return answerTextA;
        if (btn == answerButtonB) return answerTextB;
        if (btn == answerButtonC) return answerTextC;
        if (btn == answerButtonD) return answerTextD;
        return null;
    }

    List<Button> GetWrongAnswerButtons()
    {
        List<Button> wrong = new List<Button>();
        string correct = quizQuestions[currentQuestionIndex].correctAnswer.ToUpper();
        Button[] all = { answerButtonA, answerButtonB, answerButtonC, answerButtonD };
        string[] letters = { "A", "B", "C", "D" };

        for (int i = 0; i < all.Length; i++)
            if (letters[i] != correct && !eliminatedButtons.Contains(all[i]))
                wrong.Add(all[i]);

        return wrong;
    }

    string GetAnswerText(string letter)
    {
        QuizQuestion q = quizQuestions[currentQuestionIndex];
        switch (letter.ToUpper())
        {
            case "A": return q.answerA;
            case "B": return q.answerB;
            case "C": return q.answerC;
            case "D": return q.answerD;
        }
        return "";
    }

    string GetCorrectTitle()
    {
        string[] t = { "Brilliant!", "Nailed it!", "Amazing!", "Correct!", "Perfect!" };
        return t[Random.Range(0, t.Length)];
    }

    
    // End Quiz
    async void EndQuiz()
    {
        if (progressBar != null) progressBar.value = 1f;

        AudioManager.Instance?.PlayQuizComplete();
        AudioManager.Instance?.PlayDashboardMusic();

        float percentage = ((float)correctAnswers / quizQuestions.Count) * 100f;
        int basePoints = correctAnswers * 10;
        int bonusPoints = percentage >= 90 ? 50 : (percentage >= 80 ? 25 : 0);
        int totalPoints = basePoints + bonusPoints;

        string currentTopic = QuizDataManager.Instance != null
            ? QuizDataManager.Instance.CurrentTopic : "general";

        if (FirebaseManager.Instance != null && !SessionManager.IsGuest)
        {
            QuizResult result = new QuizResult
            {
                Topic = currentTopic,
                CorrectAnswers = correctAnswers,
                WrongAnswers = wrongAnswers,
                TotalQuestions = quizQuestions.Count,
                Percentage = percentage,
                PointsEarned = totalPoints
            };

            await FirebaseManager.Instance.SaveQuizResult(result);
            await FirebaseManager.Instance.SaveTopicProgress(result.Topic, correctAnswers, quizQuestions.Count);

            if (AdaptiveLearningEngine.Instance != null)
            {
                await AdaptiveLearningEngine.Instance.SaveStateForTopic(currentTopic);
                Debug.Log($"AdaptiveLearning: saved state for {currentTopic}");
            }


            if (AchievementManager.Instance != null)
                await AchievementManager.Instance.CheckAndUnlockAchievements(result);

            DashboardManager.RecordQuizForChallenge(percentage);

            Debug.Log($"Quiz saved! Topic: {currentTopic}, Points: {totalPoints}");
        }
        else if (SessionManager.IsGuest)
        {
            Debug.Log($"Guest mode: quiz not saved. Topic: {currentTopic}");
        }

        if (resultsManager != null)
            resultsManager.ShowResults(correctAnswers, wrongAnswers, quizQuestions.Count);
    }

    async void SaveQuizResultToFirebase(float percentage)
    {
        if (SessionManager.IsGuest) return;
        if (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsInitialized) return;
        if (FirebaseManager.Instance.CurrentUser == null) return;

        string currentTopic = QuizDataManager.Instance != null
            ? QuizDataManager.Instance.CurrentTopic : "general";

        int basePoints = correctAnswers * 10;
        int bonusPoints = percentage >= 90 ? 50 : (percentage >= 80 ? 25 : 0);
        int totalPoints = basePoints + bonusPoints;

        var quizResult = new QuizResult
        {
            Topic = currentTopic,
            CorrectAnswers = correctAnswers,
            WrongAnswers = wrongAnswers,
            TotalQuestions = quizQuestions.Count,
            Percentage = percentage,
            PointsEarned = totalPoints
        };

        bool saved = await FirebaseManager.Instance.SaveQuizResult(quizResult);
        if (saved) Debug.Log($"Quiz result saved! Topic: {currentTopic}, Points: {totalPoints}");
    }

    
    // Public button callbacks
    public void RetryQuiz()
    {
        AudioManager.Instance?.PlayButtonClick();
        AudioManager.Instance?.PlayQuizMusic();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToTopicSelect()
    {
        AudioManager.Instance?.PlayButtonClick();

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene("TopicSelectionScene");
        else
            SceneManager.LoadScene("TopicSelectionScene");
    }
}
