using UnityEngine;

public class BGMManager : MonoBehaviour
{
    [SerializeField] protected AudioDataSO m_DefaultBGM;

    protected int m_CurrentlyPlayingAudioToken;
    protected bool m_CurrentlyPlayingDefaultBgm = false;

    protected virtual void Awake()
    {
        StartPlayingDefaultBGM();
    }

    protected virtual void StartPlayingDefaultBGM()
    {
        if (!SoundManager.IsReady)
        {
            SoundManager.OnReady += StartPlayingDefaultBGM;
            return;
        }

        SoundManager.OnReady -= StartPlayingDefaultBGM;

        m_CurrentlyPlayingAudioToken = SoundManager.Instance.PlayWithFadeIn(m_DefaultBGM);
        m_CurrentlyPlayingDefaultBgm = true;
    }

    public void FadeOutCurrBgm(VoidEvent postFade = null)
    {
        SoundManager.Instance.FadeOutAndStop(m_CurrentlyPlayingAudioToken, 2f, postFade);
    }

    public void PlayOtherBgm(AudioDataSO audio)
    {
        m_CurrentlyPlayingAudioToken = SoundManager.Instance.PlayWithFadeIn(audio);
        m_CurrentlyPlayingDefaultBgm = false;
    }
}
