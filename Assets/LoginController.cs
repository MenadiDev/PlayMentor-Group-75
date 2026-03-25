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

    [Header("Feedback Text (optional — shows below inputs)")]
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private GameObject loadingIndicator;

    [Header("Error Popup (optional — shows on wrong password)")]
    [SerializeField] private GameObject errorPopup;
    [SerializeField] private TMP_Text errorMessageText;
    [SerializeField] private Button errorOKButton;

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

        
        if (errorOKButton != null)
            errorOKButton.onClick.AddListener(HideErrorPopup);

        
        if (errorPopup != null) errorPopup.SetActive(false);

        StartCoroutine(WaitForFirebase());
    }

    System.Collections.IEnumerator WaitForFirebase()
    {
        while (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsInitialized)
            yield return null;

        UnityEngine.Debug.Log("Firebase ready");

        if (FirebaseManager.Instance.CurrentUser != null)
        {
            UnityEngine.Debug.Log("Already signed in — skipping login screen");
            NavigateToDashboard();
            yield break;
        }

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
        }
        else
        {
            //Show error popup on wrong credentials
            ShowErrorPopup("Wrong email or password.\nPlease try again.");
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
        }
        else
        {
            // Show error popup on failed registration 
            ShowErrorPopup("Registration failed.\nThis email may already be in use.");
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

    
    // Error Popup
    void ShowErrorPopup(string message)
    {
        if (errorPopup != null)
        {
            if (errorMessageText != null) errorMessageText.text = message;
            errorPopup.SetActive(true);
        }
        else
        {
            // Fallback to inline feedback text 
            ShowFeedback(message.Replace("\n", " "), false);
        }
    }

    void HideErrorPopup()
    {
        if (errorPopup != null) errorPopup.SetActive(false);
    }

    
    // Inline feedback
    
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