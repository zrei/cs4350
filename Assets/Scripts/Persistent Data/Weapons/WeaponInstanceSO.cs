using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This represents an instance of a weapon. It may not be unique in the context of the game
/// </summary>
[CreateAssetMenu(fileName = "WeaponInstanceSO", menuName = "ScriptableObject/Weapons/WeaponInstanceSO")]
public class WeaponInstanceSO : ScriptableObject
{
    /// <summary>
    /// Used in the inventory and for serialisation
    /// </summary>
    public int m_WeaponId;
    public string m_WeaponName;
    /// <summary>
    /// To assist in checking whether a weapon can be equipped for a certain class - would have to be synced up to the WeaponTypeSOs
    /// </summary>
    public WeaponType m_WeaponType;
    public List<WeaponModel> m_WeaponModels;

    [Tooltip("Always applies, will not expire - the number of tokens will be ignored")] 
    public List<PassiveEffect> m_PassiveEffects;

    public List<InflictedToken> GetInflictedTokens(int characterlevel)
    {
        List<InflictedToken> inflictedTokens = new();
        foreach (PassiveEffect passiveEffect in m_PassiveEffects)
        {
            inflictedTokens.AddRange(passiveEffect.GetInflictedTokens(characterlevel));
        }
        return inflictedTokens;
    }

    /// <summary>
    /// How much this weapon affects base attack stat
    /// </summary>
    public float m_BaseAttackModifier = 1f;
    public float m_BaseHealModifier = 1f;

}
