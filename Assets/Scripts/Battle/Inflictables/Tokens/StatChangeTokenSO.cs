using UnityEngine;

[CreateAssetMenu(fileName = "StatChangeTokenSO", menuName = "ScriptableObject/Classes/ActiveSkills/Token/StatChangeTokenSO")]
public class StatChangeTokenSO : TokenSO
{
    public StatType m_AffectedStat;
    public StatChangeType m_StatChangeType;
    public override TokenType TokenType => TokenType.STAT_CHANGE;
}
