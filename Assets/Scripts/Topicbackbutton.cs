using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class TopicBackButton : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string loggedInScene = "DashboardScene";
    [SerializeField] private string guestScene = "MainMenuScene";

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        AudioManager.Instance?.PlayButtonClick();

        string target = SessionManager.IsGuest ? guestScene : loggedInScene;

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(target);
        else
            SceneManager.LoadScene(target);
    }
}