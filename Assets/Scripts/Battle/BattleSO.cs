using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

[System.Serializable]
public struct UnitPlacement
{
    public Unit m_Unit;
    public CoordPair m_Coodinates;
    public Stats m_Stats;
}

[CreateAssetMenu(fileName="BattleSO", menuName="ScriptableObject/Battle/BattleSO")]
public class BattleSO : ScriptableObject
{
    public List<UnitPlacement> m_EnemyUnitsToSpawn;
    /// <summary>
    /// List of coordinates that the player units start in
    /// </summary>
    public List<CoordPair> m_PlayerStartingCoordinates;
}