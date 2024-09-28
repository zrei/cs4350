using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectTokenSO", menuName = "ScriptableObject/Classes/ActiveSkills/Token/StatusEffectTokenSO")]
public class StatusEffectTokenSO : TokenSO
{
    public StatusEffectSO m_StatusEffect;
    public override TokenType TokenType => TokenType.INFLICT_STATUS;
}
