using System.Collections.Generic;

public interface IAttack : IStat
{
    // break it out later, possibly
    public void Attack(ActiveSkillSO attack, List<IHealth> targets);
}