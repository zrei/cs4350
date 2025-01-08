using UnityEngine;

[CreateAssetMenu(fileName = "ExtendTurnTokenTierSO", menuName = "ScriptableObject/Inflictables/Token/ExtendTurnTokenTierSO")]
public class ExtendTurnTokenTierSO : TokenTierSO
{
    public override TokenType TokenType => TokenType.EXTEND_TURN;
    public override bool m_ResetConditionMet => true;
    public override int NumTiers => 1;
}
