public class WorldMapBGMManager : BGMManager
{
    protected override void Awake()
    {
        base.Awake();

        GlobalEvents.Scene.OnBeginSceneChange += OnBeginLoadLevel;
        GlobalEvents.Scene.OnSceneTransitionEvent += OnSceneTransition;
        GlobalEvents.Dialogue.DialogueEndEvent += OnCutsceneEnd;
    }

    private void OnDestroy()
    {
        GlobalEvents.Scene.OnBeginSceneChange -= OnBeginLoadLevel;
        GlobalEvents.Scene.OnSceneTransitionEvent -= OnSceneTransition;
        GlobalEvents.Dialogue.DialogueEndEvent -= OnCutsceneEnd;
    }

    private void OnBeginLoadLevel(SceneEnum fromScene, SceneEnum toScene)
    {
        if (fromScene != SceneEnum.WORLD_MAP)
            return;
        
        FadeOutCurrBgm();
    }

    private void OnSceneTransition(SceneEnum sceneEnum)
    {
        if (sceneEnum != SceneEnum.WORLD_MAP)
            return;

        if (!m_CurrentlyPlayingDefaultBgm)
            StartPlayingDefaultBGM();
    }

    private void OnCutsceneEnd()
    {
        if (!m_CurrentlyPlayingDefaultBgm)
        {
            FadeOutCurrBgm(StartPlayingDefaultBGM);
        }
    }
}
