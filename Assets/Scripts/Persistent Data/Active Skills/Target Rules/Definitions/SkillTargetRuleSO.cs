using UnityEngine;

public abstract class SkillTargetRuleSO : ScriptableObject {
    [Tooltip("Description to use for this rule")]
    public string m_Description;

    public Sprite m_PositionSprite;
}
