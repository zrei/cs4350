using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SkillType
{
    PHYSICAL_ATTACK,
    MAGICAL_ATTACK,
    STATUS_SUPPORT,
    HEAL_SUPPORT
}

[CreateAssetMenu(fileName = "ActiveSkillSO", menuName = "ScriptableObject/ActiveSKillSO")]
public class ActiveSkillSO : ScriptableObject
{
    [Header("Details")]
    public string m_SkillName;
    public string m_Description;
    public Sprite m_Icon;
    public bool m_CastOnOppositeType;
    public SkillType[] m_SkillTypes;
    public List<Token> m_InflictedTokens;
    public List<StatusEffect> m_InflictedStatusEffects;
    // used for different purposes: Multipliers for attacks and heal amount for heal skills
    public float m_Amount = 1f;
    public float m_ConsumedManaAmount = 0f;

    [Header("Attack Config")]
    [Tooltip("Whether this attack can only target a specific row")]
    public bool m_LockTargetRow;
    [Tooltip("Will be ignored if target rows are not locked")]
    public List<int> m_AllowedTargetRows;

    [Tooltip("Whether this attack can only target a specific col")]
    public bool m_LockTargetCol;
    [Tooltip("will be ignored if target cols are not locked")]
    public List<int> m_AllowedTargetCols;

    // NOTE: Target squares can be on the same side as the caster
    [Tooltip("Whether the allowed target square is locked by range")]
    public bool m_LockTargetRange;
    [Tooltip("Will be ignored if target range is not locked")]
    public int m_AllowedTargetRange;

    [Tooltip("Whether this attack can only be initiated when the attacker is on a specific row")]
    public bool m_LockAttackerRow;
    [Tooltip("Will be ignored if attacker rows are not locked")]
    public List<int> m_AllowedAttackerRows;

    [Tooltip("Whether this attack can only be initiated when the attacker is on a specific col")]
    public bool m_LockAttackerCol;
    [Tooltip("Will be ignored if attacker cols are not locked")]
    public List<int> m_AllowedAttackerCols;

    [Header("Target")]
    [Tooltip("These are tiles that will also be targeted, represented as offsets from the target square")]
    public List<CoordPair> m_TargetSquares;

    public bool IsAoe => m_TargetSquares.Count > 0;
    public bool DealsDamage => ContainsAnyAttackType(SkillType.PHYSICAL_ATTACK, SkillType.MAGICAL_ATTACK);

    public bool ContainsAttackType(SkillType skillType)
    {
        return m_SkillTypes.Contains(skillType);
    }

    public bool ContainsAllAttackTypes(params SkillType[] skillTypes)
    {
        return skillTypes.All(x => ContainsAttackType(x));
    }

    public bool ContainsAnyAttackType(params SkillType[] skillTypes)
    {
        return skillTypes.Any(x => ContainsAttackType(x));
    }

    private bool AllowedGridTypes(Unit unit, GridType targetGridType)
    {
        if (m_CastOnOppositeType)
        {
            return (unit.UnitAllegiance == UnitAllegiance.PLAYER && targetGridType == GridType.ENEMY) || (unit.UnitAllegiance == UnitAllegiance.ENEMY && targetGridType == GridType.PLAYER);
        }
        else
        {
            return (unit.UnitAllegiance == UnitAllegiance.ENEMY && targetGridType == GridType.ENEMY) || (unit.UnitAllegiance == UnitAllegiance.PLAYER && targetGridType == GridType.PLAYER);
        }
    }

    public bool IsValidTargetTile(CoordPair targetTile, Unit unit, GridType targetGridType)
    {
        if (!AllowedGridTypes(unit, targetGridType))
            return false;

        if (m_LockTargetRow)
        {
            if (!m_AllowedTargetRows.Contains(targetTile.m_Row))
                return false;
        }

        if (m_LockTargetCol)
        {
            if (!m_AllowedTargetCols.Contains(targetTile.m_Col))
                return false;
        }

        if (m_LockAttackerRow)
        {
            if (!m_AllowedAttackerRows.Contains(unit.CurrPosition.m_Row))
                return false;
        }

        if (m_LockAttackerCol)
        {
            if (!m_AllowedAttackerCols.Contains(unit.CurrPosition.m_Col))
                return false;
        }

        if (m_LockTargetRange)
        {
            int manhattenDistance = unit.CurrPosition.GetDistanceToPoint(targetTile);
            if (manhattenDistance > m_AllowedTargetRange)
                return false;
        }

        return true;
    }

    public List<CoordPair> ConstructAttackTargetTiles(CoordPair target)
    {
        List<CoordPair> attackTargetTiles = new() {target};

        foreach (CoordPair offset in m_TargetSquares)
        {
            attackTargetTiles.Add(target.Offset(offset));
        }

        return attackTargetTiles;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (ContainsAttackType(SkillType.PHYSICAL_ATTACK) && ContainsAttackType(SkillType.MAGICAL_ATTACK))
            Logger.Log(this.GetType().Name, $"Active skill SO {name} is both a physical and magical attack", LogLevel.ERROR);
    }
#endif
}