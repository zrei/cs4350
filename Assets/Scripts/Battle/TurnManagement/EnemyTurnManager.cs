using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyTurnManager : TurnManager
{
    #region Test
    [Header("Test data")]
    [SerializeField] private AttackSO[] m_TestAttacks;
    [SerializeField] private int m_AttackRow;
    private int m_AttackIndex = 0;
    private bool m_AttackedLastTurn = false;

    private IEnumerator WaitForAttackToEnd()
    {
        yield return new WaitForSeconds(2f);
        CompleteTurn();
    }
    #endregion

    private EnemyUnit m_CurrUnit;

    public void PerformTurn(EnemyUnit enemyUnit)
    {
        m_CurrUnit = enemyUnit;
        Logger.Log(this.GetType().Name, "Start enemy turn with " + m_CurrUnit.name, LogLevel.LOG);

        GlobalEvents.Battle.EnemyTurnStartEvent?.Invoke();

        AttackSO attackSO = m_TestAttacks[m_AttackIndex];
        List<PathNode> moveableTiles = m_MapLogic.CalculateReachablePoints(GridType.ENEMY, m_CurrUnit).ToList();

        if (!m_AttackedLastTurn)
        {
            for (int i = 0; i < MapData.NUM_COLS; ++i)
            {
                CoordPair target = new CoordPair(m_AttackRow, i);
                if (m_MapLogic.IsTileOccupied(GridType.PLAYER, target))
                {
                    if (attackSO.IsValidTargetTile(target, m_CurrUnit))
                    {
                        m_MapLogic.Attack(GridType.PLAYER, m_CurrUnit, attackSO, target);
                        m_AttackIndex = (m_AttackIndex + 1) % m_TestAttacks.Length;
                        Logger.Log(this.GetType().Name, $"Enemy {m_CurrUnit.name} attacks!", LogLevel.LOG);
                        m_AttackedLastTurn = true;
                        StartCoroutine(WaitForAttackToEnd());
                        return;
                    }
                }
            }
        }
        
        m_AttackedLastTurn = false;

        if (moveableTiles.Count > 0)
        {
            PathNode finalPos = moveableTiles[Random.Range(0, moveableTiles.Count - 1)];
            Logger.Log(this.GetType().Name, $"Move enemy {m_CurrUnit.name} from {m_CurrUnit.CurrPosition} to {finalPos.m_Coordinates}", LogLevel.LOG);
            m_MapLogic.MoveUnit(GridType.ENEMY, m_CurrUnit, finalPos, CompleteTurn);
            return;
        }

        Logger.Log(this.GetType().Name, $"No move available for enemy {m_CurrUnit.name}, skipping turn", LogLevel.LOG);

    }

    private void CompleteTurn()
    {
        m_CompleteTurnEvent?.Invoke(m_CurrUnit);
    }
}