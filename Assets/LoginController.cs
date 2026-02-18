using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

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
        // Setup button listeners
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

        // Hide feedback initially
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
        }

        // Wait for Firebase to initialize
        StartCoroutine(WaitForFirebase());
    }

    System.Collections.IEnumerator WaitForFirebase()
    {
        // Wait until Firebase is ready
        while (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsInitialized)
        {
            yield return null;
        }

        UnityEngine.Debug.Log("✅ Firebase ready, login system active");

        // Subscribe to auth state changes
        FirebaseManager.Instance.OnAuthStateChanged += OnAuthStateChanged;
    }

    async void OnLoginClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        // Validation
        if (string.IsNullOrEmpty(email))
        {
            ShowFeedback("Please enter your email", false);
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowFeedback("Please enter your password", false);
            return;
        }

        if (!email.Contains("@"))
        {
            ShowFeedback("Please enter a valid email", false);
            return;
        }

        // Disable buttons during login
        SetButtonsInteractable(false);
        ShowFeedback("Signing in...", true);
        if (loadingIndicator != null) loadingIndicator.SetActive(true);

        // Attempt login
        bool success = await FirebaseManager.Instance.SignInWithEmail(email, password);

        if (loadingIndicator != null) loadingIndicator.SetActive(false);

        if (success)
        {
            ShowFeedback("Welcome back! 🎉", true);
            // Navigation happens in OnAuthStateChanged
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

        // Validation
        if (string.IsNullOrEmpty(email))
        {
            ShowFeedback("Please enter your email", false);
            return;
        }

        if (!email.Contains("@"))
        {
            ShowFeedback("Please enter a valid email", false);
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowFeedback("Please enter a password", false);
            return;
        }

        if (password.Length < 6)
        {
            ShowFeedback("Password must be at least 6 characters", false);
            return;
        }

        // Disable buttons during registration
        SetButtonsInteractable(false);
        ShowFeedback("Creating your account...", true);
        if (loadingIndicator != null) loadingIndicator.SetActive(true);

        // Extract username from email (before @)
        string username = email.Split('@')[0];

        // Attempt registration
        bool success = await FirebaseManager.Instance.SignUpWithEmail(email, password, username);

        if (loadingIndicator != null) loadingIndicator.SetActive(false);

        if (success)
        {
            ShowFeedback("Account created! Welcome! 🎉", true);
            // Navigation happens in OnAuthStateChanged
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
            // User is signed in, navigate to dashboard
            UnityEngine.Debug.Log("✅ User authenticated, navigating to dashboard");

            // Small delay so user sees success message
            Invoke("NavigateToDashboard", 0.5f);
        }
    }

    void NavigateToDashboard()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene("DashboardScene");
        }
    }

    void ShowFeedback(string message, bool isSuccess)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isSuccess ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.9f, 0.3f, 0.3f);
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
        // Unsubscribe from events
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnAuthStateChanged -= OnAuthStateChanged;
        }
    }
}
