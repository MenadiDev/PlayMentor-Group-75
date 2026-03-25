using UnityEngine;


public class SceneAudioTrigger : MonoBehaviour
{
    public enum MusicTrack { None, Dashboard, Quiz, Stop }

    [SerializeField] private MusicTrack trackOnLoad = MusicTrack.Dashboard;

    void Start()
    {
        if (AudioManager.Instance == null) return;

        switch (trackOnLoad)
        {
            case MusicTrack.Dashboard: AudioManager.Instance.PlayDashboardMusic(); break;
            case MusicTrack.Quiz: AudioManager.Instance.PlayQuizMusic(); break;
            case MusicTrack.Stop: AudioManager.Instance.StopMusic(); break;
            case MusicTrack.None: break; // keep whatever is playing
        }
    }
}
