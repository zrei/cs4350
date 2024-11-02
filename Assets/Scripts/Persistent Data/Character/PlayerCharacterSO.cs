using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="PlayerCharacterSO", menuName="ScriptableObject/Characters/PlayerCharacterSO")]
public class PlayerCharacterSO : CharacterSO
{
    [Header("Player-only details")]
    public int m_Id;
    public PathGroupSO m_PathGroup;
    public int StartingClassIndex => m_PathGroup.GetDefaultClassIndex();
    public int NumClasses => m_PathGroup.NumClasses;
    public PlayerClassSO StartingClass => m_PathGroup.GetDefaultClass();
    public int m_StartingLevel;
    [Tooltip("Starting stats when first unlocked by the player")]
    public Stats m_StartingStats;
    [Tooltip("Growth rates for each stat - how growth rates work is that once the internally tracked progress of each stat reaches an arbitrary value, a single stat point is added to their base stats. Growth rates control how fast the internal progress grows.")]
    public GrowthRate m_GrowthRates;
    public CharacterMoralityTraitSO m_CharacterMoralityTrait;
    public IEnumerable<InflictedToken> GetInflictedMoralityTokens(float currMoralityPercentage) => m_CharacterMoralityTrait.GetInflictedTokens(currMoralityPercentage);

    [Tooltip("The lord cannot die in battle, or the battle is lost")]
    public bool m_IsLord;

    public UnitModelData GetUnitModelData(OutfitType outfitType)
    {
        return m_Race.GetUnitModelData(m_Gender, outfitType);
    }

    /// <summary>
    /// Checks using the current game state only
    /// </summary>
    /// <returns></returns>
    public List<bool> GetUnlockedClassIndexes(int characterLevel)
    {
        return m_PathGroup.GetUnlockedClassIndexes(characterLevel);
    }
}
