using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillCooldownTracker
{
    private readonly Dictionary<ActiveSkillSO, int> m_Cooldowns = new();

    public void Tick()
    {
        IEnumerable<ActiveSkillSO> keys = m_Cooldowns.Keys.ToList();
        foreach (ActiveSkillSO activeSkillSO in keys)
        {
            m_Cooldowns[activeSkillSO] = Mathf.Max(m_Cooldowns[activeSkillSO] - 1, 0);
        }
    }

    public int GetCooldown(ActiveSkillSO activeSkillSO)
    {
        return m_Cooldowns.GetValueOrDefault(activeSkillSO, 0);
    }

    public bool CanUtiliseSkill(ActiveSkillSO activeSkillSO)
    {
        return GetCooldown(activeSkillSO) <= 0;
    }

    public float GetCooldownProportion(ActiveSkillSO activeSkillSO)
    {
        if (activeSkillSO.m_CooldownTurns <= 0)
            return 1f;

        return (float) (activeSkillSO.m_CooldownTurns - GetCooldown(activeSkillSO)) / activeSkillSO.m_CooldownTurns;
    }

    public void UtiliseSkill(ActiveSkillSO activeSkillSO)
    {
        if (m_Cooldowns.ContainsKey(activeSkillSO))
        {
            m_Cooldowns[activeSkillSO] = activeSkillSO.m_CooldownTurns + 1;
        }
    }

    public void SetSkills(IEnumerable<ActiveSkillSO> activeSkills)
    {
        m_Cooldowns.Clear();
        foreach (ActiveSkillSO activeSkillSO in activeSkills)
        {
            if (activeSkillSO.m_CooldownTurns > 0)
            {
                m_Cooldowns.Add(activeSkillSO, 0);
            }
        }
    }
}
