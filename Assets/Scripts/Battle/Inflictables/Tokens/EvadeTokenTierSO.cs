using UnityEngine;

[CreateAssetMenu(fileName = "EvadeTokenTierSO", menuName = "ScriptableObject/Inflictables/Token/EvadeTokenTierSO")]
public class EvadeTokenTierSO : TokenTierSO
{
    public override TokenType TokenType => TokenType.EVADE;
    public override bool m_ResetConditionMet => true;
}
