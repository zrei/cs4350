using System.Collections.Generic;
using UnityEngine;

public interface ICanAttack : IStat, ICritModifier
{
    // break it out later, possibly
    public void PerformSkill(ActiveSkillSO attack, List<IHealth> targets, Vector3? targetMovePosition);

    public float GetBaseAttackModifier();

    public float GetBaseHealModifier();
}
