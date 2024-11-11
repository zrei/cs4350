using Game.UI;

public class PartyOverviewLevelButton : OpenPartyOverviewButton
{
    private LevelManager m_LevelManager;

    protected override void HandleAwake()
    {
        base.HandleAwake();
        GlobalEvents.Scene.LevelSceneLoadedEvent += OnLevelLoaded;
        GlobalEvents.Level.LevelEndEvent += OnLevelEnd;
        GlobalEvents.Scene.EarlyQuitEvent += OnLevelEnd;
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();
        GlobalEvents.Scene.LevelSceneLoadedEvent -= OnLevelLoaded;
        GlobalEvents.Level.LevelEndEvent -= OnLevelEnd;
        GlobalEvents.Scene.EarlyQuitEvent -= OnLevelEnd;
        GlobalEvents.Level.StartPlayerPhaseEvent -= EnablePartyOverview;
        GlobalEvents.Level.EndPlayerPhaseEvent -= DisablePartyOverview;
    }

    #region Level

    private void OnLevelLoaded()
    {
        m_LevelManager = FindObjectOfType<LevelManager>();
        
        GlobalEvents.Level.StartPlayerPhaseEvent += EnablePartyOverview;
        GlobalEvents.Level.EndPlayerPhaseEvent += DisablePartyOverview;
    }

    private void OnLevelEnd()
    {
        m_LevelManager = null;
        
        DisablePartyOverview();
        GlobalEvents.Level.StartPlayerPhaseEvent -= EnablePartyOverview;
        GlobalEvents.Level.EndPlayerPhaseEvent -= DisablePartyOverview;
    }

    #endregion
    
    #region PartyOverview
    
    protected override void OpenPartyOverview()
    {
        if (UIScreenManager.Instance.IsScreenOpen(UIScreenManager.Instance.CharacterManagementScreen)) return;
        
        GlobalEvents.UI.OpenPartyOverviewEvent?.Invoke(m_LevelManager.CurrParty, true);

        UIScreenManager.Instance.OpenScreen(UIScreenManager.Instance.CharacterManagementScreen);
    }
    
    #endregion
}