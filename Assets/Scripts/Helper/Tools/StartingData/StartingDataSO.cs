using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StartingDataSO", menuName = "ScriptableObject/StartingData/StartingDataSO")]
public class StartingDataSO : ScriptableObject
{
    [Tooltip("Characters to start with")]
    public List<StartingPlayerCharacter> m_StartingCharacters;
    [Tooltip("Persistent flags that start off as true")]
    public List<Flag> m_StartingPersistentFlags;
    [Tooltip("Level to start from - previous levels will be automatically cleared")]
    public int m_StartingLevel = 1;
    [Tooltip("Weapons to start with")]
    public List<WeaponInstanceSO> m_StartingWeapons;
    [Tooltip("Percentage of morality to start from")]
    [Range(-1f, 1f)]
    public float m_StartingMoralityPercentage = 0f;
}
