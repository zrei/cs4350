using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TimeObjectiveSO", menuName = "ScriptableObject/Objective/TimeObjectiveSO")]
public class TimeObjectiveSO : ObjectiveSO
{
    public float m_TurnLimit;
    public bool m_IsFailOnTimeLimitReached;

    private class Data
    {
        public float m_ElapsedTime;
    }

    protected override void Initialize(Objective objectiveInstance, params object[] args)
    {
        if (!BattleManager.IsReady)
        {
            Debug.LogError("Trying to initialize battle objective outside of battle");
            return;
        }

        var stateData = new Data();
        objectiveInstance.m_StateData = stateData;

        stateData.m_ElapsedTime = BattleManager.Instance.TotalBattleTime;

        GlobalEvents.Battle.BattleTimeTickEvent += OnBattleTimeTick;
        objectiveInstance.OnDispose += OnDispose;

        void OnBattleTimeTick(float time)
        {
            stateData.m_ElapsedTime = time;
            objectiveInstance.UpdateState();
        }

        void OnDispose()
        {
            objectiveInstance.OnDispose -= OnDispose;
            GlobalEvents.Battle.BattleTimeTickEvent -= OnBattleTimeTick;
        }
    }

    protected override void UpdateState(Objective objectiveInstance, params object[] args)
    {
        var stateData = objectiveInstance.m_StateData as Data;
        if (stateData == null) return;

        var timeLimit = TurnQueue.CyclesToTime(m_TurnLimit);
        var progress = m_TurnLimit == 0 ? 1f : Mathf.Clamp01(stateData.m_ElapsedTime / timeLimit);
        objectiveInstance.CompletionStatus = progress >= 1
            ? (m_IsFailOnTimeLimitReached ? ObjectiveState.Failed : ObjectiveState.Completed)
            : ObjectiveState.InProgress;
        objectiveInstance.DisplayedProgress = m_IsFailOnTimeLimitReached ? 1 - progress : progress;
        if (m_IsFailOnTimeLimitReached)
        {
            objectiveInstance.DisplayText = $"Time limit: {Mathf.Clamp(timeLimit - stateData.m_ElapsedTime, 0, timeLimit):F1} <sprite name=\"TimeToAct\" tint>";
        }
        else
        {
            objectiveInstance.DisplayText = $"Survive: {Mathf.Clamp(timeLimit - stateData.m_ElapsedTime, 0, timeLimit):F1} <sprite name=\"TimeToAct\" tint>";
        }
    }

    protected override void Show(Objective objectiveInstance, bool active)
    {
    }
}
