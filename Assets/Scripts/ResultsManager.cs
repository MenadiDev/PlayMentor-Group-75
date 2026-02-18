using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Image = UnityEngine.UI.Image;

public class ResultsManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private Image trophyIcon;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI percentageText;

    [Header("Progress Ring")]
    [SerializeField] private Image progressFill;

    [Header("Stars")]
    [SerializeField] private Image star1;
    [SerializeField] private Image star2;
    [SerializeField] private Image star3;

    [Header("Message")]
    [SerializeField] private TextMeshProUGUI messageTitle;
    [SerializeField] private TextMeshProUGUI messageDescription;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI correctValue;
    [SerializeField] private TextMeshProUGUI wrongValue;

    [Header("Buttons")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button dashboardButton;

    [Header("Trophy Icons (Optional)")]
    [SerializeField] private Sprite trophyGold;
    [SerializeField] private Sprite trophySilver;
    [SerializeField] private Sprite trophyBronze;

    [Header("Star Sprites (Optional)")]
    [SerializeField] private Sprite starFilled;
    [SerializeField] private Sprite starEmpty;

    [Header("Colors")]
    [SerializeField] private Color perfectColor = new Color(1f, 0.84f, 0f); // Gold
    [SerializeField] private Color excellentColor = new Color(0.13f, 0.59f, 0.95f); // Blue
    [SerializeField] private Color goodColor = new Color(0.3f, 0.69f, 0.31f); // Green
    [SerializeField] private Color needsWorkColor = new Color(1f, 0.34f, 0.13f); // Orange

    [Header("Animation Settings")]
    [SerializeField] private float progressAnimationDuration = 1.5f;
    [SerializeField] private float starAnimationDelay = 0.2f;

    private QuizManager quizManager;

    void Start()
    {
        // Get reference to QuizManager
        quizManager = FindObjectOfType<QuizManager>();

        // Setup button listeners
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(RetryQuiz);
        }

        if (dashboardButton != null)
        {
            dashboardButton.onClick.AddListener(GoToDashboard);
        }

        // Hide results panel initially
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }
    }

    public void ShowResults(int correct, int wrong, int total)
    {
        // Calculate percentage
        float percentage = ((float)correct / total) * 100f;

        // Show panel
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
        }

        // Start animations
        StartCoroutine(AnimateResults(correct, wrong, total, percentage));
    }

    IEnumerator AnimateResults(int correct, int wrong, int total, float percentage)
    {
        // Reset progress fill to 0
        if (progressFill != null)
        {
            progressFill.fillAmount = 0f;
        }

        // Update trophy and title based on score
        UpdateTrophyAndTitle(percentage);

        // Small delay before starting animations
        yield return new WaitForSeconds(0.3f);

        // Animate progress ring
        yield return StartCoroutine(AnimateProgressFill(percentage / 100f));

        // Update score text with animation
        yield return StartCoroutine(AnimateScore(correct, total));

        // Show percentage
        if (percentageText != null)
        {
            percentageText.text = $"{percentage:F0}%";
        }

        // Animate stars
        yield return StartCoroutine(AnimateStars(percentage));

        // Update message based on performance
        UpdateMessage(percentage);

        // Update stats
        if (correctValue != null) correctValue.text = correct.ToString();
        if (wrongValue != null) wrongValue.text = wrong.ToString();
    }

    void UpdateTrophyAndTitle(float percentage)
    {
        if (percentage >= 90)
        {
            if (trophyIcon != null && trophyGold != null) trophyIcon.sprite = trophyGold;
            if (titleText != null)
            {
                titleText.text = "Perfect!";
                titleText.color = perfectColor;
            }
        }
        else if (percentage >= 80)
        {
            if (trophyIcon != null && trophyGold != null) trophyIcon.sprite = trophyGold;
            if (titleText != null)
            {
                titleText.text = "Excellent Work!";
                titleText.color = excellentColor;
            }
        }
        else if (percentage >= 70)
        {
            if (trophyIcon != null && trophySilver != null) trophyIcon.sprite = trophySilver;
            if (titleText != null)
            {
                titleText.text = "Great Job!";
                titleText.color = goodColor;
            }
        }
        else if (percentage >= 60)
        {
            if (trophyIcon != null && trophySilver != null) trophyIcon.sprite = trophySilver;
            if (titleText != null)
            {
                titleText.text = "Good Effort!";
                titleText.color = goodColor;
            }
        }
        else
        {
            if (trophyIcon != null && trophyBronze != null) trophyIcon.sprite = trophyBronze;
            if (titleText != null)
            {
                titleText.text = "Keep Trying! ";
                titleText.color = needsWorkColor;
            }
        }
    }

    IEnumerator AnimateProgressFill(float targetFill)
    {
        if (progressFill == null) yield break;

        float elapsed = 0f;
        float startFill = 0f;

        while (elapsed < progressAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / progressAnimationDuration;

            // Smooth easing
            t = t * t * (3f - 2f * t); // Smoothstep

            progressFill.fillAmount = Mathf.Lerp(startFill, targetFill, t);
            yield return null;
        }

        progressFill.fillAmount = targetFill;
    }

    IEnumerator AnimateScore(int finalCorrect, int total)
    {
        if (scoreText == null) yield break;

        int currentScore = 0;
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentScore = Mathf.RoundToInt(Mathf.Lerp(0, finalCorrect, elapsed / duration));
            scoreText.text = $"{currentScore}/{total}";
            yield return null;
        }

        scoreText.text = $"{finalCorrect}/{total}";
    }

    IEnumerator AnimateStars(float percentage)
    {
        // Determine how many stars to show
        int starCount = 0;
        if (percentage >= 90) starCount = 3;
        else if (percentage >= 70) starCount = 2;
        else if (percentage >= 50) starCount = 1;

        Image[] stars = { star1, star2, star3 };

        // Hide all stars initially
        foreach (var star in stars)
        {
            if (star != null)
            {
                star.transform.localScale = Vector3.zero;
                if (starEmpty != null) star.sprite = starEmpty;
            }
        }

        // Animate filled stars popping in
        for (int i = 0; i < starCount; i++)
        {
            if (stars[i] != null)
            {
                if (starFilled != null) stars[i].sprite = starFilled;

                // Pop animation
                float duration = 0.3f;
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float scale = Mathf.Lerp(0f, 1.2f, elapsed / duration);
                    stars[i].transform.localScale = Vector3.one * scale;
                    yield return null;
                }

                // Bounce back
                elapsed = 0f;
                duration = 0.15f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float scale = Mathf.Lerp(1.2f, 1f, elapsed / duration);
                    stars[i].transform.localScale = Vector3.one * scale;
                    yield return null;
                }

                yield return new WaitForSeconds(starAnimationDelay);
            }
        }

        // Show empty stars for remaining slots
        for (int i = starCount; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                stars[i].transform.localScale = Vector3.one;
            }
        }
    }

    void UpdateMessage(float percentage)
    {
        if (messageTitle == null || messageDescription == null) return;

        if (percentage >= 90)
        {
            messageTitle.text = "Outstanding!";
            messageDescription.text = "You mastered this topic!";
        }
        else if (percentage >= 80)
        {
            messageTitle.text = "Excellent!";
            messageDescription.text = "You're doing awesome! Keep it up!";
        }
        else if (percentage >= 70)
        {
            messageTitle.text = "Well Done!";
            messageDescription.text = "You're making great progress!";
        }
        else if (percentage >= 60)
        {
            messageTitle.text = "Good Try!";
            messageDescription.text = "Review the material and try again!";
        }
        else
        {
            messageTitle.text = "Keep Learning!";
            messageDescription.text = "Practice makes perfect!";
        }
    }

    void RetryQuiz()
    {
        // Hide results panel
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }

        // Tell QuizManager to restart
        if (quizManager != null)
        {
            quizManager.RetryQuiz();
        }
    }

    void GoToDashboard()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene("DashboardScene");
        }
    }
}