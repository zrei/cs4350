using System.Collections.Generic;
using UnityEngine;


public enum WeaponType
{
    SWORD,
    LANCE,
    AXE,
    BOW,
    MAGIC
}


// this represents instances of weapons
[CreateAssetMenu(fileName = "WeaponSO", menuName = "ScriptableObject/Classes/WeaponSO")]
public class WeaponSO : ScriptableObject
{
    public int m_WeaponId;
    public WeaponAnimationType m_WeaponAnimationType;

    // tier 1 weapon model?
    public WeaponModel m_WeaponModel;
    public List<TokenSO> m_PassiveTokens;
}

[System.Serializable]
public struct BeginnerWeapon
{
    public WeaponType m_WeaponType;
    public WeaponSO m_WeaponSO;
}

public class WeaponManager
{
    [SerializeField] List<BeginnerWeapon> m_BeginnerWeapons;
}
