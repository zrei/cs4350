using UnityEngine;

[CreateAssetMenu(fileName="PlayerCharacterSO", menuName="ScriptableObject/Characters/PlayerCharacterSO")]
public class PlayerCharacterSO : CharacterSO
{
    public int m_Id;
    public PlayerClassSO m_StartingClass;
    public int m_StartingLevel;
    [Tooltip("Starting stats when first unlocked by the player")]
    public Stats m_StartingStats;
    [Tooltip("Growth rates for each stat - how growth rates work is that once the internally tracked progress of each stat reaches an arbitrary value, a single stat point is added to their base stats. Growth rates control how fast the internal progress grows.")]
    public GrowthRate m_GrowthRates;

    public UnitModelData GetUnitModelData(OutfitType outfitType)
    {
        return m_Race.GetUnitModelData(m_Gender, outfitType);
    }
}
