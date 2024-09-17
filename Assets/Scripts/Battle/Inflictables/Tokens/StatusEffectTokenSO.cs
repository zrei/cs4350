using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectTokenSO", menuName = "ScriptableObject/Token/StatusEffectTokenSO")]
public class StatusEffectTokenSO : TokenSO
{
    public StatusEffectSO m_StatusEffect;
    public override TokenType TokenType => TokenType.INFLICT_STATUS;
}
