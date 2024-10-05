using System.Collections.Generic;

public interface ICanAttack : IStat, ICritModifier
{
    // break it out later, possibly
    public void PerformSkill(ActiveSkillSO attack, List<IHealth> targets);
}
