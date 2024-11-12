using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXSystem : MonoBehaviour
{
    private ParticleSystem[] m_ParticleSystems;
    private List<ParticleSystem.MainModule> m_MainModules = new();
    private List<ParticleSystem.MinMaxGradient> m_OriginalStartColors = new();

    public event Action<VFXSystem> onStopped;

    private Coroutine StopCo
    {
        set
        {
            if (m_StopCo == value) return;
            if (m_StopCo != null)
            {
                StopCoroutine(m_StopCo);
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
        onStopped?.Invoke(this);
        RestoreStartColor();
    }

    public void Play(float? duration = null, bool unscaledTime = false)
    {
        if (m_ParticleSystems[0].isPlaying) return;

        m_ParticleSystems[0].Play();

        StopCo = null;
        if (duration != null)
        {
            m_StopCo = StartCoroutine(StopAfterDelay(duration.Value, unscaledTime));
        }
    }

    public void Stop()
    {
        m_ParticleSystems[0].Stop();
    }

    private IEnumerator StopAfterDelay(float duration, bool unscaledTime)
    {
        if (unscaledTime)
        {
            yield return new WaitForSecondsRealtime(duration);
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }
        Stop();
    }

    public void SetStartColor(Color color)
    {
        m_MainModules.ForEach(main => main.startColor = color);
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
