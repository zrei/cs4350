using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class LevelManager : MonoBehaviour
{
    // Graph Information
    [SerializeField] LevelNodeManager m_LevelNodeManager;
    
    // Graphics
    [SerializeField] LevelGraphicsManager m_LevelGraphicsManager;
    
    #region Test
    [SerializeField] private LevelSO m_TestLevel;
    [SerializeField] private NodeInternal testStartNodeInternal;
    [SerializeField] private List<Unit> m_TestPlayerUnits;
    
    public void Start()
    {
        Initialise();

        GlobalEvents.Level.NodeEnteredEvent(testStartNodeInternal);
    }
    #endregion
    
    #region Initialisation

    public void Initialise()
    {
        var levelNodes = GetComponentsInChildren<NodeInternal>().ToList();
        var levelEdges = GetComponentsInChildren<LevelEdge>().ToList();
        var timeLimit = m_TestLevel.m_TimeLimit;
        
        // Initialise the internal graph representation of the level
        m_LevelNodeManager.Initialise(levelNodes, levelEdges, timeLimit);
        
        // Initialise the graphics of the level
        m_LevelGraphicsManager.Initialise(levelNodes, levelEdges);
        
        m_LevelNodeManager.SetStartNode(testStartNodeInternal);
    }

    #endregion
    

    // Update is called once per frame
    void Update()
    {
    }
}
