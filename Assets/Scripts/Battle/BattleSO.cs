using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemyUnitPlacement
{
    public Gender m_Gender;
    public CoordPair m_Coodinates;
    public Stats m_Stats;
    public ClassSO m_Class;
    public RaceSO m_Race;
    public EnemyActionSetSO m_Actions;
    public Sprite m_EnemySprite;

    public UnitModelData GetUnitModelData()
    {
        return m_Race.GetUnitModelData(m_Gender, m_Class.m_OutfitType);
    }
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
