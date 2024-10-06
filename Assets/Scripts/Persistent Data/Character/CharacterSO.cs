using UnityEngine;

[CreateAssetMenu(fileName="CharacterSO", menuName="ScriptableObject/Characters/CharacterSO")]
public class CharacterSO : ScriptableObject
{
    public int m_Id;
    public Gender m_Gender;
    public RaceSO m_Race;
    public string m_CharacterName;
    public string m_Description;
    public Sprite m_CharacterSprite;
    public ClassSO m_StartingClass;
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
