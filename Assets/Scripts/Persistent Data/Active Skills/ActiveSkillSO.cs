using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[System.Serializable]
public struct InflictedStatusEffect
{
    public StatusEffectSO m_StatusEffect;
    public int m_Stack;

    public override string ToString()
    {
        return $"{m_Stack}x <color=#{ColorUtility.ToHtmlStringRGB(m_StatusEffect.m_Color)}>{m_StatusEffect}</color>";
    }
}

[System.Serializable]
public class SkillFX
{
    public enum AttachmentType
    {
        WeaponModel,
        Caster,
        Target,
    }

    public AttachmentType m_AttachmentType;

    public int m_WeaponModelIndex;
    public int m_AttachPointIndex;
    // todo: refactor this to feedback system
    public ParticleSystem m_FeedbackSystemPrefab;

    public void Play(Unit caster, Unit target)
    {
        if (m_FeedbackSystemPrefab == null) return;

        Transform attachPoint = null;
        switch (m_AttachmentType)
        {
            case AttachmentType.WeaponModel:
                var weaponModels = caster.WeaponModels;
                var weaponModelIndex = Mathf.Clamp(m_WeaponModelIndex, 0, weaponModels.Count);
                if (weaponModelIndex < 0 || weaponModelIndex >= weaponModels.Count) return;
                var weaponModel = weaponModels[weaponModelIndex];

                var attachPoints = weaponModel.fxAttachPoints;
                var attachPointIndex = Mathf.Clamp(m_AttachPointIndex, 0, attachPoints.Count);
                if (attachPointIndex < 0 || attachPointIndex >= attachPoints.Count) return;
                attachPoint = attachPoints[attachPointIndex];
                break;
            case AttachmentType.Caster:
                attachPoint = caster?.transform;
                break;
            case AttachmentType.Target:
                attachPoint = target?.transform;
                break;
        }

        if (attachPoint == null) return;

        var fx = Object.Instantiate(m_FeedbackSystemPrefab, attachPoint);
        IEnumerator PlayAndDispose()
        {
            fx.Play();
            while (fx.isPlaying)
            {
                yield return null;
            }
            Object.Destroy(fx.gameObject);
        }
        CoroutineManager.Instance.StartCoroutine(PlayAndDispose());
    }
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
    [Range(0f, 5f)]
    public float m_DamageModifier = 1f;

    [Space]
    // healing
    [Tooltip("Determines the proportion of health to heal from magic attack - only used if skill heals")]
    public float m_HealProportion = 1f;

    [Space]
    [Tooltip("Determines the proportion of mana to add to the target from magic attack - only used if skill alters mana")]
    public float m_ManaAlterProportion = 0f;

    [Space]
    [Tooltip("Adds to summon upon attack - only used if skill summons")]
    public List<SummonWrapper> m_Summons;

    [Space]
    [Tooltip("Rules governing where the target can be teleported to - should generally be location checks for target")]
    public List<TargetLocationRuleSO> m_TeleportTargetRules;
    
    [Header("Animations")]
    [Tooltip("The amount of time after the animation for this skill starts that the response animation from targets should start playing")]
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
    public TargetSO m_TargetSO;

    [Header("FX")]
    public List<SkillFX> m_SkillFXs;

    #region Helpers
    public bool IsAoe => m_TargetSO.IsAoe;
    public bool DealsDamage => ContainsSkillType(SkillEffectType.DEALS_DAMAGE);
    public bool IsHeal => ContainsSkillType(SkillEffectType.HEAL);
    public bool IsMagic => m_SkillType == SkillType.MAGIC;
    public bool IsPhysicalAttack => !IsMagic && DealsDamage;
    public bool IsMagicAttack => IsMagic && DealsDamage;
    public bool IsSelfTarget => m_TargetRules.Any(x => x is LockToSelfTargetRuleSO);
    public bool IsOpposingSideTarget => !IsSelfTarget && m_TargetRules.Any(x => x is TargetOpposingSideRuleSO);
    public bool HasAttackerLimitations => m_TargetRules.Any(x => x is IAttackerRule);
    // depends on whether attacks that target the opposing side but only deal status effects will still use the attack animation
    // public bool WillPlaySupportAnimation => !DealsDamage && !m_TargetRules.Any(x => x is TargetOpposingSideRuleSO);
    #endregion

    public string GetDescription(ICanAttack caster, IHealth target)
    {
        var builder = new StringBuilder();

        var skillTypesSet = new HashSet<SkillEffectType>(m_SkillTypes);
        if (skillTypesSet.Contains(SkillEffectType.DEALS_DAMAGE))
        {
            var dmgSpriteTag = m_SkillType switch
            {
                SkillType.PHYSICAL => "<sprite name=\"PhysicalAttack\" tint>",
                SkillType.MAGIC => "<sprite name=\"MagicAttack\" tint>",
                _ => string.Empty,
            };
            var dmgText = $"{(target != null ? DamageCalc.CalculateDamage(caster, target, this) : DamageCalc.CalculateDamage(caster, this)):F1}";
            builder.AppendLine($"DMG: {dmgText} {dmgSpriteTag}");
        }
        if (skillTypesSet.Contains(SkillEffectType.HEAL))
        {
            builder.AppendLine($"HEAL: {DamageCalc.CalculateHealAmount(caster, this):F1}");
        }
        if (skillTypesSet.Contains(SkillEffectType.ALTER_MANA))
        {
            builder.AppendLine($"ALTER MANA: {DamageCalc.CalculateManaAlterAmount(caster, this):F1}");
        }
        if (skillTypesSet.Contains(SkillEffectType.SUMMON))
        {
            builder.AppendLine($"Summon unit");
        }
        if (skillTypesSet.Contains(SkillEffectType.TELEPORT))
        {
            builder.AppendLine($"Teleport target");
        }
        if (skillTypesSet.Contains(SkillEffectType.DEALS_STATUS_OR_TOKENS))
        {
            builder.AppendLine("Applies:");
            foreach (var token in m_InflictedTokens)
            {
                builder.AppendLine(token.ToString());
            }
            foreach (var status in m_InflictedStatusEffects)
            {
                builder.AppendLine(status.ToString());
            }
        }

        if (!string.IsNullOrEmpty(m_Description))
        {
            builder.AppendLine(m_Description);
        }
        return builder.ToString();
    }

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

    /// <summary>
    /// Checks if target tile is valid
    /// Does not check for occupied tiles, that is the responsibility of the grid logic
    /// </summary>
    /// <param name="targetTile"></param>
    /// <param name="unit"></param>
    /// <param name="targetGridType"></param>
    /// <returns></returns>
    public bool IsValidTargetTile(CoordPair targetTile, Unit unit, GridType targetGridType)
    {
        if (unit.IsTaunted(out Unit forceTarget) && !ConstructAttackTargetTiles(targetTile).Contains(forceTarget.CurrPosition))
            return false;
        return m_TargetRules.Where(x => x is ITargetRule).All(x => ((ITargetRule) x).IsValidTargetTile(targetTile, unit, targetGridType));
    }

    /// <summary>
    /// Check if this particular attacker tile is valid
    /// </summary>
    /// <param name="unit">The attacker tile</param>
    /// <returns></returns>
    public bool IsValidAttackerTile(CoordPair attackerCoordinates)
    {
        return m_TargetRules.Where(x => x is IAttackerRule).All(x => ((IAttackerRule) x).IsValidAttackerTile(attackerCoordinates));
    }

    public bool IsValidTeleportTargetTile(CoordPair targetTile, Unit unit, GridType targetGridType)
    {
        if (IsOpposingSideTarget && !GridHelper.IsOpposingSide(unit.UnitAllegiance, targetGridType))
            return false;
        else if (!IsOpposingSideTarget && !GridHelper.IsSameSide(unit.UnitAllegiance, targetGridType))
            return false;
        return m_TeleportTargetRules.All(x => x.IsValidTargetTile(targetTile, unit, targetGridType));
    }

    public List<CoordPair> ConstructAttackTargetTiles(CoordPair target) => m_TargetSO.ConstructAttackTargetTiles(target);

#if UNITY_EDITOR
    private void OnValidate()
    {
        foreach (InflictedToken inflictedToken in m_InflictedTokens)
        {
            if (inflictedToken == null)
                Logger.Log(this.GetType().Name, $"Empty token for {name}", LogLevel.WARNING);
            if (inflictedToken.m_Tier > inflictedToken.m_TokenTierData.NumTiers || inflictedToken.m_Tier <= 0)
                Logger.Log(this.GetType().Name, $"Invalid tier {inflictedToken.m_Tier} for token tier {inflictedToken.m_TokenTierData.m_Id} for {name}", LogLevel.WARNING);
        }
    }
#endif
}

[System.Serializable]
public struct SummonWrapper
{
    public bool m_PrioritsePositions;
    public List<int> m_PrioritisedRows;
    public List<int> m_PrioritisedCols;

    public List<Adds> m_Adds;
}

[System.Serializable]
public struct Adds
{
    public EnemyCharacterSO m_EnemyCharacterSO;
    public Stats m_StatAugments;
}
