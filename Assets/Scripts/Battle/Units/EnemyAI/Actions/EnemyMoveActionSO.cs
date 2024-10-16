using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyMoveActionWrapper : EnemyActionWrapper
{
    private HashSet<PathNode> m_ReachablePoints;
    private PathNode m_CachedTarget;

    private EnemyMoveActionSO MoveAction => (EnemyMoveActionSO) m_Action;

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
            float finalNodeWeight = weight * MoveAction.GetFinalWeightProportionForTile(enemyUnit, mapLogic, node.m_Coordinates);

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
            float finalNodeWeight = weight * MoveAction.GetFinalWeightProportionForTile(enemyUnit, mapLogic, node.m_Coordinates);
            nodeWeights[i] = (node, finalNodeWeight);
        }

        PathNode toMoveTo = RandomHelper.GetRandomT(nodeWeights);

        mapLogic.MoveUnit(GridType.ENEMY, enemyUnit, toMoveTo, completeActionEvent);
    }
}

[System.Serializable]
public struct EnemyMoveTileCondition
{
    public EnemyMoveTileConditionSO m_Condition;
    public float m_MultProportion;
    public bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile) => m_Condition.IsConditionMet(enemyUnit, mapLogic, targetTile);
}

[CreateAssetMenu(fileName = "EnemyMoveActionSO", menuName="ScriptableObject/Battle/Enemy/EnemyAI/Actions/EnemyMoveActionSO")]
public class EnemyMoveActionSO : EnemyActionSO
{
    public List<EnemyMoveTileCondition> m_TargetConditions;

    public float GetFinalWeightProportionForTile(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair target)
    {
        float finalNodeWeight = 1f;

        foreach (EnemyMoveTileCondition targetCondition in m_TargetConditions)
        {
            if (targetCondition.IsConditionMet(enemyUnit, mapLogic, target))
                finalNodeWeight *= targetCondition.m_MultProportion;
        }

        return finalNodeWeight;
    }

    public override EnemyActionWrapper GetWrapper(int priority)
    {
        return new EnemyMoveActionWrapper {m_Action = this, m_Priority = priority};
    }
}
