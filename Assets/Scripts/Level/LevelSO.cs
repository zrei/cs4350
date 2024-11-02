using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="LevelSO", menuName="ScriptableObject/Level/LevelSO")]
public class LevelSO : ScriptableObject
{
    public int m_LevelId;
    public int m_LevelNum;
    public string m_LevelName;
    [TextArea]
    public string m_LevelDescription;
    public float m_TimeLimit;
    public int m_UnitLimit = 8;
    public GameObject m_BiomeObject;

    public string PreDialogueFlag => "PRE_DIALOGUE_" + m_LevelId;
    public string PostDialogueFlag => "POST_DIALOGUE_" + m_LevelId;
    
    public List<PlayerCharacterSO> m_RewardCharacters;
    public List<WeaponInstanceSO> m_RewardWeapons;
}
