public class WorldMapBGMManager : BGMManager
{
    protected override void Awake()
    {
        base.Awake();

        GlobalEvents.WorldMap.OnBeginLoadLevelEvent += OnBeginLoadLevel;
        GlobalEvents.Level.ReturnFromLevelEvent += StartPlayingDefaultBGM;
        GlobalEvents.Dialogue.DialogueEndEvent += OnCutsceneEnd;
    }

    private void OnDestroy()
    {
        GlobalEvents.WorldMap.OnBeginLoadLevelEvent -= OnBeginLoadLevel;
        GlobalEvents.Level.ReturnFromLevelEvent -= StartPlayingDefaultBGM;
        GlobalEvents.Dialogue.DialogueEndEvent -= OnCutsceneEnd;
    }

    private void OnBeginLoadLevel()
    {
        FadeOutCurrBgm();
    }

    private void OnCutsceneEnd()
    {
        if (!m_CurrentlyPlayingDefaultBgm)
        {
            FadeOutCurrBgm(StartPlayingDefaultBGM);
        }
    }
}
