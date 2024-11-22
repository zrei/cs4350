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
        MovingPrimaryToSecondaryAverage,
        MovingPrimaryToSecondaryAll,
        MovingSequential
    }

    public Type m_Type;
    public VFXSystem m_VFXPrefab;
    public List<VFXAudio> m_VFXAudio;

    [Header("Overrides")]
    public bool m_IsOverrideStartColor = false;
    public Color m_StartColorOverride = Color.white;
    public bool m_IsOverrideDuration = false;
    public float m_DurationOverride = 0.0f;

    [Header("Spawn Configuration")]
    public int m_ExtraSpawnCount = 0;
    public float m_SpawnRandomOffset = 0f;
    public bool m_ApplyRandomToFirst = false;

    [Header("Moving Primary To Secondary Configuration")]
    public float m_MoveArcHeight = 0.0f;
    public float m_MoveDuration = 0.2f;

    [Header("Moving Sequential Configuration")]
    public bool m_MoveSequentialReturnToPrimary = false;
    public int m_MoveSequentialExtraLoopCount = 0;

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

    public virtual VoidEvent Play(
        Transform primaryAttachPoint,
        List<Transform> secondaryAttachPoints = null,
        Color? color = null,
        float? duration = null,
        bool unscaledTime = false,
        VoidEvent onComplete = null)
    {
        foreach (VFXAudio vFXAudio in m_VFXAudio)
        {
            SoundManager.Instance.Play(vFXAudio.m_AudioDataSO, vFXAudio.m_AudioDelay);
        }

        if (color == null && m_IsOverrideStartColor)
        {
            color = m_StartColorOverride;
        }

        if (duration == null && m_IsOverrideDuration)
        {
            duration = m_DurationOverride;
        }

        List<VFXSystem> vfxs = new();
        switch (m_Type)
        {
            case Type.StationaryAttachPrimary:
                HandleStationaryAttachPrimary(vfxs, primaryAttachPoint, secondaryAttachPoints, color, duration, unscaledTime);
                break;
            case Type.StationaryAttachSecondaryFirst:
                HandleStationaryAttachSecondaryFirst(vfxs, primaryAttachPoint, secondaryAttachPoints, color, duration, unscaledTime);
                break;
            case Type.StationaryAttachSecondaryAll:
                HandleStationaryAttachSecondaryAll(vfxs, primaryAttachPoint, secondaryAttachPoints, color, duration, unscaledTime);
                break;
            case Type.MovingPrimaryToSecondaryAverage:
                HandleMovingPrimaryToSecondaryAverage(vfxs, primaryAttachPoint, secondaryAttachPoints, color, duration, unscaledTime);
                break;
            case Type.MovingPrimaryToSecondaryAll:
                HandleMovingPrimaryToSecondaryAll(vfxs, primaryAttachPoint, secondaryAttachPoints, color, duration, unscaledTime);
                break;
            case Type.MovingSequential:
                HandleMovingSequential(vfxs, primaryAttachPoint, secondaryAttachPoints, color, duration, unscaledTime);
                break;
        }

        if (onComplete != null)
        {
            bool isOnCompleteInvoked = false;
            void OnStop(VFXSystem vfx)
            {
                vfx.onStop -= OnStop;
                if (!isOnCompleteInvoked)
                {
                    isOnCompleteInvoked = true;
                    onComplete?.Invoke();
                }
            }
            foreach (var vfx in vfxs) vfx.onStop += OnStop;
        }

        return Stop;
        void Stop()
        {
            if (vfxs == null) return;
            foreach (var vfx in vfxs)
            {
                vfx.Stop();
            }
        }
    }

    private void HandleStationaryAttachSingle(
        List<VFXSystem> vfxs,
        Transform parent,
        Color? color = null,
        float? duration = null,
        bool unscaledTime = false,
        bool applyRandomOffset = false)
    {
        var vfx = Get();
        if (vfx == null) return;
        vfx.SetStartColor(color);
        vfx.transform.SetParent(parent, false);
        vfx.transform.localPosition = Vector3.zero;
        if (applyRandomOffset)
        {
            vfx.transform.position += Random.insideUnitSphere * m_SpawnRandomOffset;
        }
        vfx.transform.rotation = Quaternion.identity;
        vfx.Play(duration, unscaledTime);
        vfxs.Add(vfx);
    }

    private void HandleStationaryAttachPrimary(
        List<VFXSystem> vfxs,
        Transform primaryAttachPoint,
        List<Transform> secondaryAttachPoints = null,
        Color? color = null,
        float? duration = null,
        bool unscaledTime = false)
    {
        HandleStationaryAttachSingle(vfxs, primaryAttachPoint, color, duration, unscaledTime, m_ApplyRandomToFirst);

        for (var i = 0; i < m_ExtraSpawnCount; i++)
        {
            HandleStationaryAttachSingle(vfxs, primaryAttachPoint, color, duration, unscaledTime, true);
        }
    }

    private void HandleStationaryAttachSecondaryFirst(
        List<VFXSystem> vfxs,
        Transform primaryAttachPoint,
        List<Transform> secondaryAttachPoints = null,
        Color? color = null,
        float? duration = null,
        bool unscaledTime = false)
    {
        var secondaryAT = secondaryAttachPoints[0];
        HandleStationaryAttachSingle(vfxs, secondaryAT, color, duration, unscaledTime, m_ApplyRandomToFirst);
        for (var i = 0; i < m_ExtraSpawnCount; i++)
        {
            HandleStationaryAttachSingle(vfxs, secondaryAT, color, duration, unscaledTime, true);
        }
    }

    private void HandleStationaryAttachSecondaryAll(
        List<VFXSystem> vfxs,
        Transform primaryAttachPoint,
        List<Transform> secondaryAttachPoints = null,
        Color? color = null,
        float? duration = null,
        bool unscaledTime = false)
    {
        foreach (var secondaryAT in secondaryAttachPoints)
        {
            HandleStationaryAttachSingle(vfxs, secondaryAT, color, duration, unscaledTime, m_ApplyRandomToFirst);
            for (var i = 0; i < m_ExtraSpawnCount; i++)
            {
                HandleStationaryAttachSingle(vfxs, secondaryAT, color, duration, unscaledTime, true);
            }
        }
    }

    private void HandleMovingPrimaryToSecondarySingle(
        List<VFXSystem> vfxs,
        Vector3Producer startPos,
        Vector3Producer midPos,
        Vector3Producer endPos,
        Color? color = null,
        float? duration = null,
        bool unscaledTime = false,
        bool applyRandomOffset = false)
    {
        var vfx = Get();
        if (vfx == null) return;
        vfx.SetStartColor(color);
        vfx.transform.SetParent(null);
        vfx.transform.position = startPos();

        if (applyRandomOffset)
        {
            var randMidPos = midPos() + Random.insideUnitSphere * m_SpawnRandomOffset;
            midPos = () => randMidPos;
        }

        vfx.Play(m_MoveDuration, unscaledTime);
        var animCo = PathAnimator.BezierAnimate(
            startPos,
            midPos,
            endPos,
            pos => vfx.transform.position = pos,
            rot => vfx.transform.eulerAngles = rot,
            m_MoveDuration,
            unscaledTime
        );
        void OnStop(VFXSystem x)
        {
            x.onStop -= OnStop;
            CoroutineManager.Instance.StopCoroutine(animCo);
        }
        vfx.onStop += OnStop;
        vfxs.Add(vfx);
    }

    private void HandleMovingPrimaryToSecondaryAverage(
        List<VFXSystem> vfxs,
        Transform primaryAttachPoint,
        List<Transform> secondaryAttachPoints = null,
        Color? color = null,
        float? duration = null,
        bool unscaledTime = false)
    {
        var startPos = primaryAttachPoint.position;
        var endPos = Vector3.zero;
        secondaryAttachPoints.ForEach(x => endPos += x.position);
        endPos /= secondaryAttachPoints.Count;
        var midPos = (startPos + endPos) / 2;
        midPos.y += m_MoveArcHeight;

        Vector3Producer startPosProducer = () => startPos;
        Vector3Producer endPosProducer = () => endPos;
        Vector3Producer midPosProducer = () => midPos;

        HandleMovingPrimaryToSecondarySingle(vfxs, startPosProducer, midPosProducer, endPosProducer, color, duration, unscaledTime, m_ApplyRandomToFirst);
        for (var i = 0; i < m_ExtraSpawnCount; i++)
        {
            HandleMovingPrimaryToSecondarySingle(vfxs, startPosProducer, midPosProducer, endPosProducer, color, duration, unscaledTime, true);
        }
    }

    private void HandleMovingPrimaryToSecondaryAll(
        List<VFXSystem> vfxs,
        Transform primaryAttachPoint,
        List<Transform> secondaryAttachPoints = null,
        Color? color = null,
        float? duration = null,
        bool unscaledTime = false)
    {
        foreach (var secondaryAT in secondaryAttachPoints)
        {
            var startPos = primaryAttachPoint.position;
            var endPos = secondaryAT.position;
            var midPos = (startPos + endPos) / 2;
            midPos.y += m_MoveArcHeight;

            Vector3Producer startPosProducer = () => startPos;
            Vector3Producer endPosProducer = () => endPos;
            Vector3Producer midPosProducer = () => midPos;

            HandleMovingPrimaryToSecondarySingle(vfxs, startPosProducer, midPosProducer, endPosProducer, color, duration, unscaledTime, m_ApplyRandomToFirst);
            for (var i = 0; i < m_ExtraSpawnCount; i++)
            {
                HandleMovingPrimaryToSecondarySingle(vfxs, startPosProducer, midPosProducer, endPosProducer, color, duration, unscaledTime, true);
            }
        }
    }

    private void HandleMovingSequentialSingle(
        List<VFXSystem> vfxs,
        List<Vector3Producer> points,
        Color? color = null,
        float? duration = null,
        bool unscaledTime = false,
        bool applyRandomOffset = false)
    {
        var vfx = Get();
        if (vfx == null) return;
        vfx.SetStartColor(color);
        vfx.transform.SetParent(null);

        if (applyRandomOffset)
        {
            for (var i = 0; i < points.Count; i++)
            {
                var randPos = points[i]() + Random.insideUnitSphere * m_SpawnRandomOffset;
                points[i] = () => randPos;
            }
        }

        vfx.transform.position = points[0]();
        vfx.Play(m_MoveDuration, unscaledTime);

        var animCo = PathAnimator.PassThroughPointsAnimate(
            points,
            pos => vfx.transform.position = pos,
            rot => vfx.transform.eulerAngles = rot,
            m_MoveDuration,
            unscaledTime
        );
        void OnStop(VFXSystem x)
        {
            x.onStop -= OnStop;
            CoroutineManager.Instance.StopCoroutine(animCo);
        }
        vfx.onStop += OnStop;
        vfxs.Add(vfx);
    }

    private void HandleMovingSequential(
        List<VFXSystem> vfxs,
        Transform primaryAttachPoint,
        List<Transform> secondaryAttachPoints = null,
        Color? color = null,
        float? duration = null,
        bool unscaledTime = false)
    {
        var startPos = primaryAttachPoint.position;
        var points = new List<Vector3Producer>() { () => startPos };

        var secondaryPoints = new List<Vector3Producer>();
        foreach (var secondaryAT in secondaryAttachPoints)
        {
            var secondaryPos = secondaryAT.position;
            secondaryPoints.Add(() => secondaryPos);
        }
        if (m_MoveSequentialReturnToPrimary)
        {
            secondaryPoints.Add(points[0]);
        }

        points.AddRange(secondaryPoints);
        for (var i = 0; i < m_MoveSequentialExtraLoopCount; i++)
        {
            points.AddRange(secondaryPoints);
        }

        HandleMovingSequentialSingle(vfxs, points, color, duration, unscaledTime, m_ApplyRandomToFirst);
        for (var i = 0; i < m_ExtraSpawnCount; i++)
        {
            HandleMovingSequentialSingle(vfxs, points, color, duration, unscaledTime, true);
        }
    }
}
