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
    public WeaponAnimationType WeaponAnimationType => m_WeaponType.m_WeaponAnimationType;
    public WeaponInstanceSO DefaultWeapon => m_WeaponType.m_BeginnerWeapon;

    [Header("Skills")]
    public PassiveSkillSO[] m_PassiveSkills;
    public ActiveSkillSO[] m_ActiveSkills;

    [Header("Movement")]
    public TileType[] m_TraversableTileTypes = new TileType[] {TileType.NORMAL};
    public bool m_CanSwapTiles = false;
}

public enum OutfitType
{
    MAGE,
    HOODED,
    ARMOR
}
