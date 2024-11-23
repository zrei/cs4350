using System.Collections.Generic;
using Game.UI;
using UnityEngine;

[CreateAssetMenu(fileName="LevelSO", menuName="ScriptableObject/Level/LevelSO")]
public class LevelSO : ScriptableObject
{
    [Header("Level Details")]
    public int m_LevelId;
    public int m_LevelNum;
    public string m_LevelName;
    [TextArea]
    public string m_LevelDescription;
    [Tooltip("Whether the level will immediately end when the player fails a battle")]
    public bool m_FailOnDefeat = true;

    [Header("BGM")]
    public AudioDataSO m_LevelBGM;

    [Header("Battle Biome")]
    public BattleMapType m_BiomeName;

    [Header("Rations")]
    public float m_StartingRations;
    
    [Header("Party Limits")]
    public int m_UnitLimit = 8;
    [Tooltip("Characters that must take part in this battle - the lord and any characters not owned by the player will be disregarded if it is in this list\nIf the number of characters exceeds the party limit, only the first few will be taken")]
    public List<PlayerCharacterSO> m_LockedInCharacters;
    
    [Header("Rewards")]
    public List<PlayerCharacterSO> m_RewardCharacters;
    public List<WeaponInstanceSO> m_RewardWeapons;
    
    public string PreDialogueFlag => "PRE_DIALOGUE_" + m_LevelId;
    public string PostDialogueFlag => "POST_DIALOGUE_" + m_LevelId;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (m_LockedInCharacters.Count + 1 > m_UnitLimit)
        {
            Logger.LogEditor(this.GetType().Name, $"There are more locked in characters than slots for level {m_LevelNum} ({name})", LogLevel.WARNING);
        }
    }
#endif
}
