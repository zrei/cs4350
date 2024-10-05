using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class UI_TurnOrder : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_TurnOrderText;

    private void Awake()
    {
        GlobalEvents.Battle.TurnOrderUpdatedEvent += OnTurnOrderUpdated;
        GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
    }

    private void OnDestroy()
    {
        GlobalEvents.Battle.TurnOrderUpdatedEvent -= OnTurnOrderUpdated;
        GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
    }

    private void OnTurnOrderUpdated(List<Unit> newTurnOrder)
    {
        StringBuilder stringBuilder = new StringBuilder("Turn order: ");
        foreach (Unit unit in newTurnOrder)
        {
            stringBuilder.Append(unit.name + ", ");
        }
        m_TurnOrderText.text = stringBuilder.ToString();
    }

    private void OnBattleEnd(UnitAllegiance _, int _2)
    {
        gameObject.SetActive(false);
    }
}