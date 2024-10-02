using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit
{
    public override UnitAllegiance UnitAllegiance => UnitAllegiance.ENEMY;

    private List<(EnemyActionCondition, EnemyActionSO)> m_OrderedConditions;
    private List<EnemyAction> m_OrderedActions;

    #region Caching
    private EnemyActionSO m_CachedAction;
    // cached attack targets
    private Dictionary<EnemyActiveSkillActionSO, CoordPair> m_CachedAttackTargets;
    // cached movement target
    private PathNode m_CachedMoveTarget = null!;
    #endregion

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
            m_OrderedActions.Add(enemyAction);
        }
        m_OrderedConditions.Sort((x, y) => y.Item1.m_Priority.CompareTo(x.Item1.m_Priority));
        m_OrderedActions.Sort((x, y) => y.m_BasePriority.CompareTo(x.m_BasePriority));
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
        foreach (EnemyAction action in m_OrderedActions)
        {
            EnemyActionSO enemyActionSO = action.m_EnemyAction;
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
        EnemyActionSO enemyActionSO = GetActionToBePerformed(mapLogic);
        
        switch (enemyActionSO.EnemyActionType)
        {
            case EnemyActionType.PASS:
                ((EnemyPassActionSO) enemyActionSO).PassTurn(completeActionEvent);
                break;
            case EnemyActionType.SKILL:
                EnemyActiveSkillActionSO skill = (EnemyActiveSkillActionSO) enemyActionSO;
                // it's this convoluted cos the caching hasn't been put into place yet
                CoordPair targetTile = skill.CalculateAttackPosition(this, mapLogic);
                skill.PerformSkill(this, mapLogic, targetTile, completeActionEvent);
                break;
            case EnemyActionType.MOVE:
                EnemyMoveActionSO move = (EnemyMoveActionSO) enemyActionSO;
                PathNode movementTarget = move.CalculateMovementPosition(this, mapLogic);
                move.PerformMove(this, mapLogic, movementTarget, completeActionEvent);
                break;
        }
        
        //.PerformAction(this, mapLogic, completeActionEvent);
    }
}
