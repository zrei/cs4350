using System.Linq;
using UnityEngine;

public enum TokenType
{
    INFLICT_STATUS,
    STAT_CHANGE,
    SUPPORT_EFFECT_UP
}

public enum ConsumeType
{
    CONSUME_ON_SUPPORT,
    CONSUME_ON_MAG_ATTACK,
    CONSUME_ON_PHYS_ATTACK,
    CONSUME_ON_MAG_DEFEND,
    CONSUME_ON_PHYS_DEFEND
}

public abstract class TokenSO : ScriptableObject
{
    public string m_TokenName;
    public string m_Description;
    public Sprite m_Icon;
    public string m_DisplayAmountFormat = "{0:F0}";
    public virtual TokenType TokenType => TokenType.INFLICT_STATUS;

    [Tooltip("When to consume this token")]
    public ConsumeType[] m_Consumption;

    public bool ContainsConsumptionType(ConsumeType consumeType)
    {
        return m_Consumption.Contains(consumeType);
    }
}
