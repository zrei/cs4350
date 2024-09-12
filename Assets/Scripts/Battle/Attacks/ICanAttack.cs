using System.Collections.Generic;

public interface ICanAttack : IStat
{
    // break it out later, possibly
    public void PerformSKill(ActiveSkillSO attack, List<IHealth> targets);
}