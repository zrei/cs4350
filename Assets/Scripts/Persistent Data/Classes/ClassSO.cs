using System.Collections.Generic;
using UnityEngine;

public abstract class ClassSO : ScriptableObject
{
    [Header("Details")]
    public string m_ClassName;
    public Sprite m_Icon;
    public OutfitType m_OutfitType;
    [TextArea]
    public string m_ClassDescription;

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
    public MovementType m_MovementType = MovementType.CARDINAL;

    [Header("Passive Effects")]
    public List<ClassPassiveEffect> m_PassiveEffects;

    /// <summary>
    /// Character level for player classes is the level of the current character.
    /// Character level for enemy classes is the highest player level present in the battle.
    /// </summary>
    /// <param name="characterlevel"></param>
    /// <returns></returns>
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
