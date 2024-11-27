using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum EnemyTag
{
    None,
    Default,
    Boss,
}

[CreateAssetMenu(fileName = "DefeatEnemiesObjectiveSO", menuName = "ScriptableObject/Objective/DefeatEnemiesObjectiveSO")]
public class DefeatEnemiesObjectiveSO : ObjectiveSO
{
    private class Data
    {
        public int m_TotalCount;
        public HashSet<EnemyUnit> m_TrackedUnits;
    }

    public EnemyTag m_EnemyTags = EnemyTag.Default;

    protected override void Initialize(Objective objectiveInstance, params object[] args)
    {
        if (!BattleManager.IsReady)
        {
            Debug.LogError("Trying to initialize battle objective outside of battle");
            return;
        }

        var stateData = new Data()
        {
            m_TotalCount = 0,
            m_TrackedUnits = new()
        };
        objectiveInstance.m_StateData = stateData;

        var battleManager = BattleManager.Instance;
        var units = battleManager.EnemyUnits;
        foreach (var unit in units)
        {
            if (unit is not EnemyUnit enemyUnit) continue;
            if (!m_EnemyTags.HasFlag(enemyUnit.m_EnemyTags)) continue;

            stateData.m_TrackedUnits.Add(enemyUnit);
            stateData.m_TotalCount++;
        }

        GlobalEvents.Battle.UnitDefeatedEvent += OnUnitDefeated;
        objectiveInstance.OnDispose += OnDispose;

        void OnUnitDefeated(Unit unit)
        {
            if (unit is not EnemyUnit enemyUnit) return;

            if (stateData.m_TrackedUnits.Remove(enemyUnit))
            {
                objectiveInstance.UpdateState();
            }
        }

        void OnDispose()
        {
            objectiveInstance.OnDispose -= OnDispose;
            GlobalEvents.Battle.UnitDefeatedEvent -= OnUnitDefeated;
        }
    }

    protected override void UpdateState(Objective objectiveInstance, params object[] args)
    {
        var stateData = objectiveInstance.m_StateData as Data;
        if (stateData == null) return;

        var progress = stateData.m_TotalCount == 0 ? 0f : 1 - (float)stateData.m_TrackedUnits.Count / stateData.m_TotalCount;
        objectiveInstance.DisplayedProgress = progress;
        objectiveInstance.CompletionStatus = progress >= 1 ? ObjectiveState.Completed : ObjectiveState.InProgress;
        objectiveInstance.DisplayText = $"Defeat all enemies: ({stateData.m_TotalCount - stateData.m_TrackedUnits.Count}/{stateData.m_TotalCount})";
    }

    protected override void Show(Objective objectiveInstance, bool active)
    {
        var stateData = objectiveInstance.m_StateData as Data;
        if (stateData == null) return;

        foreach (var unit in stateData.m_TrackedUnits)
        {
            var objectiveMarker = unit.UnitMarker;
            objectiveMarker.SetColor(m_Color);
            if (unit.m_EnemyTags.HasFlag(EnemyTag.Boss))
            {
                objectiveMarker.SetMarkerType(UnitMarker.IconType.Boss);
            }
            else
            {
                objectiveMarker.SetMarkerType(UnitMarker.IconType.Enemy);
            }
            objectiveMarker.SetActive(active);
        }
    }
    
    public override string ToString()
    {
        return "Defeat all enemies";
    }
}
