using UnityEngine;

[CreateAssetMenu(fileName = "EnemyCharacterSO", menuName = "ScriptableObject/Characters/EnemyCharacterSO")]
public class EnemyCharacterSO : CharacterSO
{
    public EnemyActionSetSO m_Actions;

    public UnitModelData GetUnitModelData()
    {
        return m_Race.GetUnitModelData(m_Gender, m_StartingClass.m_OutfitType);
    }
}
