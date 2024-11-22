using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct VFXAudio
{
    public AudioDataSO m_AudioDataSO;
    public float m_AudioDelay;
}

[CreateAssetMenu(fileName = "VFXSO", menuName = "ScriptableObject/VFX/VFXSO")]
public class VFXSO : ScriptableObject
{
    public enum Type
    {
        StationaryAttachPrimary,
        StationaryAttachSecondaryFirst,
        StationaryAttachSecondaryAll,
        MovingPrimaryToSecondaryFirst,
        MovingPrimaryToSecondaryAll,
        MovingSequential,
    }

    public Type m_Type;
    public VFXSystem m_VFXPrefab;
    public List<VFXAudio> m_VFXAudio;

    public bool m_IsOverrideStartColor;
    public Color m_StartColorOverride = Color.white;

    public float m_MoveArcHeight = 0.0f;
    public float m_MoveDuration = 0.2f;


    private VFXSystem Get()
    {
        if (!VFXPoolManager.IsReady)
        {
            Debug.LogWarning($"no instance of VFXPoolManager exists");
            return null;
        }
        var vfx = VFXPoolManager.Instance.Get(this);
        if (vfx == null) Debug.LogWarning($"{name} vfx pool max size reached");
        return vfx;
    }

    public virtual void Play(
        Transform primaryAttachPoint,
        List<Transform> secondaryAttachPoints = null,
        Color? color = null,
        float? duration = null,
        bool unscaledTime = false)
    {
        foreach (VFXAudio vFXAudio in m_VFXAudio)
        {
            SoundManager.Instance.Play(vFXAudio.m_AudioDataSO, vFXAudio.m_AudioDelay);
        }


        switch (m_Type)
        {
            case Type.StationaryAttachPrimary:
                HandleStationaryAttachPrimary(primaryAttachPoint, secondaryAttachPoints, color, duration, unscaledTime);
                break;
            case Type.StationaryAttachSecondaryFirst:
                HandleStationaryAttachSecondaryFirst(primaryAttachPoint, secondaryAttachPoints, color, duration, unscaledTime);
                break;
            case Type.StationaryAttachSecondaryAll:
                HandleStationaryAttachSecondaryAll(primaryAttachPoint, secondaryAttachPoints, color, duration, unscaledTime);
                break;
            case Type.MovingPrimaryToSecondaryFirst:
                HandleMovingPrimaryToSecondaryFirst(primaryAttachPoint, secondaryAttachPoints, color, duration, unscaledTime);
                break;
            case Type.MovingPrimaryToSecondaryAll:
                HandleMovingPrimaryToSecondaryAll(primaryAttachPoint, secondaryAttachPoints, color, duration, unscaledTime);
                break;
            case Type.MovingSequential:
                HandleMovingSequential(primaryAttachPoint, secondaryAttachPoints, color, duration, unscaledTime);
                break;
        }
    }

    private void Initialize(VFXSystem vfx, Color? color)
    {
        Color? startColorOverride = color != null
            ? color
            : m_IsOverrideStartColor
                ? m_StartColorOverride
                : null;

        if (startColorOverride != null)
        {
            vfx.SetStartColor(startColorOverride.Value);
        }
    }

    private void HandleStationaryAttachPrimary(
        Transform primaryAttachPoint,
        List<Transform> secondaryAttachPoints = null,
        Color? color = null,
        float? duration = null,
        bool unscaledTime = false)
    {
        var vfx = Get();
        if (vfx == null) return;
        Initialize(vfx, color);
        vfx.transform.SetParent(primaryAttachPoint, false);
        vfx.Play(duration, unscaledTime);
    }

    private void HandleStationaryAttachSecondaryFirst(
        Transform primaryAttachPoint,
        List<Transform> secondaryAttachPoints = null,
        Color? color = null,
        float? duration = null,
        bool unscaledTime = false)
    {
        var vfx = Get();
        if (vfx == null) return;
        Initialize(vfx, color);
        vfx.transform.SetParent(secondaryAttachPoints[0], false);
        vfx.Play(duration, unscaledTime);
    }

    private void HandleStationaryAttachSecondaryAll(
        Transform primaryAttachPoint,
        List<Transform> secondaryAttachPoints = null,
        Color? color = null,
        float? duration = null,
        bool unscaledTime = false)
    {
        foreach (var secondaryAT in secondaryAttachPoints)
        {
            var vfx = Get();
            if (vfx == null) return;
            Initialize(vfx, color);
            vfx.transform.SetParent(secondaryAT, false);
            vfx.Play(duration, unscaledTime);
        }
    }

    private void HandleMovingPrimaryToSecondaryFirst(
        Transform primaryAttachPoint,
        List<Transform> secondaryAttachPoints = null,
        Color? color = null,
        float? duration = null,
        bool unscaledTime = false)
    {
        var vfx = Get();
        if (vfx == null) return;
        Initialize(vfx, color);
        vfx.transform.SetParent(null);
        vfx.transform.position = primaryAttachPoint.position;
        vfx.Play(m_MoveDuration, unscaledTime);
        var startPos = primaryAttachPoint.position;
        PathAnimator.BezierAnimate(
            () => startPos,
            () =>
            {
                var pos = primaryAttachPoint.position + secondaryAttachPoints[0].position;
                pos.y += m_MoveArcHeight;
                return pos;
            },
            () => secondaryAttachPoints[0].position,
            pos => vfx.transform.position = pos,
            rot => vfx.transform.eulerAngles = rot,
            m_MoveDuration
        );
    }

    private void HandleMovingPrimaryToSecondaryAll(
        Transform primaryAttachPoint,
        List<Transform> secondaryAttachPoints = null,
        Color? color = null,
        float? duration = null,
        bool unscaledTime = false)
    {
        foreach (var secondaryAT in secondaryAttachPoints)
        {
            var secondary = secondaryAT;

            var vfx = Get();
            if (vfx == null) return;
            Initialize(vfx, color);
            vfx.transform.SetParent(null);
            vfx.transform.position = primaryAttachPoint.position;
            vfx.Play(m_MoveDuration, unscaledTime);
            var startPos = primaryAttachPoint.position;
            PathAnimator.BezierAnimate(
                () => startPos,
                () =>
                {
                    var pos = primaryAttachPoint.position + secondary.position;
                    pos.y += m_MoveArcHeight;
                    return pos;
                },
                () => secondary.position,
                pos => vfx.transform.position = pos,
                rot => vfx.transform.eulerAngles = rot,
                m_MoveDuration
            );
        }
    }

    private void HandleMovingSequential(
        Transform primaryAttachPoint,
        List<Transform> secondaryAttachPoints = null,
        Color? color = null,
        float? duration = null,
        bool unscaledTime = false)
    {
        var vfx = Get();
        if (vfx == null) return;
        Initialize(vfx, color);
        vfx.transform.SetParent(null);
        vfx.transform.position = primaryAttachPoint.position;
        vfx.Play(m_MoveDuration, unscaledTime);
        var startPos = primaryAttachPoint.position;
        var points = new List<Vector3Producer>() { () => startPos };
        points.AddRange(secondaryAttachPoints.ConvertAll<Vector3Producer>(x => () => x.position));
        PathAnimator.PassThroughPointsAnimate(
            points,
            pos => vfx.transform.position = pos,
            rot => vfx.transform.eulerAngles = rot,
            m_MoveDuration
        );
    }
}
