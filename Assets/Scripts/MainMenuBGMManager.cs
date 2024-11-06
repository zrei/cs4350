using UnityEngine;

public class MainMenuBGMManager : MonoBehaviour
{
    [SerializeField] AudioDataSO m_MainMenuBGM;

    private int m_CurrentlyPlayingAudio;

    private void Awake()
    {
        StartPlayingMainMenuBGM();

        GlobalEvents.MainMenu.OnBeginLoadWorldMap += OnBeginLoadWorldMap;
    }

    private void StartPlayingMainMenuBGM()
    {
        if (!SoundManager.IsReady)
        {
            SoundManager.OnReady += StartPlayingMainMenuBGM;
            return;
        }

        SoundManager.OnReady -= StartPlayingMainMenuBGM;

        m_CurrentlyPlayingAudio = SoundManager.Instance.PlayWithFadeIn(m_MainMenuBGM);
    }

    private void OnDestroy()
    {
        GlobalEvents.MainMenu.OnBeginLoadWorldMap -= OnBeginLoadWorldMap;
    }

    private void OnBeginLoadWorldMap()
    {
        SoundManager.Instance.FadeOutAndStop(m_CurrentlyPlayingAudio);
    }
}