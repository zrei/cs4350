using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelEdge : MonoBehaviour
{
    [SerializeField] private NodeInternal nodeInternalA;
    public NodeInternal NodeInternalA => nodeInternalA;
    
    [SerializeField] private NodeInternal nodeInternalB;
    public NodeInternal NodeInternalB => nodeInternalB;
    
    [SerializeField] private float m_Cost;
    public float Cost => m_Cost;
}
