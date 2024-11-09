using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectiveMarker : MonoBehaviour
{
    public enum Type
    {
        Default,
        Enemy,
        Boss,
        Lord,
    }

    [SerializeField]
    List<ParticleSystem> m_Particles;

    [SerializeField]
    ParticleSystem m_DefaultIconParticles;

    [SerializeField]
    ParticleSystem m_EnemyIconParticles;

    [SerializeField]
    ParticleSystem m_BossIconParticles;

    [SerializeField]
    ParticleSystem m_LordIconParticles;

    ParticleSystem m_ActiveIconParticles;

    bool m_IsActive;

    private void Awake()
    {
        var emission = m_DefaultIconParticles.emission;
        emission.enabled = false;
        emission = m_EnemyIconParticles.emission;
        emission.enabled = false;
        emission = m_BossIconParticles.emission;
        emission.enabled = false;
        emission = m_LordIconParticles.emission;
        emission.enabled = false;
        SetMarkerType(Type.Default);

        GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
    }

    private void OnBattleEnd(UnitAllegiance _, int numTurns)
    {
        GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
        SetActive(false);
    }

    public void SetColor(Color color)
    {
        m_Particles?.ForEach(x => { var main = x.main; main.startColor = color; });
    }

    public void SetMarkerType(Type type)
    {
        ParticleSystem next = null;
        switch (type)
        {
            case Type.Default:
                next = m_DefaultIconParticles;
                break;
            case Type.Enemy:
                next = m_EnemyIconParticles;
                break;
            case Type.Boss:
                next = m_BossIconParticles;
                break;
            case Type.Lord:
                next = m_LordIconParticles;
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
