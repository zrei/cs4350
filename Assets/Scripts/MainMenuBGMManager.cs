using UnityEngine;

public class MainMenuBGMManager : BGMManager
{
    protected override void Awake()
    {
        base.Awake();

        GlobalEvents.Scene.OnBeginSceneChange += OnBeginLoadWorldMap;
    }

    private void OnDestroy()
    {
        GlobalEvents.Scene.OnBeginSceneChange -= OnBeginLoadWorldMap;
    }

    private void OnBeginLoadWorldMap(SceneEnum _, SceneEnum _2)
    {
        FadeOutCurrBgm();
    }
}
