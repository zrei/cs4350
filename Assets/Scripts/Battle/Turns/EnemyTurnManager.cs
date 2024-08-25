using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnManager : TurnManager
{
    public void PerformTurn(EnemyUnit enemyUnit)
    {
        m_CompleteTurnEvent?.Invoke();
    }
}