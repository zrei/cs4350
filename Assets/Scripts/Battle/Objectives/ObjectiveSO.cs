using System.Collections.Generic;
using UnityEngine;

public delegate void ObjectiveUpdateEvent(IObjective objective);

[System.Flags]
public enum ObjectiveTag
{
    None,
    WinOnComplete,
    LoseOnFail,
}

public enum ObjectiveState
{
    InProgress,
    Completed,
    Failed,
}

public interface IObjective
{
    event ObjectiveUpdateEvent OnUpdate;
    Color Color { get; }
    ObjectiveState CompletionStatus { get; }
    float DisplayedProgress { get; }
    bool UseProgressBar { get; }
    ObjectiveTag ObjectiveTags { get; }

    void Initialize(params object[] args);
    void UpdateState(params object[] args);
    void Dispose();
    string ToString();
    void Show(bool active);
}

public abstract class ObjectiveSO : ScriptableObject
{
    [Header("Visual Settings")]
    public bool m_IsHiddenObjective;
    public Color m_Color = Color.white;
    public bool m_HideProgressBar;

    public bool m_IsOverrideDisplayText;
    public string m_DisplayTextOverride;

    [Header("Logic Settings")]
    public ObjectiveTag m_ObjectiveType = ObjectiveTag.WinOnComplete;
    public List<DataActionSO> m_OnCompleteActions;
    public List<DataActionSO> m_OnFailActions;

    public IObjective CreateInstance() { return new Objective(this); }
    protected abstract void Initialize(Objective objectiveInstance, params object[] args);
    protected abstract void UpdateState(Objective objectiveInstance, params object[] args);
    protected abstract void Show(Objective objectiveInstance, bool active);

    protected class Objective : IObjective
    {
        public event ObjectiveUpdateEvent OnUpdate;
        public event VoidEvent OnDispose;

        public Color Color => m_ObjectiveSO.m_Color;
        public ObjectiveState CompletionStatus
        {
            get => m_CompletionStatus;
            set
            {
                if (m_CompletionStatus == value) return;

                m_CompletionStatus = value;
                switch (m_CompletionStatus)
                {
                    case ObjectiveState.Completed:
                        m_ObjectiveSO.m_OnCompleteActions?.ForEach(x => x?.Execute());
                        break;
                    case ObjectiveState.Failed:
                        m_ObjectiveSO.m_OnFailActions?.ForEach(x => x?.Execute());
                        break;
                }
            }
        }
        private ObjectiveState m_CompletionStatus = ObjectiveState.InProgress;
        public float DisplayedProgress { get; set; } = 0f;
        public bool UseProgressBar => !m_ObjectiveSO.m_HideProgressBar;
        public string DisplayText { get; set; } = string.Empty;
        public ObjectiveTag ObjectiveTags => m_ObjectiveSO.m_ObjectiveType;

        public object m_StateData;

        public readonly ObjectiveSO m_ObjectiveSO;
        public Objective(ObjectiveSO objectiveSO)
        {
            m_ObjectiveSO = objectiveSO;
        }

        public void Initialize(params object[] args)
        {
            m_ObjectiveSO.Initialize(this, args);
            UpdateState();
        }

        public void UpdateState(params object[] args)
        {
            m_ObjectiveSO.UpdateState(this, args);
            OnUpdate?.Invoke(this);
        }

        public void Dispose()
        {
            OnDispose?.Invoke();
            OnDispose = null;
        }

        public override string ToString()
        {
            if (m_ObjectiveSO.m_IsOverrideDisplayText) return m_ObjectiveSO.m_DisplayTextOverride;

            return DisplayText;
        }

        public void Show(bool active)
        {
            m_ObjectiveSO.Show(this, active);
        }
    }
}
