using System.Collections.Generic;
using System.Linq;

public class PlayerUnit : Unit
{
    public override UnitAllegiance UnitAllegiance => UnitAllegiance.PLAYER;

    #region Initialisation
    /// <summary>
    /// Initialise stats, position, etc.
    /// Called when the unit is first spawned onto the battlefield
    /// </summary>
    public void Initialise(PlayerCharacterBattleData characterBattleData, List<InflictedToken> additionalPermanentTokens, float currMoralityPercentage)
    {
        CharacterName = characterBattleData.m_BaseData.m_CharacterName;
        CharacterSOInstanceID = characterBattleData.m_BaseData.GetInstanceID();
        List<InflictedToken> permanentTokens = new();
        permanentTokens.AddRange(additionalPermanentTokens);
        permanentTokens.AddRange(characterBattleData.GetInflictedTokens(currMoralityPercentage));
        base.Initialise(characterBattleData.m_CurrStats, characterBattleData.m_BaseData.m_Race, characterBattleData.m_ClassSO, characterBattleData.m_BaseData.m_CharacterSprite, characterBattleData.GetUnitModelData(), characterBattleData.m_CurrEquippedWeapon, permanentTokens);
    }
    #endregion

    #region Active Skills
    public override IEnumerable<ActiveSkillSO> GetActiveSkills()
    {
        return ((PlayerClassSO) m_Class).m_ActiveSkills;
    }
    #endregion
}
