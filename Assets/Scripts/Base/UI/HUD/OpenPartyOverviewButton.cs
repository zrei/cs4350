using Game.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class OpenPartyOverviewButton : MonoBehaviour
{
    [SerializeField] SelectableBase m_OpenButton;

    private CanvasGroup m_CanvasGroup;

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
        m_CanvasGroup = GetComponent<CanvasGroup>();
        m_CanvasGroup.alpha = 0f;
        m_CanvasGroup.interactable = false;
        m_CanvasGroup.blocksRaycasts = false;
        m_OpenButton.onSubmit.AddListener(OpenPartyOverview);
    }

    protected virtual void HandleDestroy()
    {
        GlobalEvents.UI.OpenPartyOverviewEvent -= OnOpenPartyOverview;
        GlobalEvents.UI.OnClosePartyOverviewEvent -= OnPartyOverviewClosed;
    }
    
    #region PartyOverview
    
    protected virtual void OpenPartyOverview()
    {
        if (UIScreenManager.Instance.IsScreenOpen(UIScreenManager.Instance.CharacterManagementScreen)) return;
        UIScreenManager.Instance.OpenScreen(UIScreenManager.Instance.CharacterManagementScreen);
    }
    
    private void OnOpenPartyOverview(List<PlayerCharacterData> list, bool _)
    {
        m_OpenButton.onSubmit.RemoveListener(OpenPartyOverview);
        SetActive(false);
    }
    
    private void OnPartyOverviewClosed()
    {
        m_OpenButton.onSubmit.AddListener(OpenPartyOverview);
        SetActive(true);
    }
    
    protected void EnablePartyOverview()
    {
        SetActive(true);
        GlobalEvents.UI.OpenPartyOverviewEvent += OnOpenPartyOverview;
        GlobalEvents.UI.OnClosePartyOverviewEvent += OnPartyOverviewClosed;
    }
    
    protected void DisablePartyOverview()
    {
        SetActive(false);
        GlobalEvents.UI.OpenPartyOverviewEvent -= OnOpenPartyOverview;
        GlobalEvents.UI.OnClosePartyOverviewEvent -= OnPartyOverviewClosed;
    }
    
    #endregion

    private void SetActive(bool active)
    {
        IEnumerator Transition()
        {
            var targetA = active ? 1f : 0f;
            var currentA = m_CanvasGroup.alpha;

            var t = 0f;
            var duration = 0.1f;
            while (t < duration)
            {
                t += Time.deltaTime;
                m_CanvasGroup.alpha = Mathf.Lerp(currentA, targetA, t / duration);
                yield return null;
            }
            m_CanvasGroup.alpha = targetA;
            m_CanvasGroup.interactable = active;
            m_CanvasGroup.blocksRaycasts = active;
        }
        StopAllCoroutines();
        StartCoroutine(Transition());
    }
}