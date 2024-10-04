using System.Linq;
using UnityEngine;

public enum TokenType
{
    INFLICT_STATUS,
    STAT_CHANGE,
    SUPPORT_EFFECT_UP,
    CRIT,
    TAUNT
}

public enum ConsumeType
{
    CONSUME_ON_SUPPORT,
    CONSUME_ON_MAG_ATTACK,
    CONSUME_ON_PHYS_ATTACK,
    CONSUME_ON_MAG_DEFEND,
    CONSUME_ON_PHYS_DEFEND,
    CONSUME_ON_MOVE,
    CONSUME_ON_SKILL,
    CONSUME_POST_DAMAGE,
    CONSUME_POST_DEFENCE
}

public abstract class TokenSO : ScriptableObject
{
    [Header("Details")]
    public string m_TokenName;
    public string m_Description;
    public Sprite m_Icon;
    
    /*
    public float m_Amount;
    public string m_DisplayAmountFormat = "{0:F0}";
    */
    // public virtual TokenType TokenType => TokenType.INFLICT_STATUS;
}
