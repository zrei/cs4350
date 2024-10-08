using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BattleResult : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_ResultText;
    [SerializeField] Button m_ReturnButton;

    private void Awake()
    {
        m_ResultText.gameObject.SetActive(false);
        m_ReturnButton.onClick.AddListener(ReturnFromBattle);
        m_ReturnButton.gameObject.SetActive(false);
        GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
    }

    private void OnDestroy()
    {
        GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
    }

    private void OnBattleEnd(UnitAllegiance victor, int numTurns)
    {
        m_ResultText.gameObject.SetActive(true);
        m_ResultText.text = victor switch
        {
            UnitAllegiance.PLAYER => "You win!",
            UnitAllegiance.ENEMY => "You lose!",
            _ => "???"
        };
        
        m_ReturnButton.gameObject.SetActive(true);
    }
    
    public void ReturnFromBattle()
    {
        m_ResultText.gameObject.SetActive(false);
        m_ReturnButton.gameObject.SetActive(false);
        GameSceneManager.Instance.UnloadBattleScene();
    }
}