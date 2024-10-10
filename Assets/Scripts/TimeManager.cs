using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
    protected override void HandleAwake()
    {
        base.HandleAwake();

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    public void ModifyTime(float timeScaleMultiplier, float realTimeDuration)
    {
        if (timeScaleMultiplier < 0) return;
        StartCoroutine(ModifyTimeCoroutine(timeScaleMultiplier, realTimeDuration));
    }

    private IEnumerator ModifyTimeCoroutine(float timeScaleMultiplier, float realTimeDuration)
    {
        var prevTimeScale = Time.timeScale;
        Time.timeScale *= timeScaleMultiplier;

        yield return new WaitForSecondsRealtime(realTimeDuration);

        if (timeScaleMultiplier == 0) Time.timeScale = prevTimeScale;
        else Time.timeScale /= timeScaleMultiplier;
    }
}
