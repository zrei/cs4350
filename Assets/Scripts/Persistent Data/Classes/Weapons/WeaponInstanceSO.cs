using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This represents an instance of a weapon. It may not be unique in the context of the game
/// </summary>
[CreateAssetMenu(fileName = "WeaponInstanceSO", menuName = "ScriptableObject/Classes/Weapons/WeaponInstanceSO")]
public class WeaponInstanceSO : ScriptableObject
{
    /// <summary>
    /// Used in the inventory and for serialisation
    /// </summary>
    public int m_WeaponId;
    /// <summary>
    /// To assist in checking whether a weapon can be equipped for a certain class - would have to be synced up to the WeaponTypeSOs
    /// </summary>
    public WeaponType m_WeaponType;
    public WeaponModel m_WeaponModel;
    public List<TokenSO> m_PassiveTokens;

    /// <summary>
    /// How much this weapon affects base attack stat
    /// </summary>
    public float m_BaseAttackModifier = 1f;

}
