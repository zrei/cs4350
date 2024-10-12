using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class EnemyActiveSkillActionWrapper : EnemyActionWrapper
{
    public IEnumerable<CoordPair> PossibleAttackPositions => m_PossibleAttackPositionsIgnoreOccupied;

    private List<CoordPair> m_PossibleAttackPositions;
    private List<CoordPair> m_PossibleAttackPositionsIgnoreOccupied;

    private CoordPair m_Target;

    private EnemyActiveSkillActionSO ActiveSkillAction => (EnemyActiveSkillActionSO) m_Action;
    private ActiveSkillSO ActiveSkill => ActiveSkillAction.m_ActiveSkill;
    public GridType TargetGridType => ActiveSkillAction.TargetGridType;

    public override bool CanActionBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        ActiveSkillSO activeSKill = ActiveSkill;

        if (activeSKill.m_ConsumedMana > enemyUnit.CurrentMana)
            return false;

        if (activeSKill.IsSelfTarget)
            return true;

        m_PossibleAttackPositions = new();
        m_PossibleAttackPositionsIgnoreOccupied = new();
        bool hasPossibleAttackPosition = false;

        GridType targetGridType = TargetGridType;
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                CoordPair coordinates = new CoordPair(r, c);
                if (mapLogic.IsValidSkillTargetTile(activeSKill, enemyUnit, coordinates, targetGridType, true))
                {
                    m_PossibleAttackPositions.Add(coordinates);
                    hasPossibleAttackPosition = true;
                }
                if (mapLogic.IsValidSkillTargetTile(activeSKill, enemyUnit, coordinates, targetGridType, false))
                {
                    m_PossibleAttackPositionsIgnoreOccupied.Add(coordinates);
                }
            }
        }

        return hasPossibleAttackPosition;
    }

    // preparation for caching
    public void CalculateMovementPosition(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        ActiveSkillSO activeSKill = ActiveSkill;

        if (activeSKill.IsSelfTarget)
        {
            m_Target = enemyUnit.CurrPosition;
            return;
        }

        float baseWeight = 1f / m_PossibleAttackPositions.Count;

        List<(CoordPair, float)> targetWeights = m_PossibleAttackPositions.Select(x => (x, baseWeight)).ToList();

        for (int i = 0; i < targetWeights.Count; ++i)
        {
            (CoordPair target, float weight) = targetWeights[i];
            float finalNodeWeight = weight * ActiveSkillAction.GetFinalWeightProportionForTile(enemyUnit, mapLogic, target);
            targetWeights[i] = (target, finalNodeWeight);
        }

        m_Target = RandomHelper.GetRandomT(targetWeights);
    }

    public override void PerformAction(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent completeActionEvent)
    {
        ActiveSkillSO activeSKill = ActiveSkill;
        var attackDelay = 1.5f;
        IEnumerator PlayActionWithAnimation()
        {
            int animationTrigger = 0;
            animationTrigger += (int)(ActiveSkill.m_OverrideWeaponAnimationType ? ActiveSkill.m_OverriddenWeaponAnimationType : enemyUnit.WeaponAnimationType);
            animationTrigger += (int)ActiveSkill.m_SkillAnimationType;

            enemyUnit.PlaySkillStartAnimation(animationTrigger);

            if (ActiveSkill.IsSelfTarget)
            {
                mapLogic.ShowAttackForecast(GridType.ENEMY, new List<CoordPair>() { enemyUnit.CurrPosition });
                yield return new WaitForSeconds(attackDelay);
                mapLogic.ShowAttackForecast(GridType.ENEMY, new List<CoordPair>() { });

                mapLogic.PerformSkill(TargetGridType, enemyUnit, ActiveSkill, enemyUnit.CurrPosition, completeActionEvent);
                yield break;
            }

            float baseWeight = 1f / m_PossibleAttackPositions.Count;

            List<(CoordPair, float)> targetWeights = m_PossibleAttackPositions.Select(x => (x, baseWeight)).ToList();

            for (int i = 0; i < targetWeights.Count; ++i)
            {
                (CoordPair target, float weight) = targetWeights[i];
                float finalNodeWeight = weight * ActiveSkillAction.GetFinalWeightProportionForTile(enemyUnit, mapLogic, target);
                targetWeights[i] = (target, finalNodeWeight);
            }

            CoordPair targetTile = RandomHelper.GetRandomT(targetWeights);

            mapLogic.ShowAttackForecast(GridType.PLAYER, new List<CoordPair>() { targetTile });
            yield return new WaitForSeconds(attackDelay);
            mapLogic.ShowAttackForecast(GridType.PLAYER, new List<CoordPair>() { });

            mapLogic.PerformSkill(TargetGridType, enemyUnit, ActiveSkill, targetTile, completeActionEvent);
        }
        CoroutineManager.Instance.StartCoroutine(PlayActionWithAnimation());
    }
}


[CreateAssetMenu(fileName = "EnemyActiveSkillActionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Actions/EnemyActiveSkillActionSO")]
public class EnemyActiveSkillActionSO : EnemyTargetActionSO
{
    public ActiveSkillSO m_ActiveSkill;

    public GridType TargetGridType => GridHelper.GetTargetType(m_ActiveSkill, UnitAllegiance.ENEMY);
    
    public override EnemyActionWrapper GetWrapper(int priority)
    {
        return new EnemyActiveSkillActionWrapper {m_Action = this, m_Priority = priority};
    }

    /*
    public override bool CanActionBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        if (m_ActiveSkill is MagicActiveSkillSO && enemyUnit.RemainingMana < ((MagicActiveSkillSO) m_ActiveSkill).m_ConsumedManaAmount)
            return false;

        if (m_ActiveSkill.IsSelfTarget)
            return true;
        
        GridType targetGridType = TargetGridType;

        bool hasPossibleAttackPosition = false;
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                CoordPair coordinates = new CoordPair(r, c);
                if (m_ActiveSkill.IsValidTargetTile(coordinates, enemyUnit, targetGridType) && mapLogic.IsTileOccupied(targetGridType, coordinates))
                {
                    m_PossibleAttackPositions.Add(coordinates);
                    hasPossibleAttackPosition = true;
                }
            }
        }

        return hasPossibleAttackPosition;
    }
    */

    /*
    public List<ActiveSkillCondition> m_ActiveSkillConditions;

    //private GridType TargetGridType => GridHelper.GetTargetType(m_ActiveSkill, UnitAllegiance.ENEMY);

    private List<(ActiveSkillSO, CoordPair)> m_PossibleAttacks;


    private GridType GetTargetGridType(ActiveSkillSO activeSkill)
    {
        return GridHelper.GetTargetType(activeSkill, UnitAllegiance.ENEMY);
    }

    private bool CanActiveSkillBePerformed(ActiveSkillSO activeSkill, EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        if (activeSkill is MagicActiveSkillSO && enemyUnit.RemainingMana < ((MagicActiveSkillSO) activeSkill).m_ConsumedManaAmount)
            return false;

        if (activeSkill.IsSelfTarget)
            return true;
        
        GridType targetGridType = GetTargetGridType(activeSkill);

        bool hasPossibleAttackPosition = false;
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                CoordPair coordinates = new CoordPair(r, c);
                if (activeSkill.IsValidTargetTile(coordinates, enemyUnit, targetGridType) && mapLogic.IsTileOccupied(targetGridType, coordinates))
                {
                    m_PossibleAttacks.Add((activeSkill, coordinates));
                    hasPossibleAttackPosition = true;
                }
            }
        }

        return hasPossibleAttackPosition;
    }

    public override bool CanActionBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        m_PossibleAttacks = new();
        bool canPerformAnyAttack = false;
        foreach (ActiveSkillSO activeSkill in enemyUnit.GetAvailableActiveSkills())
        {
            if (CanActiveSkillBePerformed(activeSkill, enemyUnit, mapLogic))
            {
                canPerformAnyAttack = true;
            }
        }
        return canPerformAnyAttack;
    }

    public override void PerformAction(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent completeActionEvent)
    {
        float initialWeight = 1f / m_PossibleAttacks.Count;
        List<((ActiveSkillSO skill, CoordPair coordinates) val, float weight)> finalWeights = m_PossibleAttacks.Select(x => (x, initialWeight)).ToList();
        
        IEnumerable<ActiveSkillCondition> metConditions = m_ActiveSkillConditions.Where(x => x.IsConditionMet(enemyUnit, mapLogic));

        float finalStatusSupportSkillProportion = 1f;
        float finalHealSkillProportion = 1f;
        float finalAttackSkillProportion = 1f;
        foreach (ActiveSkillCondition condition in metConditions)
        {
            switch (condition.m_AffectedSkillType)
            {
                case SkillType.ATTACK:
                    finalAttackSkillProportion *= condition.m_MultProportion;
                    break;
                case SkillType.HEAL_SUPPORT:
                    finalHealSkillProportion *= condition.m_MultProportion;
                    break;
                case SkillType.STATUS_SUPPORT:
                    finalStatusSupportSkillProportion *= condition.m_MultProportion;
                    break;
            }
        }

        for (int i = 0; i < finalWeights.Count; ++i)
        {
            ActiveSkillSO skill = finalWeights[i].val.skill;
            //CoordPair 
            float finalWeight = finalWeights[i].weight;
            if (skill.ContainsAttackType(SkillType.ATTACK))
            {
                finalWeight *= finalAttackSkillProportion;
            }

            if (skill.ContainsAttackType(SkillType.STATUS_SUPPORT))
            {
                finalWeight *= finalStatusSupportSkillProportion;
            }

            if (skill.ContainsAttackType(SkillType.HEAL_SUPPORT))
            {
                finalWeight *= finalHealSkillProportion;
            }

            finalWeights[i] = (finalWeights[i].val, finalWeight);
        }

        (ActiveSkillSO activeSkill, CoordPair coordinates) = RandomHelper.GetRandomT(finalWeights);
        Logger.Log(GetType().Name, $"Choose active skill: {activeSkill.name}", LogLevel.LOG);
        mapLogic.PerformSkill(GetTargetGridType(activeSkill), enemyUnit, activeSkill, coordinates, completeActionEvent);
    }
    */
}
