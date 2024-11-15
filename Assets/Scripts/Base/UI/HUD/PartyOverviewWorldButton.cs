using System.Collections.Generic;
using Game.UI;

public class PartyOverviewWorldButton : OpenPartyOverviewButton
{
    protected override void HandleAwake()
    {
        base.HandleAwake();
        GlobalEvents.WorldMap.OnGoToLevel += OnGoToLevel;
        GlobalEvents.WorldMap.OnBeginLoadLevelEvent += DisablePartyOverview;
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();
        GlobalEvents.WorldMap.OnGoToLevel -= OnGoToLevel;
        GlobalEvents.WorldMap.OnBeginLoadLevelEvent -= DisablePartyOverview;
    }

    #region WorldMap

    private void OnGoToLevel(LevelData _)
    {
        EnablePartyOverview();
    }

    #endregion
    
    #region PartyOverview
    
    protected override void OpenPartyOverview()
    {
        if (UIScreenManager.Instance.IsScreenOpen(UIScreenManager.Instance.CharacterManagementScreen)) return;
        
        GlobalEvents.UI.OpenPartyOverviewEvent?.Invoke(CharacterDataManager.Instance.RetrieveAllCharacterData(new List<int>()), false);

        UIScreenManager.Instance.OpenScreen(UIScreenManager.Instance.CharacterManagementScreen);
    }
    
    #endregion
}