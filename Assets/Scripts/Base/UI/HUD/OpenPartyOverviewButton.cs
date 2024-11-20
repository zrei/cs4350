using Game.UI;
using System.Collections.Generic;
using UnityEngine;

public class OpenPartyOverviewButton : MonoBehaviour
{
    [SerializeField]
    SelectableBase m_OpenButton;

    private void Awake()
    {
        HandleAwake();
    }

    private void OnDestroy()
    {
        HandleDestroy();
    }

    protected virtual void HandleAwake()
    {
        m_OpenButton.onSubmit.AddListener(OpenPartyOverview);
    }

    protected virtual void HandleDestroy()
    {
        GlobalEvents.UI.OpenPartyOverviewEvent -= OnOpenPartyOverview;
        GlobalEvents.UI.OnClosePartyOverviewEvent -= OnPartyOverviewClosed;
    }

    protected virtual void OpenPartyOverview()
    {
        IUIScreen characterManagementScreen = UIScreenManager.Instance.CharacterManagementScreen;
        if (UIScreenManager.Instance.IsScreenOpen(characterManagementScreen)) return;

        if (LevelManager.IsReady)
        {
            UIScreenManager.Instance.OpenScreen(characterManagementScreen, false, new CharacterManagementUIData(LevelManager.Instance.CurrParty, true));
        }
        else if (CharacterDataManager.IsReady)
        {
            UIScreenManager.Instance.OpenScreen(characterManagementScreen, false, new CharacterManagementUIData(CharacterDataManager.Instance.RetrieveAllCharacterData(new List<int>()), false));
        }
    }

    private void OnOpenPartyOverview(List<PlayerCharacterData> list, bool _)
    {
        m_OpenButton.onSubmit.RemoveListener(OpenPartyOverview);
    }

    private void OnPartyOverviewClosed()
    {
        m_OpenButton.onSubmit.AddListener(OpenPartyOverview);
    }

    protected void EnablePartyOverview()
    {
        GlobalEvents.UI.OpenPartyOverviewEvent += OnOpenPartyOverview;
        GlobalEvents.UI.OnClosePartyOverviewEvent += OnPartyOverviewClosed;
    }

    protected void DisablePartyOverview()
    {
        GlobalEvents.UI.OpenPartyOverviewEvent -= OnOpenPartyOverview;
        GlobalEvents.UI.OnClosePartyOverviewEvent -= OnPartyOverviewClosed;
    }
}