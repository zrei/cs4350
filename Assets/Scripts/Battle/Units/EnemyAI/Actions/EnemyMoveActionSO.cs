using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyMoveActionSO", menuName="ScriptableObject/Battle/EnemyAI/Actions/EnemyMoveActionSO")]
public class EnemyMoveActionSO : EnemyActionSO
{
    public float m_BaseCanBeAttackedProportion = 1f;
    public float m_BaseCanPerformActiveSkillProportion = 1f;
    public float m_BaseCanReceiveBuffProportion = 1f;
    public List<EnemyMoveCondition> m_EnemyMoveConditions;

    private HashSet<PathNode> m_ReachablePoints;

    public override bool CanActionBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        m_ReachablePoints = mapLogic.CalculateReachablePoints(GridType.ENEMY, enemyUnit, (int) enemyUnit.GetTotalStat(StatType.MOVEMENT_RANGE));
        // check if there's any space to move to
        return m_ReachablePoints.Count > 0;
    }

    public override void PerformAction(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent completeActionEvent)
    {
        IEnumerable<EnemyMoveCondition> metConditions = m_EnemyMoveConditions.Where(x => x.IsConditionMet(enemyUnit, mapLogic));
        // for each point: is an action doable from there? is an attack receivable from there? is a buff receivable from there?
        // different weights for each situation that then randomly selects a square

        float baseWeight = 1f / m_ReachablePoints.Count;
        Debug.Log("Base weight: " + baseWeight);

        List<(PathNode, float)> nodeWeights = m_ReachablePoints.Select(x => (x, baseWeight)).ToList();

        float finalCanBeAttackedProportion = m_BaseCanBeAttackedProportion;
        foreach (EnemyMoveCondition condition in metConditions)
        {
            finalCanBeAttackedProportion *= condition.m_CanBeAttackedProportion;
        }

        float finalCanPerformActiveSkillProportion = m_BaseCanPerformActiveSkillProportion;
        foreach (EnemyMoveCondition condition in metConditions)
        {
            finalCanPerformActiveSkillProportion *= condition.m_CanPerformActiveSkillProportion;
        }

        float finalCanReceiveBuffProportion = m_BaseCanReceiveBuffProportion;
        foreach (EnemyMoveCondition condition in metConditions)
        {
            finalCanReceiveBuffProportion *= condition.m_CanReceiveBuffProportion;
        }

        for (int i = 0; i < nodeWeights.Count; ++i)
        {
            (PathNode node, float weight) = nodeWeights[i];
            float finalNodeWeight = weight;
            CoordPair position = node.m_Coordinates;
            if (CanBeAttackedAtPosition(position, mapLogic))
            {
                finalNodeWeight *= finalCanBeAttackedProportion;
            }
            
            if (CanUseActiveSkillAtPosition(position, enemyUnit, mapLogic))
            {
                finalNodeWeight *= finalCanPerformActiveSkillProportion;
            }

            if (CanReceiveBuffAtPosition(position, mapLogic))
            {
                finalNodeWeight *= finalCanReceiveBuffProportion;
            }

            nodeWeights[i] = (node, finalNodeWeight);
        }

        PathNode toMoveTo = RandomHelper.GetRandomT(nodeWeights);

        mapLogic.MoveUnit(GridType.ENEMY, enemyUnit, nodeWeights.Last().Item1, completeActionEvent);
    }

    public bool CanBeAttackedAtPosition(CoordPair moveToPosition, MapLogic mapLogic)
    {
        return false;
    }

    public bool CanUseActiveSkillAtPosition(CoordPair moveToPosition, EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return false;
    }

    public bool CanReceiveBuffAtPosition(CoordPair moveToPosition, MapLogic mapLogic)
    {
        return false;
    }
}

[System.Serializable]
public class EnemyMoveCondition : ICondition
{
    public EnemyConditionSO m_Condition;
    public float m_CanBeAttackedProportion = 1f;
    public float m_CanPerformActiveSkillProportion = 1f;
    public float m_CanReceiveBuffProportion = 1f;

    public bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic) => m_Condition.IsConidtionMet(enemyUnit, mapLogic);
}