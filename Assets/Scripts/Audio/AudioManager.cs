using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Debug = UnityEngine.Debug;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip bgmDashboard;
    [SerializeField] private AudioClip bgmQuiz;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip sfxButtonClick;
    [SerializeField] private AudioClip sfxCorrect;
    [SerializeField] private AudioClip sfxWrong;
    [SerializeField] private AudioClip sfxQuizComplete;
    [SerializeField] private AudioClip sfxStreak;
    [SerializeField] private AudioClip sfxGameOver;     // ← NEW: failure sting

    [Header("Volume")]
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.4f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 0.8f;

    [Header("Crossfade")]
    [SerializeField] private float crossfadeDuration = 1.2f;

    private AudioClip currentBGM;
    private Coroutine crossfadeCoroutine;

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
            return;
        }
    }

    void Start()
    {
        ApplySavedSettings();
        PlayMusic(bgmDashboard);
    }

    // ─── Music ───────────────────────────────────────
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) { StopMusic(); return; }
        if (currentBGM == clip && musicSource.isPlaying) return;

        currentBGM = clip;

        if (crossfadeCoroutine != null) StopCoroutine(crossfadeCoroutine);

        if (musicSource.isPlaying)
            crossfadeCoroutine = StartCoroutine(CrossfadeTo(clip));
        else
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.volume = IsMusicEnabled() ? musicVolume : 0f;
            musicSource.Play();
        }
    }

    public void PlayDashboardMusic() => PlayMusic(bgmDashboard);
    public void PlayQuizMusic() => PlayMusic(bgmQuiz);

    public void StopMusic()
    {
        if (crossfadeCoroutine != null) StopCoroutine(crossfadeCoroutine);
        StartCoroutine(FadeOutMusic());
    }

    public void PauseMusic() { if (musicSource.isPlaying) musicSource.Pause(); }
    public void ResumeMusic() { if (!musicSource.isPlaying) musicSource.UnPause(); }

    // ─── SFX ─────────────────────────────────────────
    public void PlayButtonClick() => PlaySFX(sfxButtonClick);
    public void PlayCorrect() => PlaySFX(sfxCorrect);
    public void PlayWrong() => PlaySFX(sfxWrong);
    public void PlayQuizComplete() => PlaySFX(sfxQuizComplete);
    public void PlayStreak() => PlaySFX(sfxStreak);
    public void PlayGameOver() => PlaySFX(sfxGameOver);   // ← NEW

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || !IsSFXEnabled()) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // ─── Volume / Settings ───────────────────────────
    public void SetMusicEnabled(bool enabled)
    {
        musicSource.volume = enabled ? musicVolume : 0f;
        PlayerPrefs.SetInt("Music_Enabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetSFXEnabled(bool enabled)
    {
        PlayerPrefs.SetInt("SFX_Enabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (IsMusicEnabled()) musicSource.volume = musicVolume;
        PlayerPrefs.SetFloat("Music_Volume", musicVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFX_Volume", sfxVolume);
        PlayerPrefs.Save();
    }

    void ApplySavedSettings()
    {
        if (PlayerPrefs.HasKey("Music_Volume")) musicVolume = PlayerPrefs.GetFloat("Music_Volume");
        if (PlayerPrefs.HasKey("SFX_Volume")) sfxVolume = PlayerPrefs.GetFloat("SFX_Volume");
        musicSource.volume = IsMusicEnabled() ? musicVolume : 0f;
    }

    // ─── Coroutines ──────────────────────────────────
    IEnumerator CrossfadeTo(AudioClip newClip)
    {
        float elapsed = 0f;
        float startVolume = musicSource.volume;

        while (elapsed < crossfadeDuration / 2f)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (crossfadeDuration / 2f));
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.loop = true;
        musicSource.Play();

        elapsed = 0f;
        float targetVolume = IsMusicEnabled() ? musicVolume : 0f;
        while (elapsed < crossfadeDuration / 2f)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / (crossfadeDuration / 2f));
            yield return null;
        }
        musicSource.volume = targetVolume;
    }

    IEnumerator FadeOutMusic()
    {
        float elapsed = 0f;
        float startVolume = musicSource.volume;
        float duration = 0.6f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = IsMusicEnabled() ? musicVolume : 0f;
    }

    bool IsMusicEnabled() => PlayerPrefs.GetInt("Music_Enabled", 1) == 1;
    bool IsSFXEnabled() => PlayerPrefs.GetInt("SFX_Enabled", 1) == 1;
}