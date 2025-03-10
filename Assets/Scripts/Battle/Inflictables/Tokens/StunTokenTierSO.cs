using UnityEngine;

[CreateAssetMenu(fileName = "StunTokenTierSO", menuName = "ScriptableObject/Inflictables/Token/StunTokenTierSO")]
public class StunTokenTierSO : TokenTierSO {
    public override TokenType TokenType => TokenType.STUN;
    public override bool m_ResetConditionMet => true;
}
