using UnityEngine;

[CreateAssetMenu(fileName = "MagicActiveSkillSO", menuName = "ScriptableObject/Classes/ActiveSkills/MagicActiveSkillSO")]
public class MagicActiveSkillSO : ActiveSkillSO
{
    public override bool IsMagic => true;
    public float m_ConsumedManaAmount = 0f;
}