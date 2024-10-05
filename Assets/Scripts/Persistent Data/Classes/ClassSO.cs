using UnityEngine;

/// <summary>
/// Packages the information on a class
/// </summary>
[CreateAssetMenu(fileName = "ClassSO", menuName = "ScriptableObject/Classes/ClassSO")]
public class ClassSO : ScriptableObject
{
    [Header("Details")]
    public int m_Id;
    public string m_ClassName;
    public Sprite m_Icon;
    public OutfitType m_OutfitType;

    [Header("Unlock Details")]
    [Tooltip("Level at which this class is unlocked")]
    public int m_LevelLock;
    [Tooltip("Amount that character's base stats are augmented")]

    [Header("Stats and Growth Rates")]
    public Stats m_StatAugments;
    [Tooltip("Amount that character's growth rate is augmented")]
    public GrowthRate m_GrowthRateAugments;

    [Header("Weapon")]
    public WeaponTypeSO m_WeaponType;

    [Header("Skills")]
    public PassiveSkillSO[] m_PassiveSkills;
    public ActiveSkillSO[] m_ActiveSkills;

    [Header("Movement")]
    public TileType[] m_TraversableTileTypes = new TileType[] {TileType.NORMAL};
    public bool m_CanSwapTiles = false;
}

/// <summary>
/// Packages information on a weapon TYPE, e.g. the entire archetype of bows
/// </summary>
public class WeaponTypeSO : ScriptableObject
{
    public WeaponType m_WeaponType;
    public WeaponAnimationType m_WeaponAnimationType;

    /// <summary>
    /// The beginner weapon to be assigned for this weapon type if the unit has not equipped a weapon
    /// </summary>
    public WeaponInstanceSO m_BeginnerWeapon;
}

public enum OutfitType
{
    MAGE,
    HOODED,
    ARMOR
}
