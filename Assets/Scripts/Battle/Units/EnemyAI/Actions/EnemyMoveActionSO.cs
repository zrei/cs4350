using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyMoveActionSO", menuName="ScriptableObject/Battle/EnemyAI/Actions/EnemyMoveActionSO")]
public class EnemyMoveActionSO : EnemyActionSO
{
    public List<EnemyTileCondition> m_TargetConditions;


    public HashSet<PathNode> GetReachablePoints(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return mapLogic.CalculateReachablePoints(GridType.ENEMY, enemyUnit, (int) enemyUnit.GetTotalStat(StatType.MOVEMENT_RANGE));
    }

    public override bool CanActionBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return GetReachablePoints(enemyUnit, mapLogic).Count > 0;
    }

    // preparation for caching
    public PathNode CalculateMovementPosition(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        HashSet<PathNode> reachablePoints = GetReachablePoints(enemyUnit, mapLogic);

        float baseWeight = 1f / reachablePoints.Count;

        List<(PathNode, float)> nodeWeights = reachablePoints.Select(x => (x, baseWeight)).ToList();

        for (int i = 0; i < nodeWeights.Count; ++i)
        {
            (PathNode node, float weight) = nodeWeights[i];
            float finalNodeWeight = weight;
            CoordPair position = node.m_Coordinates;

            foreach (EnemyTileCondition targetCondition in m_TargetConditions)
            {
                if (targetCondition.IsConditionMet(enemyUnit, mapLogic, position))
                    finalNodeWeight *= targetCondition.m_MultProportion;
            }

            nodeWeights[i] = (node, finalNodeWeight);
        }

        return RandomHelper.GetRandomT(nodeWeights);
    }

    public void PerformMove(EnemyUnit enemyUnit, MapLogic mapLogic, PathNode targetTile, VoidEvent completeActionEvent)
    {
        mapLogic.MoveUnit(GridType.ENEMY, enemyUnit, targetTile, completeActionEvent);
    }
}

[System.Serializable]
public struct EnemyTileCondition
{
    public EnemyTileConditionSO m_Condition;
    public float m_MultProportion;
    public bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile) => m_Condition.IsConditionMet(enemyUnit, mapLogic, targetTile);
}
