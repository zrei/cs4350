using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemyUnitPlacement
{
    public GameObject m_BaseMesh;
    public EnemyUnit m_Unit;
    public CoordPair m_Coodinates;
    public Stats m_Stats;
    public ClassSO m_Class;
    public EnemyActionSetSO m_Actions;
}

[CreateAssetMenu(fileName="BattleSO", menuName="ScriptableObject/Battle/BattleSO")]
public class BattleSO : ScriptableObject
{
    public List<EnemyUnitPlacement> m_EnemyUnitsToSpawn;
    /// <summary>
    /// List of coordinates that the player units start in
    /// </summary>
    public List<CoordPair> m_PlayerStartingTiles;
    public int m_ExpReward;
}
