using UnityEngine;

[CreateAssetMenu(fileName = "PhysicalActiveSkillSO", menuName = "ScriptableObject/ActiveSkills/PhysicalActiveSkillSO")]
public class PhysicalActiveSkillSO : ActiveSkillSO
{
    public override bool IsMagic => false;
}
