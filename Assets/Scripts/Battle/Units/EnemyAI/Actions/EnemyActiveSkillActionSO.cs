using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "EnemyActiveSkillActionSO", menuName = "ScriptableObject/Battle/EnemyAI/Actions/EnemyActiveSkillActionSO")]
public class EnemyActiveSkillActionSO : EnemyActionSO
{
    public ActiveSkillSO m_ActiveSkill;
    public List<EnemyTileCondition> m_TargetConditions;

    private GridType TargetGridType => GridHelper.GetTargetType(m_ActiveSkill, UnitAllegiance.ENEMY);

    private List<CoordPair> m_PossibleAttackPositions;

    public override bool CanActionBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        if (m_ActiveSkill is MagicActiveSkillSO && enemyUnit.CurrentMana < ((MagicActiveSkillSO) m_ActiveSkill).m_ConsumedManaAmount)
            return false;

        if (m_ActiveSkill.IsSelfTarget)
            return true;
        
        GridType targetGridType = TargetGridType;

        m_PossibleAttackPositions = new();
        bool hasPossibleAttackPosition = false;
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                CoordPair coordinates = new CoordPair(r, c);
                if (mapLogic.IsValidSkillTargetTile(m_ActiveSkill, enemyUnit, coordinates, TargetGridType, true))
                {
                    m_PossibleAttackPositions.Add(coordinates);
                    hasPossibleAttackPosition = true;
                }
            }
        }

        return hasPossibleAttackPosition;
    }

    public override void PerformAction(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent completeActionEvent)
    {
        if (m_ActiveSkill.IsSelfTarget)
        {
            mapLogic.PerformSkill(TargetGridType, enemyUnit, m_ActiveSkill, enemyUnit.CurrPosition, completeActionEvent);
            return;
        }

        float baseWeight = 1f / m_PossibleAttackPositions.Count;

        List<(CoordPair, float)> targetWeights = m_PossibleAttackPositions.Select(x => (x, baseWeight)).ToList();

        for (int i = 0; i < targetWeights.Count; ++i)
        {
            (CoordPair target, float weight) = targetWeights[i];
            float finalNodeWeight = weight;

            foreach (EnemyTileCondition targetCondition in m_TargetConditions)
            {
                if (targetCondition.IsConditionMet(enemyUnit, mapLogic, target))
                    finalNodeWeight *= targetCondition.m_MultProportion;
            }

            targetWeights[i] = (target, finalNodeWeight);
        }

        CoordPair targetTile = RandomHelper.GetRandomT(targetWeights);

        mapLogic.PerformSkill(TargetGridType, enemyUnit, m_ActiveSkill, targetTile, completeActionEvent);
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
