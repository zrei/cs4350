using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct InflictedStatusEffect
{
    public StatusEffectSO m_StatusEffect;
    public int m_Stack;
}

[CreateAssetMenu(fileName = "ActiveSkillSO", menuName = "ScriptableObject/ActiveSkills/ActiveSkillSO")]
public class ActiveSkillSO : ScriptableObject
{
    [Header("Details")]
    public string m_SkillName;
    [TextArea]
    public string m_Description;
    public Sprite m_Icon;
    [Tooltip("Will determine which stats to use in calculating damage")]
    public SkillType m_SkillType;
    [Tooltip("Amount of mana to consume to utilise this skill. Leave as 0 if this does not consume mana")]
    public float m_ConsumedMana = 0f;

    [Header("Effects")]
    [Tooltip("Determines what the skill does upon being activated")]
    public SkillEffectType[] m_SkillTypes;

    [Space]
    // status
    [Tooltip("Tokens to inflict on target - only used if skill inflicts status or token")]
    public List<InflictedToken> m_InflictedTokens;
    // TODO: If status effects cannot be inflicted at all without a token already being applied, then this can be removed
    [Tooltip("Status effects to inflict on target - only used if skill inflicts status or token")]
    public List<InflictedStatusEffect> m_InflictedStatusEffects;

    [Space]
    // damage
    [Tooltip("Determines the base attack modifier for damage - only used if skill deals damage")]
    [Range(0f, 1f)]
    public float m_DamageModifier = 1f;

    [Space]
    // healing
    [Tooltip("Determines the proportion of health to heal from magic attack - only used if skill heals")]
    public float m_HealProportion = 1f;

    [Space]
    [Tooltip("Determines the amount of mana to add to the target - only used if skill alters mana")]
    public float m_ManaAlterAmount = 0f;

    [Space]
    [Tooltip("Adds to summon upon attack - only used if skill summons")]
    public List<EnemyUnitPlacement> m_Summons;
    
    [Header("Animations")]
    [Tooltip("The amount of time after the animation for this skill starts that the response animation from targets should start playing")]
    public float m_DelayResponseAnimationTime = 0.2f;
    public float m_AnimationTime = 2f;
    public bool m_TargetWillPlayHurtAnimation = false;
    [Tooltip("Use this to override the weapon animation type instead of taking it from the character's weapon")]
    public bool m_OverrideWeaponAnimationType = false;
    [Tooltip("Ignored if animation type is not overridden")]
    public WeaponAnimationType m_OverriddenWeaponAnimationType;
    [Tooltip("What skill animation to play for this skill")]
    public SkillAnimationType m_SkillAnimationType = SkillAnimationType.ATTACK;

    [Header("Attack Config")]
    public List<SkillTargetRuleSO> m_TargetRules;

    [Header("Target")]
    [Tooltip("These are tiles that will also be targeted, represented as offsets from the target square")]
    public List<CoordPair> m_TargetSquares;

    #region Helpers
    public bool IsAoe => m_TargetSquares.Count > 0;
    public bool DealsDamage => ContainsSkillType(SkillEffectType.DEALS_DAMAGE);
    public bool IsHeal => ContainsSkillType(SkillEffectType.HEAL);
    public bool IsMagic => m_SkillType == SkillType.MAGIC;
    public bool IsPhysicalAttack => !IsMagic && DealsDamage;
    public bool IsMagicAttack => IsMagic && DealsDamage;
    public bool IsSelfTarget => m_TargetRules.Any(x => x is LockToSelfTargetRuleSO);
    public bool IsOpposingSideTarget => !IsSelfTarget && m_TargetRules.Any(x => x is TargetOpposingSideRuleSO);
    // depends on whether attacks that target the opposing side but only deal status effects will still use the attack animation
    // public bool WillPlaySupportAnimation => !DealsDamage && !m_TargetRules.Any(x => x is TargetOpposingSideRuleSO);
    #endregion

    public bool ContainsSkillType(SkillEffectType skillType)
    {
        return m_SkillTypes.Contains(skillType);
    }

    public bool ContainsAllSkillTypes(params SkillEffectType[] skillTypes)
    {
        return skillTypes.All(x => ContainsSkillType(x));
    }

    public bool ContainsAnyAttackType(params SkillEffectType[] skillTypes)
    {
        return skillTypes.Any(x => ContainsSkillType(x));
    }

    // does not check for occupied tiles, that is the responsibility of the grid logic
    public bool IsValidTargetTile(CoordPair targetTile, Unit unit, GridType targetGridType)
    {
        if (unit.IsTaunted(out Unit forceTarget) && !ConstructAttackTargetTiles(targetTile).Contains(forceTarget.CurrPosition))
            return false;
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        HashSet<CoordPair> previousTargets = new();
        foreach (CoordPair coordPair in m_TargetSquares)
        {
            if (coordPair.Equals(new CoordPair(0, 0)))
            {
                Logger.Log(this.GetType().Name, $"Target tiles for {name} repeats origin", LogLevel.WARNING);
            }
            else if (previousTargets.Contains(coordPair))
            {
                Logger.Log(this.GetType().Name, $"Repeated target tile: {coordPair} for {name}", LogLevel.WARNING);
            }
            else
            {
                previousTargets.Add(coordPair);
            }
        }

        foreach (InflictedToken inflictedToken in m_InflictedTokens)
        {
            if (inflictedToken.m_Tier > inflictedToken.m_TokenTierData.NumTiers || inflictedToken.m_Tier <= 0)
                Logger.Log(this.GetType().Name, $"Invalid tier {inflictedToken.m_Tier} for token tier {inflictedToken.m_TokenTierData.m_Id} for {name}", LogLevel.ERROR);
        }
    }
#endif
}
