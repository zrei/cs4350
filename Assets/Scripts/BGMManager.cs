using UnityEngine;

public class BGMManager : MonoBehaviour
{
    [SerializeField] protected AudioDataSO m_BGM;

    protected int m_CurrentlyPlayingAudio;

    protected virtual void Awake()
    {
        StartPlayingBGM();
    }

    protected virtual void StartPlayingBGM()
    {
        if (!SoundManager.IsReady)
        {
            SoundManager.OnReady += StartPlayingBGM;
            return;
        }

        SoundManager.OnReady -= StartPlayingBGM;

        m_CurrentlyPlayingAudio = SoundManager.Instance.PlayWithFadeIn(m_BGM);
    }

    public void FadeOutBgm()
    {
        SoundManager.Instance.FadeOutAndStop(m_CurrentlyPlayingAudio);
    }
}
