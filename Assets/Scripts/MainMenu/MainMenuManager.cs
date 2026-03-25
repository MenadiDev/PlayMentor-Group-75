using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    // Drag this to BtnPlay OnClick()
    public void OnPlayGuest()
    {
        SessionManager.IsGuest = true;
        SceneManager.LoadScene("TopicSelectionScene");
    }

    // Drag this to BtnLogin OnClick()
    public void OnLogin()
    {
        SessionManager.IsGuest = false;
        SceneManager.LoadScene("LoginScene");
    }

    // Drag this to BtnRegister OnClick()
    public void OnRegister()
    {
        SessionManager.IsGuest = false;
        SceneManager.LoadScene("LoginScene");
    }
}

