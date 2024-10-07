using UnityEngine;

[CreateAssetMenu(fileName = "EnemyCharacterSO", menuName = "ScriptableObject/Battle/Enemy/EnemyCharacterSO")]
public class EnemyCharacterSO : CharacterSO
{
    public EnemyClassSO m_EnemyClass;
    [Tooltip("Stats")]
    public Stats m_Stats;
    public WeaponInstanceSO m_EquippedWeapon;

    public UnitModelData GetUnitModelData()
    {
        return m_Race.GetUnitModelData(m_Gender, m_EnemyClass.m_OutfitType);
    }

    public EnemyActionSetSO EnemyActionSetSO => m_EnemyClass.m_EnemyActionSet;
}
