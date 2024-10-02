using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit
{
    public override UnitAllegiance UnitAllegiance => UnitAllegiance.ENEMY;

    private List<(EnemyActionCondition, EnemyActionSO)> m_OrderedConditions;
    private List<(int, EnemyActionSO)> m_OrderedActions;

    public void Initialise(Stats stats, ClassSO enemyClass, EnemyActionSetSO actionSet, Sprite enemySprite, UnitModelData unitModelData)
    {
        base.Initialise(stats, enemyClass, enemySprite, unitModelData);
        InitialiseActions(actionSet);
    }

    private void InitialiseActions(EnemyActionSetSO enemyActionSetSO)
    {
        m_OrderedConditions = new();
        m_OrderedActions = new();
        foreach (EnemyAction enemyAction in enemyActionSetSO.m_EnemyActions)
        {
            foreach (EnemyActionCondition condition in enemyAction.m_Conditions)
            {
                m_OrderedConditions.Add((condition, enemyAction.m_EnemyAction));
            }
            m_OrderedActions.Add((enemyAction.m_BasePriority, enemyAction.m_EnemyAction));
        }
        m_OrderedConditions.Sort((x, y) => y.Item1.m_Priority.CompareTo(x.Item1.m_Priority));
        m_OrderedActions.Sort((x, y) => y.Item1.CompareTo(x.Item1));
    }

    // can cache action
    public EnemyActionSO GetActionToBePerformed(MapLogic mapLogic)
    {
        HashSet<EnemyActionSO> unperformableActions = new();

        foreach ((EnemyActionCondition condition, EnemyActionSO action) in m_OrderedConditions)
        {
            if (unperformableActions.Contains(action))
                continue;

            if (!action.CanActionBePerformed(this, mapLogic))
            {
                unperformableActions.Add(action);
                continue;
            }

            if (condition.IsConditionMet(this, mapLogic))
            {
                return action;
            }
        }

        // if no condition has been met, retrieve an action according to their base priority and if it can be performed
        // Pass action can always be performed
        foreach ((int priority, EnemyActionSO enemyActionSO) in m_OrderedActions)
        {
            if (unperformableActions.Contains(enemyActionSO))
                continue;

            if (!enemyActionSO.CanActionBePerformed(this, mapLogic))
            {
                continue;
            }
            
            return enemyActionSO;
        }

        return default;
    }

    public void PerformAction(MapLogic mapLogic, VoidEvent completeActionEvent)
    {
        GetActionToBePerformed(mapLogic).PerformAction(this, mapLogic, completeActionEvent);
    }
}
