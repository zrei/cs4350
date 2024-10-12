using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyUnitPlacement
{
    public CoordPair m_Coordinates;
    public EnemyCharacterSO m_EnemyCharacterData;
    public Stats m_StatAugments;
    [Tooltip("Whether this unit needs to be defeated for the battle to be considered done - this is ignored if the win condition is not to defeat required units")]
    public bool m_DefeatRequired = true;

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

    [Header("Win Conditions")]
    // only one win condition is allowed
    public WinCondition m_WinCondition = WinCondition.DEFEAT_REQUIRED;
    [Tooltip("Number of turns the player needs to survive - this is ignored if the win condition is not SURVIVE_TURNS")]
    public float m_TurnsToSurvive = 25f;

    [Header("Lose conditions")]
    [Tooltip("Secondary lose conditions apart from certain units being defeated")]
    public SecondaryLoseCondition[] m_AdditionalLoseConditions;
    [Tooltip("Maximum number of turns the player is given to complete the battle - this is ignored if TOO_MANY_TURNS is not one of the secondary lose conditions")]
    // note: this should not be mixed with a win condition of SURVIVE_TURNS.
    public float m_MaxTurns = 25f;
}
