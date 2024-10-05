using UnityEngine;

// the attempt at refactoring effects :)
public abstract class ActiveSkillEffectSO : ScriptableObject
{
    public abstract void ApplyEffect(Unit attacker, Unit target, bool isPhysical);
}
