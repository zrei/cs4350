using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    PHYSICAL_ATTACK,
    MAGICAL_ATTACK,
    STATUS_SUPPORT,
    HEAL_SUPPORT
}

public abstract class ActiveSkillSO : ScriptableObject
{
    public SkillType m_AttackType;
 
    [Header("Details")]
    public string m_SkillName;
    public string m_Description;
    public Sprite m_Icon;

    [Header("Attack Config")]
    [Tooltip("Whether this attack can only target a specific row")]
    public bool m_LockTargetRow;
    [Tooltip("Will be ignored if target rows are not locked")]
    public List<int> m_AllowedTargetRows;

    [Tooltip("Whether this attack can only target a specific col")]
    public bool m_LockTargetCol;
    [Tooltip("will be ignored if target cols are not locked")]
    public List<int> m_AllowedTargetCols;

    // TODO: This seems more like a support skill thing...
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

    protected abstract bool AllowedGridTypes(Unit unit, GridType targetGridType);

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
}

public class PhysicalAttackSkillSO : ActiveSkillSO, IAttack {
    public bool IsPhysical()
    {
        return true;
    }

    protected override bool AllowedGridTypes(Unit unit, GridType targetGridType)
    {
        if (unit.UnitAllegiance == UnitAllegiance.PLAYER)
            return targetGridType == GridType.ENEMY;
        else if (unit.UnitAllegiance == UnitAllegiance.ENEMY)
            return targetGridType == GridType.PLAYER;
        return false;
    }
}

public abstract class MagicSkillSO : ActiveSkillSO
{
    public float m_ManaCost;
}

public abstract class SupportSkillSO : MagicSkillSO
{
    public bool m_AllowSelfTarget;

    protected override bool AllowedGridTypes(Unit unit, GridType targetGridType)
    {
        if (unit.UnitAllegiance == UnitAllegiance.PLAYER)
            return targetGridType == GridType.PLAYER;
        else if (unit.UnitAllegiance == UnitAllegiance.ENEMY)
            return targetGridType == GridType.ENEMY;
        return false;
    }
}

public class HealSkillSO : SupportSkillSO
{
    public float m_HealAmount;
}

public class StatusSupportSkillSO : SupportSkillSO
{
    public List<StatusEffect> m_InflictedStatusEffects;
    public List<Token> m_InflictedTokens;
}

public class MagicAttackSkillSO : MagicSkillSO, IAttack {
    public bool IsPhysical()
    {
        return false;
    }

    protected override bool AllowedGridTypes(Unit unit, GridType targetGridType)
    {
        if (unit.UnitAllegiance == UnitAllegiance.PLAYER)
            return targetGridType == GridType.ENEMY;
        else if (unit.UnitAllegiance == UnitAllegiance.ENEMY)
            return targetGridType == GridType.PLAYER;
        return false;
    }
}

public interface IAttack
{
    public bool IsPhysical();
}