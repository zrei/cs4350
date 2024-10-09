using System.Collections.Generic;
using System.Linq;

public class PlayerUnit : Unit
{
    public override UnitAllegiance UnitAllegiance => UnitAllegiance.PLAYER;

    public string CharacterName { get; private set; }

    #region Initialisation
    /// <summary>
    /// Initialise stats, position, etc.
    /// Called when the unit is first spawned onto the battlefield
    /// </summary>
    public void Initialise(PlayerCharacterBattleData characterBattleData)
    {
        CharacterName = characterBattleData.m_BaseData.m_CharacterName;
        base.Initialise(characterBattleData.m_CurrStats, characterBattleData.m_ClassSO, characterBattleData.m_BaseData.m_CharacterSprite, characterBattleData.GetUnitModelData(), characterBattleData.m_CurrEquippedWeapon);
    }
    #endregion

    #region Active Skills
    public List<ActiveSkillSO> GetAvailableActiveSkills()
    {
        return ((PlayerClassSO) m_Class).m_ActiveSkills.ToList();
    }
    #endregion
}
