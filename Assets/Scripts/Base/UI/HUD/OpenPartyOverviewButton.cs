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
        if (UIScreenManager.Instance.IsScreenOpen(UIScreenManager.Instance.CharacterManagementScreen)) return;

        if (LevelManager.IsReady)
        {
            GlobalEvents.UI.OpenPartyOverviewEvent?.Invoke(LevelManager.Instance.CurrParty, true);
        }
        else if (CharacterDataManager.IsReady)
        {
            GlobalEvents.UI.OpenPartyOverviewEvent?.Invoke(CharacterDataManager.Instance.RetrieveAllCharacterData(new List<int>()), false);
        }

        UIScreenManager.Instance.OpenScreen(UIScreenManager.Instance.CharacterManagementScreen);
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