using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitMarker : MonoBehaviour
{
    public enum IconType
    {
        Objective,
        Enemy,
        Boss,
        Lord,
        TimeToAct,
    }

    [SerializeField]
    List<ParticleSystem> m_Particles;

    [SerializeField]
    ParticleSystem m_ObjectiveIconParticles;

    [SerializeField]
    ParticleSystem m_EnemyIconParticles;

    [SerializeField]
    ParticleSystem m_BossIconParticles;

    [SerializeField]
    ParticleSystem m_LordIconParticles;

    [SerializeField]
    ParticleSystem m_TimeToActIconParticles;

    ParticleSystem m_ActiveIconParticles;

    bool m_IsActive;

    private void Awake()
    {
        var emission = m_ObjectiveIconParticles.emission;
        emission.enabled = false;
        emission = m_EnemyIconParticles.emission;
        emission.enabled = false;
        emission = m_BossIconParticles.emission;
        emission.enabled = false;
        emission = m_LordIconParticles.emission;
        emission.enabled = false;
        emission = m_TimeToActIconParticles.emission;
        emission.enabled = false;
        SetMarkerType(IconType.Objective);

        GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
        GlobalEvents.Battle.AttackAnimationEvent += OnAttackAnimation;
    }

    private void OnDestroy()
    {
        GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
        GlobalEvents.Battle.AttackAnimationEvent -= OnAttackAnimation;
    }

    private void OnBattleEnd(UnitAllegiance _, int numTurns)
    {
        GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
        GlobalEvents.Battle.AttackAnimationEvent -= OnAttackAnimation;
        SetActive(false);
    }

    private void OnAttackAnimation(ActiveSkillSO activeSkill, Unit attacker, List<Unit> target)
    {
        SetActive(false);
    }

    public void SetColor(Color color)
    {
        m_Particles?.ForEach(x => { var main = x.main; main.startColor = color; });
    }

    public void SetMarkerType(IconType type)
    {
        ParticleSystem next = null;
        switch (type)
        {
            case IconType.Objective:
                next = m_ObjectiveIconParticles;
                break;
            case IconType.Enemy:
                next = m_EnemyIconParticles;
                break;
            case IconType.Boss:
                next = m_BossIconParticles;
                break;
            case IconType.Lord:
                next = m_LordIconParticles;
                break;
            case IconType.TimeToAct:
                next = m_TimeToActIconParticles;
                break;
        }
        if (next == null || next == m_ActiveIconParticles) return;

        if (m_ActiveIconParticles != null)
        {
            m_ActiveIconParticles.Stop();
            var emission = m_ActiveIconParticles.emission;
            emission.enabled = false;
        }
        m_ActiveIconParticles = next;
        if (m_ActiveIconParticles != null)
        {
            var emission = m_ActiveIconParticles.emission;
            emission.enabled = true;
            if (m_IsActive)
            {
                m_ActiveIconParticles.Play();
            }
        }
    }

    public void SetActive(bool active)
    {
        if (m_IsActive == active) return;

        m_IsActive = active;
        if (active)
        {
            m_Particles?.FirstOrDefault()?.Play();
        }
        else
        {
            m_Particles?.FirstOrDefault()?.Stop();
        }
    }
}
