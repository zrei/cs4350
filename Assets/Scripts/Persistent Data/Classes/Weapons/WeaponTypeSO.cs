using UnityEngine;

/// <summary>
/// Packages information on a weapon TYPE, e.g. the entire archetype of bows
/// </summary>
[CreateAssetMenu(fileName = "WeaponTypeSO", menuName = "ScriptableObject/Classes/Weapons/WeaponTypeSO")]
public class WeaponTypeSO : ScriptableObject
{
    public WeaponType m_WeaponType;
    public WeaponAnimationType m_WeaponAnimationType;

    /// <summary>
    /// The beginner weapon to be assigned for this weapon type if the unit has not equipped a weapon
    /// </summary>
    public WeaponInstanceSO m_BeginnerWeapon;
}
