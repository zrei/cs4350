
public enum SkillEffectType
{
    DEALS_DAMAGE,
    DEALS_STATUS_OR_TOKENS,
    HEAL,
    SUMMON,
    ALTER_MANA,
    /// <summary>
    /// This should be used in conjunction with other skill types to alter one's own mana despite not targetting self
    /// </summary>
    ALTER_MANA_SELF,
    TELEPORT,
    CLEANSE,
    APPLY_TILE_EFFECT,
    DAMAGE_SELF,
    LIFESTEAL,
}

public enum SkillType
{
    MAGIC,
    PHYSICAL
}

public enum SkillAnimationType
{
    SUPPORT = 1,
    // can have light attack, heavy attack, bla bla bla
    ATTACK = 2,
}
