using System.Collections.Generic;
using Game.UI;
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

    public static class UI
    {
        public delegate void PartyEvent(List<PlayerCharacterData> _, bool inLevel);
        public static PartyEvent OpenPartyOverviewEvent;
        public static VoidEvent OnClosePartyOverviewEvent;
        public static VoidEvent SavePartyChangesEvent;

        public static void ClearEvents()
        {
            OpenPartyOverviewEvent = null;
            OnClosePartyOverviewEvent = null;
            SavePartyChangesEvent = null;
        }
    }

    public static class Battle
    {
        public delegate void UnitEvent(Unit _);
        public delegate void TurnOrderEvent(List<Unit> _);
        public delegate void PhaseEvent(PlayerTurnState _);
        public delegate void BattleOutcomeEvent(UnitAllegiance _, int numTurns);
        public delegate void AttackEvent(ActiveSkillSO activeSkill, Unit attacker, List<Unit> target);
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
        public static AttackEvent AttackAnimationEvent; 
        public static VoidEvent CompleteAttackAnimationEvent;
        public static VoidEvent ReturnFromBattleEvent;

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
            ReturnFromBattleEvent = null;
        }
    }

    public static class Level
    {
        public delegate void NodeEvent(NodeInternal _);
        public delegate void NodeTransitionEvent(NodeInternal _1, NodeInternal _2);
        public delegate void BattleNodeEvent(BattleNode _);
        public delegate void BattleNodeResultEvent(BattleNode battleNode, UnitAllegiance victor, int numTurns);
        public delegate void RewardNodeEvent(RewardNode _);
        public delegate void DialogueNodeEvent(DialogueNode _);
        public delegate void ExpSummaryEvent(List<ExpGainSummary> _);
        public delegate void MassLevelUpEvent(List<LevelUpSummary> _);
        public delegate void LevelResultEvent(LevelSO levelSo, LevelResultType _);
        public static NodeEvent NodeEnteredEvent;
        public static NodeEvent NodeClearedEvent;
        public static NodeEvent NodeExitedEvent;
        public static NodeEvent NodeSelectedEvent;
        public static NodeEvent NodeDeselectedEvent;
        public static NodeTransitionEvent NodeMovementEvent;
        public static BattleNodeEvent BattleNodeStartEvent;
        public static BattleNodeResultEvent BattleNodeEndEvent;
        public static RewardNodeEvent RewardNodeStartEvent;
        public static VoidEvent CloseRewardScreenEvent;
        public static DialogueNodeEvent DialogueNodeEndEvent;
        public static ExpSummaryEvent ExpGainEvent;
        public static VoidEvent CompleteExpGainEvent;
        public static MassLevelUpEvent MassLevellingEvent;
        public static VoidEvent CloseLevellingScreenEvent;
        /// <summary>
        /// This event is called the moment the level ends
        /// </summary>
        public static VoidEvent LevelEndEvent;
        /// <summary>
        /// This event is called to pass out the results
        /// </summary>
        public static LevelResultEvent LevelResultsEvent;
        public static VoidEvent ReturnFromLevelEvent;
        public static NodeEvent NodeHoverStartEvent;
        public static VoidEvent NodeHoverEndEvent;
        public static VoidEvent StartPlayerPhaseEvent;
        public static VoidEvent EndPlayerPhaseEvent;

        public static void ClearEvents()
        {
            NodeEnteredEvent = null;
            NodeClearedEvent = null;
            NodeExitedEvent = null;
            NodeSelectedEvent = null;
            NodeDeselectedEvent = null;
            NodeMovementEvent = null;
            BattleNodeStartEvent = null;
            BattleNodeEndEvent = null;
            RewardNodeStartEvent = null;
            CloseRewardScreenEvent = null;
            DialogueNodeEndEvent = null;
            ExpGainEvent = null;
            CompleteExpGainEvent = null;
            MassLevellingEvent = null;
            CloseLevellingScreenEvent = null;
            LevelEndEvent = null;
            LevelResultsEvent = null;
            ReturnFromLevelEvent = null;
            NodeHoverStartEvent = null;
            NodeHoverEndEvent = null;
            StartPlayerPhaseEvent = null;
            EndPlayerPhaseEvent = null;
        }
    }

    public static class Scene
    {
        public static VoidEvent BattleSceneLoadedEvent;
        public static VoidEvent LevelSceneLoadedEvent;
        public static VoidEvent WorldMapSceneLoadedEvent;
        public static VoidEvent MainMenuSceneLoadedEvent;
        public static VoidEvent EarlyQuitEvent;

        public static void ClearEvents()
        {
            BattleSceneLoadedEvent = null;
            LevelSceneLoadedEvent = null;
            WorldMapSceneLoadedEvent = null;
            MainMenuSceneLoadedEvent = null;
            EarlyQuitEvent = null;
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
        public delegate void LevelSOEvent(LevelSO levelSO);
        public static LevelEvent OnGoToLevel;
        public static VoidEvent OnBeginLoadLevelEvent;
        public static LevelSOEvent OnPartySelectEvent;
        public static VoidEvent OnEndPreCutsceneEvent;
        public static VoidEvent OpenPartyManagementEvent;

        public static void ClearEvents()
        {
            OnGoToLevel = null;
            OnBeginLoadLevelEvent = null;
            OnPartySelectEvent = null;
            OnEndPreCutsceneEvent = null;
            OpenPartyManagementEvent = null;
        }
    }

    public static class MainMenu
    {
        public static VoidEvent OnBeginLoadWorldMap;
        public static VoidEvent OnReturnToMainMenu;

        public static void ClearEvents()
        {
            OnBeginLoadWorldMap = null;
            OnReturnToMainMenu = null;
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
        UI.ClearEvents();
        Battle.ClearEvents();
        Level.ClearEvents();
        Scene.ClearEvents();
        Dialogue.ClearEvents();
        Morality.ClearEvents();
        Rations.ClearEvents();
        Flags.ClearEvents();
        WorldMap.ClearEvents();
        MainMenu.ClearEvents();
        Save.ClearEvents();
        CharacterManagement.ClearEvents();
        CutsceneEvents.ClearEvents();
    }
}
