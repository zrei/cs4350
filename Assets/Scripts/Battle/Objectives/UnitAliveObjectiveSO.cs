using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ProtectUnitObjectiveSO", menuName = "ScriptableObject/Objective/ProtectUnitObjectiveSO")]
public class UnitAliveObjectiveSO : ObjectiveSO
{
    public enum Mode
    {
        FailIfAnyDie,
        FailIfAllDie,
        CompleteIfAnyDie,
        CompleteIfAllDie,
    }

    private class Data
    {
        public int m_TotalCount;
        public HashSet<Unit> m_TrackedUnits;
        public Unit m_MainUnit;
    }

    [Header("Note: This works with enemies to create \"Boss\" type objectives.")]
    [Space(10)]

    [Tooltip("Tracked player characters for objective")]
    public List<CharacterSO> m_PlayerCharacterSOs;
    [Tooltip("Tracked enemy characters for objective")]
    public List<EnemyCharacterSO> m_EnemyCharacterSOs;
    public Mode m_Mode = Mode.FailIfAnyDie;

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

        var protectedPlayerSet = new HashSet<int>(m_PlayerCharacterSOs.ConvertAll(x => x.GetInstanceID()));
        var playerUnits = battleManager.PlayerUnits;
        foreach (var unit in playerUnits)
        {
            if (!protectedPlayerSet.Contains(unit.CharacterSOInstanceID)) continue;

            stateData.m_TrackedUnits.Add(unit);
            stateData.m_TotalCount++;
        }

        var protectedEnemySet = new HashSet<int>(m_EnemyCharacterSOs.ConvertAll(x => x.GetInstanceID()));
        var enemyUnits = battleManager.EnemyUnits;
        foreach (var unit in enemyUnits)
        {
            if (!protectedEnemySet.Contains(unit.CharacterSOInstanceID)) continue;

            stateData.m_TrackedUnits.Add(unit);
            stateData.m_TotalCount++;
        }

        GlobalEvents.Battle.UnitDefeatedEvent += OnUnitDefeated;
        if (stateData.m_TotalCount == 1)
        {
            stateData.m_MainUnit = stateData.m_TrackedUnits.First();
            stateData.m_MainUnit.OnHealthChange += OnHealthChange;
        }
        objectiveInstance.OnDispose += OnDispose;

        void OnUnitDefeated(Unit unit)
        {
            stateData.m_TrackedUnits.Remove(unit);
            objectiveInstance.UpdateState();
        }

        void OnHealthChange(float change, float current, float max)
        {
            objectiveInstance.UpdateState();
        }

        void OnDispose()
        {
            objectiveInstance.OnDispose -= OnDispose;
            GlobalEvents.Battle.UnitDefeatedEvent -= OnUnitDefeated;
            if (stateData.m_MainUnit != null)
            {
                stateData.m_MainUnit.OnHealthChange -= OnHealthChange;
            }
        }
    }

    protected override void UpdateState(Objective objectiveInstance, params object[] args)
    {
        var stateData = objectiveInstance.m_StateData as Data;
        if (stateData == null) return;

        var progress = stateData.m_TotalCount == 0 ? 0f : (float)stateData.m_TrackedUnits.Count / stateData.m_TotalCount;
        switch (m_Mode)
        {
            case Mode.FailIfAnyDie:
                objectiveInstance.CompletionStatus = progress < 1 ? ObjectiveState.Failed : ObjectiveState.InProgress;
                break;
            case Mode.FailIfAllDie:
                objectiveInstance.CompletionStatus = Mathf.Approximately(progress, 0) || progress <= 0 ? ObjectiveState.Failed : ObjectiveState.InProgress;
                break;
            case Mode.CompleteIfAnyDie:
                objectiveInstance.CompletionStatus = progress < 1 ? ObjectiveState.Completed : ObjectiveState.InProgress;
                break;
            case Mode.CompleteIfAllDie:
                objectiveInstance.CompletionStatus = Mathf.Approximately(progress, 0) || progress <= 0 ? ObjectiveState.Completed : ObjectiveState.InProgress;
                break;
        }

        var actionText = string.Empty;
        switch (m_Mode)
        {
            case Mode.FailIfAnyDie:
            case Mode.FailIfAllDie:
                actionText = "Protect";
                break;
            case Mode.CompleteIfAnyDie:
            case Mode.CompleteIfAllDie:
                actionText = "Defeat";
                break;
        }

        if (stateData.m_TotalCount == 1 && stateData.m_MainUnit != null)
        {
            objectiveInstance.DisplayedProgress = stateData.m_MainUnit.CurrentHealthProportion;
            objectiveInstance.DisplayText = $"{actionText}: {stateData.m_MainUnit.CharacterName}";
        }
        else
        {
            objectiveInstance.DisplayedProgress = progress;
            objectiveInstance.DisplayText = $"{actionText}: {string.Join(", ", stateData.m_TrackedUnits.ToList().ConvertAll(x => x.CharacterName))}";
        }
    }

    protected override void Show(Objective objectiveInstance, bool active)
    {
        var stateData = objectiveInstance.m_StateData as Data;
        if (stateData == null) return;

        bool isProtect = m_Mode == Mode.FailIfAnyDie || m_Mode == Mode.FailIfAllDie;
        foreach (var unit in stateData.m_TrackedUnits)
        {
            var objectiveMarker = unit.UnitMarker;
            objectiveMarker.SetColor(m_Color);
            if (isProtect)
            {
                objectiveMarker.SetMarkerType(UnitMarker.IconType.Lord);
            }
            else
            {
                if (unit is EnemyUnit enemyUnit && enemyUnit.m_EnemyTags.HasFlag(EnemyTag.Boss))
                {
                    objectiveMarker.SetMarkerType(UnitMarker.IconType.Boss);
                }
                else
                {
                    objectiveMarker.SetMarkerType(UnitMarker.IconType.Enemy);
                }
            }
            objectiveMarker.SetActive(active);
        }
    }

    public override string ToString()
    {
        var actionText = string.Empty;
        switch (m_Mode)
        {
            case Mode.FailIfAnyDie:
            case Mode.FailIfAllDie:
                actionText = "Protect";
                break;
            case Mode.CompleteIfAnyDie:
            case Mode.CompleteIfAllDie:
                actionText = "Defeat";
                break;
        }
        
        var trackedUnitNames = m_PlayerCharacterSOs.ConvertAll(x => x.m_CharacterName);
        trackedUnitNames.AddRange(m_EnemyCharacterSOs.ConvertAll(x => x.m_CharacterName));
        
        return $"{actionText}: {string.Join(", ", trackedUnitNames)}";
    }
}
