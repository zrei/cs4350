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
        m_OpenButton.onSubmit.RemoveListener(OpenPartyOverview);
    }

    protected virtual void OpenPartyOverview()
    {
        IUIScreen characterManagementScreen = UIScreenManager.Instance.CharacterManagementScreen;
        if (UIScreenManager.Instance.IsScreenOpen(characterManagementScreen)) return;

        if (LevelManager.IsReady)
        {
            UIScreenManager.Instance.OpenScreen(characterManagementScreen, false, LevelManager.Instance.CurrParty);
        }
        else if (CharacterDataManager.IsReady)
        {
            UIScreenManager.Instance.OpenScreen(characterManagementScreen, false, CharacterDataManager.Instance.RetrieveAllCharacterData(new List<int>()));
        }
    }
}