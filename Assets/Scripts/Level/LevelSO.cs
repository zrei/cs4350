using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="LevelSO", menuName="ScriptableObject/Level/LevelSO")]
public class LevelSO : ScriptableObject
{
    public int m_LevelId;
    public float m_TimeLimit;
    public int m_UnitLimit;
    public GameObject m_BiomeObject;
    
    public List<PlayerCharacterSO> m_RewardCharacters;
    public List<WeaponInstanceSO> m_RewardWeapons;
}
