using UnityEngine;

public abstract class ClassSO : ScriptableObject
{
    [Header("Details")]
    public string m_ClassName;
    public Sprite m_Icon;
    public OutfitType m_OutfitType;
    [TextArea]
    public string m_ClassDescription;

    [Header("Base Model Colour")]
    public Color m_SkinColor;
    public Color m_EyeColor;

    [Header("Armor Colour")]
    public Color m_ArmorPlate;
    public Color m_ArmorTrim;
    public Color m_UnderArmor;

    [Header("Weapon")]
    public WeaponTypeSO m_WeaponType;
    public WeaponAnimationType WeaponAnimationType => m_WeaponType.m_WeaponAnimationType;

    [Header("Movement")]
    public TileType[] m_TraversableTileTypes = new TileType[] {TileType.NORMAL};
    public bool m_CanSwapTiles = false;
}
