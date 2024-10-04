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
    public WeaponType m_WeaponType;
}

public class WeaponManager: Singleton<WeaponManager>
{
    [SerializeField] List<WeaponSO> m_BeginnerWeapons;

    private Dictionary<WeaponType, WeaponSO> m_BeginnerWeaponDict;

    protected override void HandleAwake()
    {
        base.HandleAwake();

        m_BeginnerWeaponDict = new();

        foreach (WeaponSO beginnerWeapon in m_BeginnerWeapons)
        {
            if (m_BeginnerWeaponDict.ContainsKey(beginnerWeapon.m_WeaponType))
            {
                Logger.Log(this.GetType().Name, $"There is already a beginner weapon of the same type {beginnerWeapon.m_WeaponType}", LogLevel.WARNING);
                continue;
            }

            m_BeginnerWeaponDict.Add(beginnerWeapon.m_WeaponType, beginnerWeapon);
        }
    }

    public WeaponSO GetBeginnerWeapon(WeaponType weaponType)
    {
        return m_BeginnerWeaponDict[weaponType];
    }
}
