using UnityEngine;

[System.Serializable]
public class StatusEffect : IStatChange
{
    [SerializeField] StatusEffectSO m_StatusEffectSO;
    private int m_StackRemaining;

    public int StackRemaining => m_StackRemaining;
    public bool IsDepleted => m_StackRemaining <= 0;

    public void Tick(Unit unit)
    {
        ApplyAffect(unit);
        ReduceStack(1);
    }

    public void AddStack(int amt)
    {
        m_StackRemaining = Mathf.Max(m_StackRemaining + amt, m_StatusEffectSO.m_MaxStack);
    }

    public void ReduceStack(int amt)
    {
        m_StackRemaining = Mathf.Max(m_StackRemaining - amt, 0);
    }

    private void ApplyAffect(Unit unit)
    {
        if (m_StatusEffectSO.StatusEffectType == StatusEffectType.INFLICT_DAMAGE)
        {
            unit.TakeDamage(((DamageStatusEffectSO) m_StatusEffectSO).m_DamagePerTurn);
        }
    }

    public float GetFlatStatChange(StatType statType)
    {
        if (m_StatusEffectSO.StatusEffectType != StatusEffectType.STAT_CHANGE)
            return 0;

        StatStatusEffectSO statStatusEffectSO = (StatStatusEffectSO) m_StatusEffectSO;
        
        if (statStatusEffectSO.m_StatChangeType != StatChangeType.FLAT)
            return 0;

        if (statStatusEffectSO.m_AffectedStat != statType)
            return 0;

        return statStatusEffectSO.m_AffectAmount;
    }

    public float GetMultStatChange(StatType statType)
    {
        if (m_StatusEffectSO.StatusEffectType != StatusEffectType.STAT_CHANGE)
            return 1;

        StatStatusEffectSO statStatusEffectSO = (StatStatusEffectSO) m_StatusEffectSO;
        
        if (statStatusEffectSO.m_StatChangeType != StatChangeType.MULT)
            return 1;

        if (statStatusEffectSO.m_AffectedStat != statType)
            return 1;

        return statStatusEffectSO.m_AffectAmount;
    }
}