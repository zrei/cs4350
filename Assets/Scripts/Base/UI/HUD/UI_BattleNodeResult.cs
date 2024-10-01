using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// Temporary UI for level result, to integrated with UI_Manager System
public class UI_BattleNodeResult : MonoBehaviour
{
    [SerializeField] GameObject m_BattleResultPanel;
    [SerializeField] TextMeshProUGUI m_ResultText;
    [SerializeField] Button m_ReturnButton;

    private void Awake()
    {
        m_ReturnButton.onClick.AddListener(CloseResults);
        m_BattleResultPanel.SetActive(false);
        GlobalEvents.Level.BattleNodeEndEvent += OnBattleNodeEnd;
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
        m_BattleResultPanel.SetActive(true);
    }
    
    public void CloseResults()
    {
        m_BattleResultPanel.SetActive(false);
    }
}