using System.Collections.Generic;
using UnityEngine;

public enum PlayerClassPlacement
{
    FRONT = 0,
    MIDDLE = 1,
    BACK = 2
}

/// <summary>
/// Packages the information on a class
/// </summary>
[CreateAssetMenu(fileName = "PlayerClassSO", menuName = "ScriptableObject/Classes/PlayerClassSO")]
public class PlayerClassSO : ClassSO
{
    [Header("Placement")]
    public PlayerClassPlacement m_PlayerClassPlacement = PlayerClassPlacement.MIDDLE;

    [Header("Unlock Details")]
    [Tooltip("Level at which this class is unlocked")]
    public int m_LevelLock;
    
    [Header("Stats and Growth Rates")]
    [Tooltip("Amount that character's base stats are augmented")]
    public Stats m_StatAugments;
    [Tooltip("Amount that character's growth rate is augmented")]
    public GrowthRate m_GrowthRateAugments;

    [Header("Skills")]
    public ActiveSkillSO[] m_ActiveSkills;

    [Header("Passive Effects")]
    public List<ClassPassiveEffect> m_PassiveEffects;

    public WeaponInstanceSO DefaultWeapon => m_WeaponType.m_BeginnerWeapon;

    public List<InflictedToken> GetInflictedTokens(int characterlevel)
    {
        List<InflictedToken> inflictedTokens = new();
        foreach (ClassPassiveEffect classEffect in m_PassiveEffects)
        {
            inflictedTokens.AddRange(classEffect.GetInflictedTokens(characterlevel));
        }
        return inflictedTokens;
    }
}

[System.Serializable]
public struct ClassPassiveEffect
{
    [Tooltip("Conditions that must be met for this set of tokens to be applied - leave untouched if there are no conditions")]
    public UnlockCondition m_UnlockCondition;
    public List<InflictedToken> m_InflictedTokens;
    public Sprite m_PassiveEffectIcon;
    public string m_Name;
    [TextArea]
    public string m_Description;

    public List<InflictedToken> GetInflictedTokens(int characterLevel)
    {
        if (m_UnlockCondition.IsSatisfied(characterLevel))
            return m_InflictedTokens;
        else
            return new();
    }
}

public enum OutfitType
{
    MAGE,
    HOODED,
    ARMOR
}
