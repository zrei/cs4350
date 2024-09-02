using TMPro;
using UnityEngine;

public class UI_BattleResult : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_ResultText;

    private void Awake()
    {
        m_ResultText.gameObject.SetActive(false);
        GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
    }

    private void OnDestroy()
    {
        GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
    }

    private void OnBattleEnd(UnitAllegiance victor)
    {
        m_ResultText.gameObject.SetActive(true);
        m_ResultText.text = victor switch
        {
            UnitAllegiance.PLAYER => "You win!",
            UnitAllegiance.ENEMY => "You lose!",
            _ => "???"
        };
    }
}