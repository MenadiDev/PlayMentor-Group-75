using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    // BtnPlay OnClick()
    public void OnPlayGuest()
    {
        SessionManager.IsGuest = true;
        SceneManager.LoadScene("TopicSelectionScene");
    }

    // BtnLogin OnClick()
    public void OnLogin()
    {
        SessionManager.IsGuest = false;
        SceneManager.LoadScene("LoginScene");
    }

    // BtnRegister OnClick()
    public void OnRegister()
    {
        SessionManager.IsGuest = false;
        SceneManager.LoadScene("LoginScene");
    }
}

