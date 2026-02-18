using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;  

public class SceneTransitionButton : MonoBehaviour
{
    [SerializeField] private string targetSceneName;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(LoadTargetScene);
        }
        else
        {
            Debug.LogError("SceneTransitionButton requires a Button component!");
        }
    }

    void LoadTargetScene()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("SceneTransitionManager.Instance is null!");
        }
    }
}