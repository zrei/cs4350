using System.Collections.Generic;
using System.Linq;

public class PlayerUnit : Unit
{
    public override UnitAllegiance UnitAllegiance => UnitAllegiance.PLAYER;

    #region Active Skills
    public List<ActiveSkillSO> GetAvailableActiveSkills()
    {
        return m_Class.m_Weapon.m_WeaponActiveSkills.ToList();
    }
    #endregion
}