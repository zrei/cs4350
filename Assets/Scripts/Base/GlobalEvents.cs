using System.Collections.Generic;
using Game.UI;
using Level.Nodes;
using UnityEngine;

public delegate void IntEvent(int _);
public delegate void VoidEvent();
public delegate void FloatEvent(float _);
public delegate void Vector3Event(Vector3 _);
public delegate Vector3 Vector3Producer();
public delegate void StringEvent(string _);
public delegate void BoolEvent(bool _);

public static class GlobalEvents
{
    public static class Battle
    {
        public delegate void UnitEvent(Unit _);
        public delegate void TurnOrderEvent(List<Unit> _);
        public delegate void PhaseEvent(PlayerTurnState _);
        public delegate void BattleOutcomeEvent(UnitAllegiance _, int numTurns);
        public static VoidEvent BattleInitializedEvent;
        public static FloatEvent BattleTimeTickEvent;
        public static UnitEvent UnitDefeatedEvent;
        public static TurnOrderEvent TurnOrderUpdatedEvent;
        public static PhaseEvent PlayerPhaseUpdateEvent;
        public static VoidEvent PlayerUnitSetupStartEvent;
        public static VoidEvent PlayerUnitSetupEndEvent;
        public static VoidEvent PlayerTurnStartEvent;
        public static VoidEvent EnemyTurnStartEvent;
        public static BattleOutcomeEvent BattleEndEvent;
        public static UnitEvent PreviewUnitEvent;
        public static UnitEvent PreviewCurrentUnitEvent;
        public static VoidEvent AttackAnimationEvent; 
        public static VoidEvent CompleteAttackAnimationEvent;

        public static void ClearEvents()
        {
            BattleInitializedEvent = null;
            BattleTimeTickEvent = null;
            UnitDefeatedEvent = null;
            TurnOrderUpdatedEvent = null;
            PlayerPhaseUpdateEvent = null;
            PlayerUnitSetupStartEvent = null;
            PlayerUnitSetupEndEvent = null;
            PlayerTurnStartEvent = null;
            EnemyTurnStartEvent = null;
            BattleEndEvent = null;
            PreviewUnitEvent = null;
            PreviewCurrentUnitEvent = null;
            AttackAnimationEvent = null;
            CompleteAttackAnimationEvent = null;
        }
    }

    public static class Level
    {
        public delegate void NodeEvent(LevelNode _);
        public delegate void LevelResultEvent(LevelSO levelSo, LevelResultType _);
        public static NodeEvent NodeEnteredEvent;
        public static NodeEvent NodeClearedEvent;
        public static NodeEvent NodeExitedEvent;
        public static NodeEvent NodeSelectedEvent;
        public static NodeEvent NodeDeselectedEvent;
        /// <summary>
        /// This event is called the moment the level ends
        /// </summary>
        public static VoidEvent LevelEndEvent;
        /// <summary>
        /// This event is called to pass out the results
        /// </summary>
        public static LevelResultEvent LevelResultsEvent;
        public static NodeEvent NodeHoverStartEvent;
        public static VoidEvent NodeHoverEndEvent;
        public static VoidEvent EndPlayerPhaseEvent;

        public static void ClearEvents()
        {
            NodeEnteredEvent = null;
            NodeClearedEvent = null;
            NodeExitedEvent = null;
            NodeSelectedEvent = null;
            NodeDeselectedEvent = null;
            LevelEndEvent = null;
            LevelResultsEvent = null;
            NodeHoverStartEvent = null;
            NodeHoverEndEvent = null;
            EndPlayerPhaseEvent = null;
        }
    }

    public static class Scene
    {
        public delegate void SceneTransitionEvent(SceneEnum prev, SceneEnum final);
        public delegate void SceneChangeEvent(SceneEnum _);
        public static SceneTransitionEvent OnBeginSceneChange;
        public static SceneChangeEvent OnSceneTransitionEvent;
        public static SceneTransitionEvent OnSceneTransitionCompleteEvent;

        public static void ClearEvents()
        {
            OnBeginSceneChange = null;
            OnSceneTransitionEvent = null;
            OnSceneTransitionCompleteEvent = null;
        }
    }

    public static class Dialogue
    {
        public static VoidEvent DialogueStartEvent;
        public static VoidEvent DialogueEndEvent;

        public static void ClearEvents()
        {
            DialogueStartEvent = null;
            DialogueEndEvent = null;
        }
    }
    
    public static class Morality
    {
        public static IntEvent MoralityChangeEvent;
        public static IntEvent MoralitySetEvent;

        public static void ClearEvents()
        {
            MoralityChangeEvent = null;
            MoralitySetEvent = null;
        }
    }
    
    public static class Rations
    {
        public static FloatEvent RationsChangeEvent;
        public static FloatEvent RationsSetEvent;

        public static void ClearEvents()
        {
            RationsChangeEvent = null;
            RationsSetEvent = null;
        }
    }

    public static class Flags
    {
        public delegate void FlagEvent(string flag, bool value, FlagType flagType);
        public static FlagEvent SetFlagEvent;

        public static void ClearEvents()
        {
            SetFlagEvent = null;
        }
    }

    public static class WorldMap 
    {
        public delegate void LevelEvent(LevelData levelData);
        public static LevelEvent OnGoToLevel;
        public static VoidEvent OnBeginLevelAnimationEvent;
        public static VoidEvent OnEndLevelAnimationEvent;

        public static void ClearEvents()
        {
            OnGoToLevel = null;
            OnBeginLevelAnimationEvent = null;
            OnEndLevelAnimationEvent = null;
        }
    }

    public static class Save
    {
        public static VoidEvent OnBeginSaveEvent;
        public static VoidEvent OnCompleteSaveEvent;

        public static void ClearEvents()
        {
            OnBeginSaveEvent = null;
            OnCompleteSaveEvent = null;
        }
    }

    public static class CharacterManagement 
    {
        public delegate void PlayerClassSOEvent(PlayerClassSO _);
        public static VoidEvent OnWeaponChangedEvent;
        public static PlayerClassSOEvent OnPreviewReclass;
        public static VoidEvent OnReclass;
        public static VoidEvent OnLordUpdate;

        public static void ClearEvents()
        {
            OnWeaponChangedEvent = null;
            OnPreviewReclass = null;
            OnReclass = null;
            OnLordUpdate = null;
        }
    }

    public static class CutsceneEvents
    {
        public delegate void CutsceneTriggerEnumEvent(CutsceneTriggerEnum _);
        public static VoidEvent StartCutsceneEvent;
        public static VoidEvent EndCutsceneEvent;
        public static CutsceneTriggerEnumEvent CutsceneTriggerEvent;

        public static void ClearEvents()
        {
            StartCutsceneEvent = null;
            EndCutsceneEvent = null;
            CutsceneTriggerEvent = null;
        }
    }

    public static void ClearEvents()
    {
        Battle.ClearEvents();
        Level.ClearEvents();
        Scene.ClearEvents();
        Dialogue.ClearEvents();
        Morality.ClearEvents();
        Rations.ClearEvents();
        Flags.ClearEvents();
        WorldMap.ClearEvents();
        Save.ClearEvents();
        CharacterManagement.ClearEvents();
        CutsceneEvents.ClearEvents();
    }
}
