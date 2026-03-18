using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance;
    public static bool IsGuest = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}