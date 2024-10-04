using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectTokenSO", menuName = "ScriptableObject/Classes/ActiveSkills/Token/StatusEffectTokenSO")]
public class StatusEffectTokenSO : TokenSO
{
    public InflictedStatusEffect m_StatusEffect;
    public override TokenType TokenType => TokenType.INFLICT_STATUS;
}
