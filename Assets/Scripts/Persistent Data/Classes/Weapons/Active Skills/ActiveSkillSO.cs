using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SkillType
{
    ATTACK,
    STATUS_SUPPORT,
    HEAL_SUPPORT
}

[System.Serializable]
public struct InflictedStatusEffect
{
    public StatusEffectSO m_StatusEffect;
    public int m_Stack;
}

public abstract class ActiveSkillSO : ScriptableObject
{
    [Header("Details")]
    public string m_SkillName;
    public string m_Description;
    public Sprite m_Icon;
    // possible to have multiple skill types
    public SkillType[] m_SkillTypes;
    public List<Token> m_InflictedTokens;
    // TODO: If status effects cannot be inflicted at all without a token already being applied, then this can be removed
    public List<InflictedStatusEffect> m_InflictedStatusEffects;
    [Tooltip("used for different purposes: Multipliers for attacks and heal amount for heal skills")]
    public float m_Amount = 1f;
    [Tooltip("The amount of time after the animation for this skill starts that the response animation from targets should start playing")]
    public float m_DelayResponseAnimationTime = 0.2f;
    public float m_AnimationTime = 2f;

    [Header("Attack Config")]
    public List<SkillTargetRuleSO> m_TargetRules;

    [Header("Target")]
    [Tooltip("These are tiles that will also be targeted, represented as offsets from the target square")]
    public List<CoordPair> m_TargetSquares;

    // helpers
    public bool IsAoe => m_TargetSquares.Count > 0;
    public bool DealsDamage => ContainsAttackType(SkillType.ATTACK);
    public virtual bool IsMagic => true;
    public bool IsPhysicalAttack => !IsMagic && DealsDamage;
    public bool IsMagicAttack => IsMagic && DealsDamage;
    public bool IsSelfTarget => m_TargetRules.Any(x => x is LockToSelfTargetRuleSO);
    public bool IsOpposingSideTarget => !IsSelfTarget && m_TargetRules.Any(x => x is TargetOpposingSideRuleSO);
    // depends on whether attacks that target the opposing side but only deal status effects will still use the attack animation
    // public bool WillPlaySupportAnimation => !DealsDamage && !m_TargetRules.Any(x => x is TargetOpposingSideRuleSO);

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

    // does not check for occupied tiles, that is the responsibility of the grid logic
    public bool IsValidTargetTile(CoordPair targetTile, Unit unit, GridType targetGridType)
    {
        return m_TargetRules.All(x => x.IsValidTargetTile(targetTile, unit, targetGridType));
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
