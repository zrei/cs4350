public class MainMenuBGMManager : BGMManager
{
    protected override void Awake()
    {
        GlobalEvents.MainMenu.OnBeginLoadWorldMap += OnBeginLoadWorldMap;
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
