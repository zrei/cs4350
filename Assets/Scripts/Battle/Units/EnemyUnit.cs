using System;
using System.Collections.Generic;

/// <summary>
/// Wraps around the EnemyActionSO as a runtime instance for each unit
/// </summary>
public abstract class EnemyActionWrapper : IConcreteAction
{
    public EnemyActionSO m_Action;

    public bool IsCompleted()
    {
        return true;
    }

    public void Reset()
    {
        // pass
    }

    public EnemyActionWrapper GetActionToBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return this;
    }

    public abstract void Run(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent onCompleteAction);

    public abstract bool ShouldBreakOut(EnemyUnit enemyUnit, MapLogic mapLogic);

    public abstract HashSet<ActiveSkillSO> GetNestedActiveSkills();
}

public class EnemyUnit : Unit
{
    public override UnitAllegiance UnitAllegiance => UnitAllegiance.ENEMY;
    public EnemyTag m_EnemyTags = EnemyTag.None;

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

    private BehaviourTree m_TopLevelBehaviour;

    public void Initialise(Stats statAugments, EnemyCharacterSO enemyCharacterSO, List<InflictedToken> permanentTokens)
    {
        CharacterSOInstanceID = enemyCharacterSO.GetInstanceID();
        CharacterName = enemyCharacterSO.m_CharacterName;
        base.Initialise(enemyCharacterSO.m_Stats.FlatAugment(statAugments), enemyCharacterSO.m_Race, enemyCharacterSO.m_EnemyClass, enemyCharacterSO.m_CharacterSprite, enemyCharacterSO.GetUnitModelData(), enemyCharacterSO.m_EquippedWeapon, permanentTokens);
        InitialiseActions(enemyCharacterSO.EnemyActionSetSO);
    }

    private void InitialiseActions(BehaviourTreeSO behaviourTreeSO)
    {
        m_TopLevelBehaviour = new(behaviourTreeSO);
        
        /*
        m_OrderedConditions = new();
        m_OrderedActions = new();
        List<ActiveSkillSO> activeSkills = new();
        foreach (EnemyAction enemyAction in enemyActionSetSO.m_EnemyActions)
        {
            EnemyActionWrapper enemyActionWrapper = enemyAction.EnemyActionWrapper;
            foreach (EnemyActionCondition condition in enemyAction.m_Conditions)
            {
                m_OrderedConditions.Add((condition, enemyActionWrapper));
            }
            m_OrderedActions.Add(enemyActionWrapper);

            if (enemyAction.m_EnemyAction is EnemyActiveSkillActionSO)
            {
                activeSkills.Add(((EnemyActiveSkillActionSO) enemyAction.m_EnemyAction).m_ActiveSkill);
            }
        }
        m_OrderedConditions.Sort((x, y) => y.Item1.m_ActionIndex.CompareTo(x.Item1.m_ActionIndex));
        m_OrderedActions.Sort((x, y) => y.m_Priority.CompareTo(x.m_Priority));
        */
        m_SkillCooldownTracker.SetSkills(m_TopLevelBehaviour.GetNestedActiveSkills());
    }

    // can cache action
    public EnemyActionWrapper GetActionToBePerformed(MapLogic mapLogic)
    {
        if (m_TopLevelBehaviour.ShouldBreakOut(this, mapLogic))
        {
            m_TopLevelBehaviour.Reset();
            NextAction = new EnemyPassActionWrapper {m_Action = new EnemyPassActionSO()};
            return NextAction;
        }

        NextAction = m_TopLevelBehaviour.GetActionToBePerformed(this, mapLogic);
        return NextAction;

        /*
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
        */
    }

    public void PerformAction(MapLogic mapLogic, VoidEvent completeActionEvent)
    {
        m_TopLevelBehaviour.Run(this, mapLogic, completeActionEvent);
        
        /*
        if (NextAction == null)
        {
            GetActionToBePerformed(mapLogic);
        }
        NextAction.PerformAction(this, mapLogic, completeActionEvent);
        */
    }

    public override IEnumerable<ActiveSkillSO> GetActiveSkills()
    {
        return m_TopLevelBehaviour.GetNestedActiveSkills();
    }
}
