using UnityEngine;

[CreateAssetMenu(fileName="CharacterSO", menuName="ScriptableObject/CharacterSO")]
public class CharacterSO : ScriptableObject
{
    // public Mesh m_BaseModel;
    public string m_CharacterName;
    public string m_Description;
    public ClassSO m_StartingClass;
    public int m_StartingLevel;
    [Tooltip("Starting stats when first unlocked by the player")]
    public Stats m_StartingStats;
    [Tooltip("Growth rates for each stat")]
    public Stats m_GrowthRates;
}