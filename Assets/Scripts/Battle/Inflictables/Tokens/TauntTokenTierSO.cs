using UnityEngine;

[CreateAssetMenu(fileName = "TauntTokenTierSO", menuName = "ScriptableObject/Inflictables/Token/TauntTokenTierSO")]
public class TauntTokenTierSO : TokenTierSO {
    public override TokenType TokenType => TokenType.TAUNT;
    public override bool m_ResetConditionMet => true;
}
