using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PathAnimator
{
    public static Coroutine BezierAnimate(
        Vector3Producer start,
        Vector3Producer middle,
        Vector3Producer end,
        Vector3Event posSetter,
        Vector3Event rotSetter,
        float duration = 0.25f,
        bool unscaledTime = false,
        VoidEvent onComplete = null)
    {
        IEnumerator AnimateCo()
        {
            if (middle == null)
            {
                var middlePos = (start() + end()) / 2;
                middle = () => middlePos;
            }

            Vector3 pos = start();

            var t = 0f;
            var progress = 0f;
            Vector3 delta = Vector3.zero;
            while (t < duration)
            {
                t += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                progress = t / duration;
                var nextPos = Vector3.Lerp(Vector3.Lerp(start(), middle(), progress), Vector3.Lerp(middle(), end(), progress), progress);
                delta = nextPos - pos;
                pos = nextPos;
                posSetter?.Invoke(pos);
                if (delta.sqrMagnitude > 0.01f) rotSetter?.Invoke(Quaternion.LookRotation(delta).eulerAngles);
                yield return null;
            }

            posSetter?.Invoke(end());
            onComplete?.Invoke();
        }

        return CoroutineManager.Instance.StartCoroutine(AnimateCo());
    }

    public static Coroutine PassThroughPointsAnimate(
        List<Vector3Producer> points,
        Vector3Event posSetter,
        Vector3Event rotSetter,
        float duration = 0.25f,
        bool unscaledTime = false,
        VoidEvent onComplete = null)
    {
        IEnumerator AnimateCo()
        {
            var durationPerPoint = duration / (points.Count - 1);
            for (int i = 0; i < points.Count - 1; i++)
            {
                var curr = points[i];
                var next = points[i + 1];
                var t = 0f;
                var progress = 0f;
                var pos = curr();
                Vector3 delta = Vector3.zero;
                while (t < durationPerPoint)
                {
                    t += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                    progress = t / durationPerPoint;
                    var nextPos = Vector3.Lerp(curr(), next(), progress);
                    delta = nextPos - pos;
                    pos = nextPos;
                    posSetter?.Invoke(pos);
                    if (delta.sqrMagnitude > 0.01f) rotSetter?.Invoke(Quaternion.LookRotation(delta).eulerAngles);
                    yield return null;
                }

                posSetter?.Invoke(next());
            }

            onComplete?.Invoke();
        }

        return CoroutineManager.Instance.StartCoroutine(AnimateCo());
    }
}
