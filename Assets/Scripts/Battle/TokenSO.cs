using UnityEngine;

public struct Token 
{
    public TokenSO m_TokenData;

    // represents different things for different token types
    public float m_Amount;
}

public enum TokenType
{
    INFLICT_STATUS,
    FLAT_PHYS_BUFF,
    MULT_PHYS_BUFF,
    FLAT_MAG_BUFF,
    MULT_MAG_BUFF
}

public class TokenSO : ScriptableObject
{
    public string m_Name;
    public Sprite m_Icon;
    public TokenType m_TokenType;

    public bool AffectDamageCalcs => m_TokenType != TokenType.INFLICT_STATUS;
}