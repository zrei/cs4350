using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test script to initialise a standalone level without loading from world map.
/// </summary>
public class TestLevelInitialiser : MonoBehaviour
{
    [Header("Test Data")]
    [SerializeField] private List<PlayerCharacterData> m_TestCharacterData;
    [SerializeField] private LevelManager m_LevelManager;
    
    private void Start()
    {
        for (int i = 0; i < m_TestCharacterData.Count; i++)
        {
            m_TestCharacterData[i].m_CurrStats = m_TestCharacterData[i].m_BaseData.m_StartingStats;
            m_TestCharacterData[i].m_CurrClassIndex = m_TestCharacterData[i].m_BaseData.m_PathGroup.GetDefaultClassIndex();
        }
        
        m_LevelManager.Initialise(m_TestCharacterData);
    }
}