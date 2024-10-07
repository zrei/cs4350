using UnityEngine;

public abstract class ClassSO : ScriptableObject
{
    [Header("Details")]
    public string m_ClassName;
    public Sprite m_Icon;
    public OutfitType m_OutfitType;


    [Header("Weapon")]
    public WeaponTypeSO m_WeaponType;
    public WeaponAnimationType WeaponAnimationType => m_WeaponType.m_WeaponAnimationType;

    [Header("Passive Buffs")]
    public PassiveSkillSO[] m_PassiveSkills;

    [Header("Movement")]
    public TileType[] m_TraversableTileTypes = new TileType[] {TileType.NORMAL};
    public bool m_CanSwapTiles = false;
}
