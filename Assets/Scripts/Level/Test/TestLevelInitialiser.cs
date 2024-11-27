using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test script to initialise a standalone level without loading from world map.
/// </summary>
public class TestLevelInitialiser : MonoBehaviour
{
    [Header("Test Data")]
    [SerializeField] private List<PlayerCharacterData> m_TestCharacterData;
    
    private void Start()
    {
        for (int i = 0; i < m_TestCharacterData.Count; i++)
        {
            m_TestCharacterData[i].m_CurrStats = m_TestCharacterData[i].m_BaseData.m_StartingStats;
            m_TestCharacterData[i].m_CurrClassIndex = m_TestCharacterData[i].m_BaseData.m_PathGroup.GetDefaultClassIndex();
        }

        if (LevelManager.IsReady)
            LevelManager.Instance.Initialise(m_TestCharacterData);
        else 
            LevelManager.OnReady += () => LevelManager.Instance.Initialise(m_TestCharacterData);
        
        GlobalEvents.Scene.OnSceneTransitionCompleteEvent?.Invoke(SceneEnum.WORLD_MAP, SceneEnum.LEVEL);
    }
}
