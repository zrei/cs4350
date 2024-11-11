using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Wraps around the EnemyActionSO as a runtime instance for each unit
/// </summary>
public abstract class EnemyActionWrapper
{
    public EnemyActionSO m_Action;
    public int m_Priority;

    public abstract bool CanActionBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic);

    public abstract void PerformAction(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent completeActionEvent);
}

public class EnemyUnit : Unit
{
    public override UnitAllegiance UnitAllegiance => UnitAllegiance.ENEMY;

    private List<(EnemyActionCondition, EnemyActionWrapper)> m_OrderedConditions;
    private List<EnemyActionWrapper> m_OrderedActions;

    public event Action<EnemyActionWrapper> OnDecideAction;
    public EnemyActionWrapper NextAction
    {
        get => nextAction;
        private set
        {
            if (nextAction == value) return;

            nextAction = value;
            OnDecideAction?.Invoke(nextAction);
        }
    }
    private EnemyActionWrapper nextAction;

    public void Initialise(Stats statAugments, EnemyCharacterSO enemyCharacterSO, List<InflictedToken> permanentTokens)
    {
        CharacterName = enemyCharacterSO.m_CharacterName;
        base.Initialise(enemyCharacterSO.m_Stats.FlatAugment(statAugments), enemyCharacterSO.m_Race, enemyCharacterSO.m_EnemyClass, enemyCharacterSO.m_CharacterSprite, enemyCharacterSO.GetUnitModelData(), enemyCharacterSO.m_EquippedWeapon, permanentTokens);
        InitialiseActions(enemyCharacterSO.EnemyActionSetSO);
    }

    private void InitialiseActions(EnemyActionSetSO enemyActionSetSO)
    {
        m_OrderedConditions = new();
        m_OrderedActions = new();
        foreach (EnemyAction enemyAction in enemyActionSetSO.m_EnemyActions)
        {
            EnemyActionWrapper enemyActionWrapper = enemyAction.EnemyActionWrapper;
            foreach (EnemyActionCondition condition in enemyAction.m_Conditions)
            {
                m_OrderedConditions.Add((condition, enemyActionWrapper));
            }
            m_OrderedActions.Add(enemyActionWrapper);
        }
        m_OrderedConditions.Sort((x, y) => y.Item1.m_Priority.CompareTo(x.Item1.m_Priority));
        m_OrderedActions.Sort((x, y) => y.m_Priority.CompareTo(x.m_Priority));
    }

    // can cache action
    public EnemyActionWrapper GetActionToBePerformed(MapLogic mapLogic)
    {
        HashSet<EnemyActionWrapper> unperformableActions = new();

        foreach ((EnemyActionCondition condition, EnemyActionWrapper action) in m_OrderedConditions)
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
                NextAction = action;
                return action;
            }
        }

        // if no condition has been met, retrieve an action according to their base priority and if it can be performed
        // Pass action can always be performed
        foreach (EnemyActionWrapper enemyActionWrapper in m_OrderedActions)
        {
            if (unperformableActions.Contains(enemyActionWrapper))
                continue;

            if (!enemyActionWrapper.CanActionBePerformed(this, mapLogic))
            {
                continue;
            }

            NextAction = enemyActionWrapper;
            return enemyActionWrapper;
        }

        NextAction = new EnemyPassActionWrapper {m_Action = new EnemyPassActionSO()};
        return NextAction;
    }

    public void PerformAction(MapLogic mapLogic, VoidEvent completeActionEvent)
    {
        if (NextAction == null)
        {
            GetActionToBePerformed(mapLogic);
        }
        NextAction.PerformAction(this, mapLogic, completeActionEvent);
    }

    public override IEnumerable<ActiveSkillSO> GetActiveSkills()
    {
        var result = new List<ActiveSkillSO>();
        foreach (var enemyAction in m_OrderedActions)
        {
            if (enemyAction.m_Action is not EnemyActiveSkillActionSO activeSkillAction) continue;
            result.Add(activeSkillAction.m_ActiveSkill);
        }
        return result;
    }
}
