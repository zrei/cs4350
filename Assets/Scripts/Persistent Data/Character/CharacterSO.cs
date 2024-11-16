using UnityEngine;

public abstract class CharacterSO : ScriptableObject
{
    public Gender m_Gender;
    public RaceSO m_Race;
    public string m_CharacterName;
    public string m_Description;
    public Sprite m_CharacterSprite;

    [Header("Base Model Colour")]
    public Color m_SkinColor;
    public Color m_EyeColor;
}
