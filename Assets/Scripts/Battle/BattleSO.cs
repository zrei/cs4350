using System.Collections.Generic;
using Game.UI;
using UnityEngine;

[System.Serializable]
public class EnemyUnitPlacement
{
    public CoordPair m_Coordinates;
    public EnemyCharacterSO m_EnemyCharacterData;
    public Stats m_StatAugments;
    public EnemyTag m_EnemyTags = EnemyTag.Default;

    public UnitModelData GetUnitModelData()
    {
        return m_EnemyCharacterData.GetUnitModelData();
    }
}

[CreateAssetMenu(fileName="BattleSO", menuName="ScriptableObject/Battle/BattleSO")]
public class BattleSO : ScriptableObject
{
    public List<ObjectiveSO> m_Objectives;

    public List<EnemyUnitPlacement> m_EnemyUnitsToSpawn;
    /// <summary>
    /// List of coordinates that the player units start in
    /// </summary>
    public List<CoordPair> m_PlayerStartingTiles;
    public int m_ExpReward;
    public AudioDataSO m_BattleBGM;

    [Header("Tutorial")]
    [Tooltip("Tutorial to play upon entering setup phase - leave empty for no tutorial")]
    public List<TutorialPageUIData> m_SetupPhaseTutorial;
    [Tooltip("Tutorial to play upon entering battle phase - leave empty for no tutorial")]
    public List<TutorialPageUIData> m_BattlePhaseTutorial;
}
