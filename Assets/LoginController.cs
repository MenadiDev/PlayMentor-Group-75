using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;

    [Header("Feedback (Optional)")]
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private GameObject loadingIndicator;

    void Start()
    {
        if (loginButton != null)
        {
            loginButton.onClick.RemoveAllListeners();
            loginButton.onClick.AddListener(() => OnLoginClicked());
        }

        if (registerButton != null)
        {
            registerButton.onClick.RemoveAllListeners();
            registerButton.onClick.AddListener(() => OnRegisterClicked());
        }

        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        if (loadingIndicator != null) loadingIndicator.SetActive(false);

        StartCoroutine(WaitForFirebase());
    }

    System.Collections.IEnumerator WaitForFirebase()
    {
        while (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsInitialized)
            yield return null;

        UnityEngine.Debug.Log("Firebase ready");

        // If the player was already logged in (session persisted by Firebase)
        // skip the login screen and go straight to their dashboard
        if (FirebaseManager.Instance.CurrentUser != null)
        {
            UnityEngine.Debug.Log("Already signed in — skipping login screen");
            NavigateToDashboard();
            yield break;
        }

        // Otherwise subscribe to auth state for when they log in manually
        FirebaseManager.Instance.OnAuthStateChanged += OnAuthStateChanged;
    }

    async void OnLoginClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email)) { ShowFeedback("Please enter your email", false); return; }
        if (!email.Contains("@")) { ShowFeedback("Please enter a valid email", false); return; }
        if (string.IsNullOrEmpty(password)) { ShowFeedback("Please enter your password", false); return; }

        SetButtonsInteractable(false);
        ShowFeedback("Signing in...", true);
        if (loadingIndicator != null) loadingIndicator.SetActive(true);

        bool success = await FirebaseManager.Instance.SignInWithEmail(email, password);

        if (loadingIndicator != null) loadingIndicator.SetActive(false);

        if (success)
        {
            ShowFeedback("Welcome back!", true);
            // NavigateToDashboard is called via OnAuthStateChanged
        }
        else
        {
            ShowFeedback("Login failed. Check your email and password.", false);
            SetButtonsInteractable(true);
        }
    }

    async void OnRegisterClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email)) { ShowFeedback("Please enter your email", false); return; }
        if (!email.Contains("@")) { ShowFeedback("Please enter a valid email", false); return; }
        if (string.IsNullOrEmpty(password)) { ShowFeedback("Please enter a password", false); return; }
        if (password.Length < 6) { ShowFeedback("Password must be at least 6 characters", false); return; }

        SetButtonsInteractable(false);
        ShowFeedback("Creating your account...", true);
        if (loadingIndicator != null) loadingIndicator.SetActive(true);

        string username = email.Split('@')[0];
        bool success = await FirebaseManager.Instance.SignUpWithEmail(email, password, username);

        if (loadingIndicator != null) loadingIndicator.SetActive(false);

        if (success)
        {
            ShowFeedback("Account created! Welcome!", true);
            // NavigateToDashboard is called via OnAuthStateChanged
        }
        else
        {
            ShowFeedback("Registration failed. Email may already be in use.", false);
            SetButtonsInteractable(true);
        }
    }

    void OnAuthStateChanged(bool isSignedIn)
    {
        if (isSignedIn)
        {
            UnityEngine.Debug.Log("User authenticated — navigating to dashboard");
            Invoke("NavigateToDashboard", 0.5f);
        }
    }

    void NavigateToDashboard()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene("DashboardScene");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("DashboardScene");
    }

    void ShowFeedback(string message, bool isSuccess)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isSuccess
                ? new Color(0.3f, 0.8f, 0.3f)
                : new Color(0.9f, 0.3f, 0.3f);
            feedbackText.gameObject.SetActive(true);
        }
        UnityEngine.Debug.Log(message);
    }

    void SetButtonsInteractable(bool interactable)
    {
        if (loginButton != null) loginButton.interactable = interactable;
        if (registerButton != null) registerButton.interactable = interactable;
        if (emailInput != null) emailInput.interactable = interactable;
        if (passwordInput != null) passwordInput.interactable = interactable;
    }

    void OnDestroy()
    {
        if (FirebaseManager.Instance != null)
            FirebaseManager.Instance.OnAuthStateChanged -= OnAuthStateChanged;
    }
}