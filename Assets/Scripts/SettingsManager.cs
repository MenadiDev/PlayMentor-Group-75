using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Debug = UnityEngine.Debug;


public class SettingsManager : MonoBehaviour
{
    [Header("Logout")]
    [SerializeField] private Button logoutButton;
    [SerializeField] private GameObject logoutPopup;
    [SerializeField] private Button logoutConfirmButton;
    [SerializeField] private Button logoutCancelButton;

    void Start()
    {
        // Hide popup on start
        if (logoutPopup != null) logoutPopup.SetActive(false);

        // Wire buttons
        if (logoutButton != null) logoutButton.onClick.AddListener(ShowPopup);
        if (logoutCancelButton != null) logoutCancelButton.onClick.AddListener(HidePopup);
        if (logoutConfirmButton != null) logoutConfirmButton.onClick.AddListener(ConfirmLogout);
    }

    void ShowPopup()
    {
        AudioManager.Instance?.PlayButtonClick();
        if (logoutPopup != null) logoutPopup.SetActive(true);
    }

    void HidePopup()
    {
        AudioManager.Instance?.PlayButtonClick();
        if (logoutPopup != null) logoutPopup.SetActive(false);
    }

    void ConfirmLogout()
    {
        AudioManager.Instance?.PlayButtonClick();

        // Sign out from Firebase
        FirebaseAuth.DefaultInstance.SignOut();

        // Stop music
        AudioManager.Instance?.StopMusic();

        // Go to login
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene("MainMenuScene");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
    }
}
