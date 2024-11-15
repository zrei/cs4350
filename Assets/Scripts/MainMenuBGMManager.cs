public class MainMenuBGMManager : BGMManager
{
    protected override void Awake()
    {
        base.Awake();

        GlobalEvents.MainMenu.OnBeginLoadWorldMap += OnBeginLoadWorldMap;
    }

    private void OnDestroy()
    {
        GlobalEvents.MainMenu.OnBeginLoadWorldMap -= OnBeginLoadWorldMap;
    }

    private void OnBeginLoadWorldMap()
    {
        FadeOutCurrBgm();
    }
}
