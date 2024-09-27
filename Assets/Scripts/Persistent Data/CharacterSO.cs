using UnityEngine;

[CreateAssetMenu(fileName="CharacterSO", menuName="ScriptableObject/CharacterSO")]
public class CharacterSO : ScriptableObject
{
    /// <summary>
    /// Arbitrary value to mark the spill over point for an internal stat progress
    /// </summary>
    public const int MAX_STAT_PROGESS = 100;

    public int m_Id;
    public GameObject m_BaseModel;
    public string m_CharacterName;
    public string m_Description;
    public Sprite m_CharacterSprite;
    public ClassSO m_StartingClass;
    public int m_StartingLevel;
    [Tooltip("Starting stats when first unlocked by the player")]
    public Stats m_StartingStats;
    [Tooltip("Growth rates for each stat - how growth rates work is that once the internally tracked progress of each stat reaches an arbitrary value, a single stat point is added to their base stats. Growth rates control how fast the internal progress grows.")]
    public GrowthRate m_GrowthRates;
}
