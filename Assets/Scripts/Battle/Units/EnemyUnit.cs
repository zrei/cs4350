using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit
{
    [Header("Pass Action")]
    [SerializeField] EnemyPassActionSO m_PassAction;

    public override UnitAllegiance UnitAllegiance => UnitAllegiance.ENEMY;

    //private EnemyActionSetSO m_Actions;

    private List<(EnemyActionCondition, EnemyActionSO)> m_OrderedConditions;

    public void Initialise(Stats stats, ClassSO enemyClass, EnemyActionSetSO actionSet, Sprite enemySprite, UnitModelData unitModelData)
    {
        base.Initialise(stats, enemyClass, enemySprite, unitModelData);
        InitialiseActions(actionSet);
        //m_Actions = actionSet;
    }

    private void InitialiseActions(EnemyActionSetSO enemyActionSetSO)
    {
        m_OrderedConditions = new();
        foreach (EnemyAction enemyAction in enemyActionSetSO.m_EnemyActions)
        {
            foreach (EnemyActionCondition condition in enemyAction.m_Conditions)
            {
                m_OrderedConditions.Add((condition, enemyAction.m_EnemyAction));
            }
        }
        m_OrderedConditions.Sort((x, y) => x.Item1.m_Priority.CompareTo(y.Item1.m_Priority));
    }

    public void PerformAction(MapLogic mapLogic, VoidEvent completeActionEvent)
    {
        HashSet<EnemyActionSO> unperformableActions = new();
        bool actionHasBeenChosen = false;
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
                actionHasBeenChosen = true;
                break;
            }
        }

        if (!actionHasBeenChosen)
        {
            m_PassAction.PerformAction(this, mapLogic, completeActionEvent);
        }
        // m_Actions.PerformAction(this, mapLogic, completeActionEvent);
    }
}
