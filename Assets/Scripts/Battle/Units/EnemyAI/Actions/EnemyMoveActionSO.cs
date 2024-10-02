using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyMoveActionSO", menuName="ScriptableObject/Battle/EnemyAI/Actions/EnemyMoveActionSO")]
public class EnemyMoveActionSO : EnemyActionSO
{
    public List<EnemyTileCondition> m_TargetConditions;

    private HashSet<PathNode> m_ReachablePoints;
    private PathNode m_CachedTarget;

    public override bool CanActionBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        m_ReachablePoints = mapLogic.CalculateReachablePoints(GridType.ENEMY, enemyUnit, (int) enemyUnit.GetTotalStat(StatType.MOVEMENT_RANGE));
        // check if there's any space to move to
        return m_ReachablePoints.Count > 0;
    }

    // preparation for caching
    public void CalculateMovementPosition(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        float baseWeight = 1f / m_ReachablePoints.Count;

        List<(PathNode, float)> nodeWeights = m_ReachablePoints.Select(x => (x, baseWeight)).ToList();

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

        m_CachedTarget = RandomHelper.GetRandomT(nodeWeights);
    }

    public override void PerformAction(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent completeActionEvent)
    {
        float baseWeight = 1f / m_ReachablePoints.Count;

        List<(PathNode, float)> nodeWeights = m_ReachablePoints.Select(x => (x, baseWeight)).ToList();

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

        PathNode toMoveTo = RandomHelper.GetRandomT(nodeWeights);

        mapLogic.MoveUnit(GridType.ENEMY, enemyUnit, toMoveTo, completeActionEvent);
    }
}

[System.Serializable]
public struct EnemyTileCondition
{
    public EnemyTileConditionSO m_Condition;
    public float m_MultProportion;
    public bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile) => m_Condition.IsConditionMet(enemyUnit, mapLogic, targetTile);
}
