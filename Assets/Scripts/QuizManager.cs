using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Image = UnityEngine.UI.Image;

public class QuizManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI questionNumberText;
    [SerializeField] private Slider progressBar;

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

    [Header("Feedback")]
    [SerializeField] private GameObject correctFeedback;
    [SerializeField] private GameObject wrongFeedback;
    [SerializeField] private float feedbackDuration = 1f;

    [Header("Colors")]
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;
    [SerializeField] private Color normalColor = Color.white;

    [Header("Results")]
    [SerializeField] private ResultsManager resultsManager;

    // Quiz state
    private List<QuizQuestion> quizQuestions;
    private int currentQuestionIndex = 0;
    private int correctAnswers = 0;
    private int wrongAnswers = 0;
    private bool hasAnswered = false;

    void Start()
    {
        // Load quiz questions
        if (QuizDataManager.Instance != null)
        {
            quizQuestions = QuizDataManager.Instance.GetQuizQuestions();
            SetupQuiz();
        }
        else
        {
            UnityEngine.Debug.LogError("QuizDataManager not found!");
            return;
        }

        // Setup button listeners
        answerButtonA.onClick.AddListener(() => OnAnswerSelected("A", answerButtonA));
        answerButtonB.onClick.AddListener(() => OnAnswerSelected("B", answerButtonB));
        answerButtonC.onClick.AddListener(() => OnAnswerSelected("C", answerButtonC));
        answerButtonD.onClick.AddListener(() => OnAnswerSelected("D", answerButtonD));

        // Hide feedback initially
        if (correctFeedback != null) correctFeedback.SetActive(false);
        if (wrongFeedback != null) wrongFeedback.SetActive(false);
    }

    void SetupQuiz()
    {
        currentQuestionIndex = 0;
        correctAnswers = 0;
        wrongAnswers = 0;
        DisplayQuestion();
    }

    void DisplayQuestion()
    {
        if (currentQuestionIndex >= quizQuestions.Count)
        {
            EndQuiz();
            return;
        }

        hasAnswered = false;
        QuizQuestion question = quizQuestions[currentQuestionIndex];

        // Update UI
        questionText.text = question.questionText;
        questionNumberText.text = $"Question {currentQuestionIndex + 1}/{quizQuestions.Count}";

        answerTextA.text = question.answerA;
        answerTextB.text = question.answerB;
        answerTextC.text = question.answerC;
        answerTextD.text = question.answerD;

        // Update progress bar
        float progress = (float)currentQuestionIndex / quizQuestions.Count;
        progressBar.value = progress;

        // Reset button colors
        ResetButtonColors();

        // Enable buttons
        SetButtonsInteractable(true);
    }

    void OnAnswerSelected(string answer, Button selectedButton)
    {
        if (hasAnswered) return;

        hasAnswered = true;
        QuizQuestion currentQuestion = quizQuestions[currentQuestionIndex];

        bool isCorrect = currentQuestion.IsCorrect(answer);

        if (isCorrect)
        {
            correctAnswers++;
            ShowFeedback(true, selectedButton);
        }
        else
        {
            wrongAnswers++;
            ShowFeedback(false, selectedButton);
            HighlightCorrectAnswer(currentQuestion.correctAnswer);
        }

        SetButtonsInteractable(false);
        StartCoroutine(MoveToNextQuestion());
    }

    void ShowFeedback(bool isCorrect, Button selectedButton)
    {
        Image buttonImage = selectedButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = isCorrect ? correctColor : wrongColor;
        }

        if (isCorrect && correctFeedback != null)
        {
            correctFeedback.SetActive(true);
        }
        else if (!isCorrect && wrongFeedback != null)
        {
            wrongFeedback.SetActive(true);
        }
    }

    void HighlightCorrectAnswer(string correctAnswer)
    {
        Button correctButton = null;

        switch (correctAnswer.ToUpper())
        {
            case "A": correctButton = answerButtonA; break;
            case "B": correctButton = answerButtonB; break;
            case "C": correctButton = answerButtonC; break;
            case "D": correctButton = answerButtonD; break;
        }

        if (correctButton != null)
        {
            Image buttonImage = correctButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = correctColor;
            }
        }
    }

    IEnumerator MoveToNextQuestion()
    {
        yield return new WaitForSeconds(feedbackDuration);

        if (correctFeedback != null) correctFeedback.SetActive(false);
        if (wrongFeedback != null) wrongFeedback.SetActive(false);

        currentQuestionIndex++;
        DisplayQuestion();
    }

    void ResetButtonColors()
    {
        SetButtonColor(answerButtonA, normalColor);
        SetButtonColor(answerButtonB, normalColor);
        SetButtonColor(answerButtonC, normalColor);
        SetButtonColor(answerButtonD, normalColor);
    }

    void SetButtonColor(Button button, Color color)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = color;
        }
    }

    void SetButtonsInteractable(bool interactable)
    {
        answerButtonA.interactable = interactable;
        answerButtonB.interactable = interactable;
        answerButtonC.interactable = interactable;
        answerButtonD.interactable = interactable;
    }

    void EndQuiz()
    {
        // Update final progress bar
        if (progressBar != null)
        {
            progressBar.value = 1f;
        }

        UnityEngine.Debug.Log($"Quiz Complete! Correct: {correctAnswers}, Wrong: {wrongAnswers}");

        // Calculate percentage
        float percentage = ((float)correctAnswers / quizQuestions.Count) * 100f;

        // Save to Firebase
        SaveQuizResultToFirebase(percentage);

        // Show results via ResultsManager
        if (resultsManager != null)
        {
            resultsManager.ShowResults(correctAnswers, wrongAnswers, quizQuestions.Count);
        }
    }

    async void SaveQuizResultToFirebase(float percentage)
    {
        // Check if Firebase is ready and user is signed in
        if (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsInitialized)
        {
            UnityEngine.Debug.LogWarning("Firebase not ready, quiz result not saved");
            return;
        }

        if (FirebaseManager.Instance.CurrentUser == null)
        {
            UnityEngine.Debug.LogWarning("User not signed in, quiz result not saved");
            return;
        }

        // Calculate points earned (10 points per correct answer, bonus for high scores)
        int basePoints = correctAnswers * 10;
        int bonusPoints = percentage >= 90 ? 50 : (percentage >= 80 ? 25 : 0);
        int totalPoints = basePoints + bonusPoints;

        var quizResult = new QuizResult
        {
            Topic = "Biology", // You can make this dynamic later
            CorrectAnswers = correctAnswers,
            WrongAnswers = wrongAnswers,
            TotalQuestions = quizQuestions.Count,
            Percentage = percentage,
            PointsEarned = totalPoints
        };

        bool saved = await FirebaseManager.Instance.SaveQuizResult(quizResult);

        if (saved)
        {
            UnityEngine.Debug.Log($" Quiz result saved! Earned {totalPoints} points");
        }
    }

    // Called by ResultsManager Retry button
    public void RetryQuiz()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
