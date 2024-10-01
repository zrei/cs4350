using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit
{
    [Header("Pass Action")]
    [SerializeField] EnemyPassActionSO m_PassAction;

    public override UnitAllegiance UnitAllegiance => UnitAllegiance.ENEMY;

    //private EnemyActionSetSO m_Actions;

    private List<(EnemyActionCondition, EnemyActionSO)> m_OrderedConditions;
    private List<(int, EnemyActionSO)> m_OrderedActions;

    public void Initialise(Stats stats, ClassSO enemyClass, EnemyActionSetSO actionSet, Sprite enemySprite, UnitModelData unitModelData)
    {
        base.Initialise(stats, enemyClass, enemySprite, unitModelData);
        InitialiseActions(actionSet);
        //m_Actions = actionSet;
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
        m_OrderedConditions.Sort((x, y) => x.Item1.m_Priority.CompareTo(y.Item1.m_Priority));
        m_OrderedActions.Sort((x, y) => x.Item1.CompareTo(y.Item1));
    }

    public void PerformAction(MapLogic mapLogic, VoidEvent completeActionEvent)
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
                action.PerformAction(this, mapLogic, completeActionEvent);
                return;
            }
        }

        foreach ((int priority, EnemyActionSO enemyActionSO) in m_OrderedActions)
        {
            if (unperformableActions.Contains(enemyActionSO))
                continue;
            
            enemyActionSO.PerformAction(this, mapLogic, completeActionEvent);
            return;
        }
        // m_Actions.PerformAction(this, mapLogic, completeActionEvent);
    }
}
