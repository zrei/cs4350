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

    public WeaponInstanceSO DefaultWeapon => m_WeaponType.m_BeginnerWeapon;
}

public enum OutfitType
{
    MAGE,
    HOODED,
    ARMOR
}
