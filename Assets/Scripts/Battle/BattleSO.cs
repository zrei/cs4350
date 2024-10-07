using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemyUnitPlacement
{
    public CoordPair m_Coordinates;
    public EnemyCharacterSO m_EnemyCharacterData;
    public Stats m_StatAugments;

    public UnitModelData GetUnitModelData()
    {
        return m_EnemyCharacterData.GetUnitModelData();
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
