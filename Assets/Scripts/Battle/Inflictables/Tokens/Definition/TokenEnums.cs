
public enum TokenType
{
    INFLICT_STATUS,
    FLAT_STAT_CHANGE,
    MULT_STAT_CHANGE,
    SUPPORT_EFFECT_UP,
    CRIT,
    TAUNT,
    STUN,
    EVADE,
    LIFESTEAL,
    REFLECT,
    /// <summary>
    /// Flat change health or mana
    /// </summary>
    FLAT_PASSIVE_CHANGE,
    /// <summary>
    /// Proportion change health or mana
    /// </summary>
    MULT_PASSIVE_CHANGE,
    /// <summary>
    /// Multiply exp gained
    /// </summary>
    EXP_MULTIPLIER,
    /// <summary>
    /// Summon units
    /// </summary>
    SUMMON,
    /// <summary>
    /// Extend your current turn
    /// </summary>
    EXTEND_TURN,
    /// <summary>
    /// Apply a token
    /// </summary>
    APPLY_TOKEN
}

public enum TokenConsumptionType
{
    CONSUME_ON_SUPPORT,
    CONSUME_PRE_ATTACK,
    CONSUME_ON_MAG_ATTACK,
    CONSUME_ON_PHYS_ATTACK,
    CONSUME_ON_MAG_DEFEND,
    CONSUME_ON_PHYS_DEFEND,
    CONSUME_ON_HEAL,
    CONSUME_ON_MANA_ALTER,
    CONSUME_ON_MOVE,
    CONSUME_ON_NEXT_TURN,
    CONSUME_ON_OPPOSING_TARGET,
    CONSUME_ON_SELF_TARGET,
    CONSUME_ON_ALLY_TARGET,
    CONSUME_POST_TURN,
    CONSUME_ON_DEFEAT_UNIT
}
