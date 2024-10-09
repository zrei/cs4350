using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Temporary UI for rewards from battle and reward nodes,
// to be integrated with UI_Manager System
public class UI_NodeRewards : MonoBehaviour
{
    [SerializeField] GameObject m_RewardPanel;
    [SerializeField] TextMeshProUGUI m_ResultText;
    [SerializeField] Button m_ReturnButton;

    private void Awake()
    {
        m_RewardPanel.SetActive(false);
        // GlobalEvents.Level.BattleNodeEndEvent += OnBattleNodeEnd;
        GlobalEvents.Level.RewardNodeStartEvent += OnRewardNodeStart;
    }

    private void OnDestroy()
    {
        GlobalEvents.Level.BattleNodeEndEvent -= OnBattleNodeEnd;
    }

    private void OnBattleNodeEnd(BattleNode battleNode, UnitAllegiance victor)
    {
        if (victor != UnitAllegiance.PLAYER) return;
        
        var expReward = battleNode.BattleSO.m_ExpReward;
        
        m_ResultText.text = $"Gained {expReward} EXP!";
        
        m_RewardPanel.SetActive(true);
        m_ReturnButton.onClick.AddListener(CloseResults);
    }
    
    private void OnRewardNodeStart(RewardNode rewardNode)
    {
        var goldReward = rewardNode.GoldReward;
        
        m_ResultText.text = $"Gained {goldReward} GOLD!";
        
        m_RewardPanel.SetActive(true);
        m_ReturnButton.onClick.AddListener(CloseResults);
    }
    
    private void CloseResults()
    {
        m_RewardPanel.SetActive(false);
        GlobalEvents.Level.CloseRewardScreenEvent?.Invoke();
        m_ReturnButton.onClick.RemoveListener(CloseResults);
    }
}