using UnityEngine;

[CreateAssetMenu(fileName = "StatChangeTokenSO", menuName = "ScriptableObject/Classes/ActiveSkills/Token/StatChangeTokenSO")]
public class StatChangeTokenSO : TokenSO
{
    public StatType m_AffectedStat;
    public StatChangeType m_StatChangeType;
    [Tooltip("Will be either flat or a proportion")]
    public float m_StatChangeAmount;
}
