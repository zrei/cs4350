public class WorldMapBGMManager : BGMManager
{
    private bool m_CurrentlyPlayingSceneBgm = false;

    protected override void Awake()
    {
        GlobalEvents.WorldMap.OnBeginLoadLevelEvent += OnBeginLoadLevel;
        GlobalEvents.Level.ReturnFromLevelEvent += StartPlayingBGM;
        GlobalEvents.Dialogue.DialogueEndEvent += OnCutsceneEnd;
    }

    private void OnDestroy()
    {
        GlobalEvents.WorldMap.OnBeginLoadLevelEvent -= OnBeginLoadLevel;
        GlobalEvents.Level.ReturnFromLevelEvent -= StartPlayingBGM;
        GlobalEvents.Dialogue.DialogueEndEvent -= OnCutsceneEnd;
    }

    private void OnBeginLoadLevel()
    {
        FadeOutBgm();
    }

    protected override void StartPlayingBGM()
    {
        base.StartPlayingBGM();

        m_CurrentlyPlayingSceneBgm = true;
    }

    private void OnCutsceneEnd()
    {
        if (!m_CurrentlyPlayingSceneBgm)
        {
            m_CurrentlyPlayingAudio = SoundManager.Instance.PlayWithFadeIn(m_BGM);
        }
    }
}
