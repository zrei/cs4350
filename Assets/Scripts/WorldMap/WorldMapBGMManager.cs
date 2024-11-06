using UnityEngine;

public class WorldMapBGMManager : MonoBehaviour
{
    [SerializeField] AudioDataSO m_WorldMapBGM;

    private int m_CurrentlyPlayingAudio;

    private void Awake()
    {
        StartPlayingWorldMapBGM();

        GlobalEvents.WorldMap.OnBeginLoadLevelEvent += OnBeginLoadLevel;
        GlobalEvents.Level.ReturnFromLevelEvent += StartPlayingWorldMapBGM;
    }

    private void StartPlayingWorldMapBGM()
    {
        if (!SoundManager.IsReady)
        {
            SoundManager.OnReady += StartPlayingWorldMapBGM;
            return;
        }

        SoundManager.OnReady -= StartPlayingWorldMapBGM;

        m_CurrentlyPlayingAudio = SoundManager.Instance.PlayWithFadeIn(m_WorldMapBGM);
    }

    private void OnDestroy()
    {
        GlobalEvents.WorldMap.OnBeginLoadLevelEvent -= OnBeginLoadLevel;
        GlobalEvents.Level.ReturnFromLevelEvent -= StartPlayingWorldMapBGM;
    }

    private void OnBeginLoadLevel()
    {
        SoundManager.Instance.FadeOutAndStop(m_CurrentlyPlayingAudio);
    }
}