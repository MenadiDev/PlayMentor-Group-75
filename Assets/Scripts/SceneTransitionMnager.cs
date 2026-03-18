
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.3f;

    void Awake()
    {
        // Singleton pattern - only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Fade in when scene starts
        if (fadeCanvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    
    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionToScene(sceneName));
    }

    // Load scene by index
    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(TransitionToScene(sceneIndex));
    }

    // Transition with fade effect - by name
    IEnumerator TransitionToScene(string sceneName)
    {
        // Fade out
        yield return StartCoroutine(FadeOut());

        // Load new scene
        SceneManager.LoadScene(sceneName);

        // Fade in
        yield return StartCoroutine(FadeIn());
    }

    // Transition with fade effect - by index
    IEnumerator TransitionToScene(int sceneIndex)
    {
        // Fade out
        yield return StartCoroutine(FadeOut());

        // Load new scene
        SceneManager.LoadScene(sceneIndex);

        // Fade in
        yield return StartCoroutine(FadeIn());
    }

    // Fade to black
    IEnumerator FadeOut()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            fadeCanvasGroup.alpha = alpha;
            yield return null;
        }

        fadeCanvasGroup.alpha = 1;
    }

    // Fade from black
    IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            fadeCanvasGroup.alpha = alpha;
            yield return null;
        }

        fadeCanvasGroup.alpha = 0;
    }
}
