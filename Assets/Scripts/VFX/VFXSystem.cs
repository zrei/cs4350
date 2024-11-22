using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void VFXSystemEvent(VFXSystem vfx);

public class VFXSystem : MonoBehaviour
{
    private ParticleSystem[] m_ParticleSystems;
    private List<ParticleSystem.MainModule> m_MainModules = new();
    private List<ParticleSystem.MinMaxGradient> m_OriginalStartColors = new();

    public event VFXSystemEvent onStop; // this is invoked when the Stop method is called
    public event VFXSystemEvent onParticleSystemStop; // this is invoked only when all the particles in the system die

    private Coroutine StopCo
    {
        set
        {
            if (m_StopCo == value) return;
            if (m_StopCo != null)
            {
                CoroutineManager.Instance.StopCoroutine(m_StopCo);
                m_StopCo = null;
            }
            m_StopCo = value;
        }
    }
    private Coroutine m_StopCo;

    private void Awake()
    {
        m_ParticleSystems = GetComponentsInChildren<ParticleSystem>();
        var isFirst = true;
        foreach (var ps in m_ParticleSystems)
        {
            var main = ps.main;
            if (isFirst)
            {
                main.stopAction = ParticleSystemStopAction.Callback;
                isFirst = false;
            }
            m_MainModules.Add(main);
            m_OriginalStartColors.Add(main.startColor);
        }
    }

    private void OnParticleSystemStopped()
    {
        StopCo = null;
        onParticleSystemStop?.Invoke(this);
        RestoreStartColor();
    }

    public void Play(float? duration = null, bool unscaledTime = false)
    {
        if (m_ParticleSystems[0].isPlaying) return;

        m_ParticleSystems[0].Play();

        StopCo = null;
        if (duration != null)
        {
            m_StopCo = CoroutineManager.Instance.ExecuteAfterDelay(Stop, duration.Value, unscaledTime);
        }
    }

    public void Stop()
    {
        StopCo = null;
        onStop?.Invoke(this);
        m_ParticleSystems[0].Stop();
    }

    public void SetStartColor(Color? color)
    {
        if (color == null) return;
        m_MainModules.ForEach(main => main.startColor = color.Value);
    }

    public void RestoreStartColor()
    {
        for (int i = 0; i < m_MainModules.Count; i++)
        {
            var main = m_MainModules[i];
            main.startColor = m_OriginalStartColors[i];
        }
    }
}
