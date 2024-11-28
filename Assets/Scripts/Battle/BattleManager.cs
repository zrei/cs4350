using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Input;
using Game;
using System.Linq;
using Game.UI;

[RequireComponent(typeof(PlayerTurnManager))]
[RequireComponent(typeof(EnemyTurnManager))]
[RequireComponent(typeof(PlayerUnitSetup))]
public class BattleManager : Singleton<BattleManager>
{
    [Header("Unit Prefabs")]
    [SerializeField] private PlayerUnit m_PlayerUnit;
    [SerializeField] private EnemyUnit m_EnemyUnit;

    [Header("References")]
    [SerializeField] private MapLogic m_MapLogic;
    [SerializeField] private Transform m_CameraLookAtPoint;
    [SerializeField] private Transform m_MapBiomeParent;

    [Header("SFX")]
    [SerializeField] private AudioDataSO m_VictorySFX;

    #region Player Setup
    // for initial battlefield setup
    private PlayerUnitSetup m_PlayerUnitSetup;
    #endregion

    #region Turn Managers
    // turn managers for the player and enemy
    private PlayerTurnManager m_PlayerTurnManager;
    private EnemyTurnManager m_EnemyTurnManager;

    public PlayerUnitSetup PlayerUnitSetup => m_PlayerUnitSetup;
    public PlayerTurnManager PlayerTurnManager => m_PlayerTurnManager;
    #endregion

    #region Turn Queue
    public float TotalBattleTime => m_TurnQueue.TotalTime;

    private TurnQueue m_TurnQueue = new TurnQueue();
    private HashSet<Unit> m_AllPlayerUnits = new HashSet<Unit>();
    private HashSet<Unit> m_AllEnemyUnits = new HashSet<Unit>();

    public HashSet<Unit> PlayerUnits => m_AllPlayerUnits;
    public HashSet<Unit> EnemyUnits => m_AllEnemyUnits;

    private const float DELAY_TILL_NEXT_TURN = 0.3f;
    #endregion

    #region State
    private bool m_BattleTick = false;
    private bool m_WithinBattle = false;
    private bool m_HasBattleConcluded = false;
    private BattleSO m_CurrBattleSO = null;
    #endregion

    #region Objectives
    public IEnumerable<IObjective> Objectives => m_Objectives;
    private HashSet<IObjective> m_Objectives = new();
    
    // still use this for tracking whether Lord character is Alive
    private HashSet<Unit> m_TrackedPlayerUnits = new HashSet<Unit>();
    #endregion

    #region Current Level State
    private float m_CurrMoralityPercentage;
    private List<InflictedToken> m_PermanentFatigueTokens;
    #endregion

    #region Camera
    private const float CAMERA_ROTATION_SPEED = 50f;
    #endregion

    #region BGM
    private int? m_BattleBGM = null;
    #endregion

    #region Initialisation
    private bool isBattleInitialised = false;
    
    /*
    private void Start()
    {
        m_PlayerTurnManager = GetComponent<PlayerTurnManager>();
        m_EnemyTurnManager = GetComponent<EnemyTurnManager>();
        m_PlayerUnitSetup = GetComponent<PlayerUnitSetup>();

        InputManager.Instance.PrimaryAxisInput.OnHoldEvent += OnRotateCamera;

        m_PlayerTurnManager.Initialise(OnCompleteTurn, m_MapLogic);
        m_EnemyTurnManager.Initialise(OnCompleteTurn, m_MapLogic);
        m_PlayerUnitSetup.Initialise(m_MapLogic, OnCompleteSetup);

        GlobalEvents.Scene.BattleSceneLoadedEvent?.Invoke();
    }
    */

    protected override void HandleAwake()
    {
        base.HandleAwake();
        GlobalEvents.Battle.UnitDefeatedEvent += OnUnitDeath;
        GlobalEvents.Scene.OnBeginSceneChange += OnSceneChange;

        m_PlayerTurnManager = GetComponent<PlayerTurnManager>();
        m_EnemyTurnManager = GetComponent<EnemyTurnManager>();
        m_PlayerUnitSetup = GetComponent<PlayerUnitSetup>();

        InputManager.Instance.PrimaryAxisInput.OnHoldEvent += OnRotateCamera;

        m_PlayerTurnManager.Initialise(OnCompleteTurn, m_MapLogic);
        m_EnemyTurnManager.Initialise(OnCompleteTurn, m_MapLogic);
        m_PlayerUnitSetup.Initialise(m_MapLogic, OnCompleteSetup);
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();
        GlobalEvents.Battle.UnitDefeatedEvent -= OnUnitDeath;
        GlobalEvents.Scene.OnBeginSceneChange -= OnSceneChange;

        if (InputManager.IsReady)
        {
            InputManager.Instance.PrimaryAxisInput.OnHoldEvent -= OnRotateCamera;
        }
    }

    /// <summary>
    /// Initialise battle with the decided upon player units and the pre-placed enemy units
    /// </summary>
    /// <param name="battleSO"></param>
    /// <param name="playerUnitData"></param>
    public void InitialiseBattle(BattleSO battleSO, List<PlayerCharacterBattleData> playerUnitData, List<InflictedToken> fatigueTokens)
    {
        m_CurrBattleSO = battleSO;
        m_TurnQueue.Clear();
        m_AllPlayerUnits.Clear();
        m_AllEnemyUnits.Clear();
        
        m_TrackedPlayerUnits.Clear();

        m_CurrMoralityPercentage = MoralityManager.Instance.CurrMoralityPercentage;
        m_PermanentFatigueTokens = fatigueTokens;

        m_BattleBGM = SoundManager.Instance.PlayWithFadeIn(battleSO.m_BattleBGM);
        StartCoroutine(BattleInitialise(battleSO, playerUnitData));
    }

    // fixing race condition
    public IEnumerator BattleInitialise(BattleSO battleSO, List<PlayerCharacterBattleData> playerUnitData)
    {
        yield return new WaitForEndOfFrame();

        m_MapLogic.ResetMap();

        foreach (EnemyUnitPlacement unitPlacement in battleSO.m_EnemyUnitsToSpawn)
        {
            InstantiateEnemyUnit(unitPlacement);
        }

        if (playerUnitData.Count > battleSO.m_PlayerStartingTiles.Count)
            Logger.Log(this.GetType().Name, "There are more player units than there are tiles to put them!", LogLevel.ERROR);

        for (int i = 0; i < playerUnitData.Count; ++i)
        {
            InstantiatePlayerUnit(playerUnitData[i], GetAvailableStartingPosition(playerUnitData[i], battleSO.m_PlayerStartingTiles));
        }

        m_TurnQueue.OrderTurnQueue();

        foreach (var objective in m_Objectives)
        {
            objective.Dispose();
        }
        m_Objectives.Clear();
        foreach (var objectiveSO in battleSO.m_Objectives)
        {
            var objective = objectiveSO.CreateInstance();
            objective.Initialize(this);
            m_Objectives.Add(objective);
        }

        isBattleInitialised = true;
        GlobalEvents.Battle.BattleInitializedEvent?.Invoke();

        yield return null;

        if (battleSO.m_SetupPhaseTutorial != null)
        {
            IUIScreen tutorialScreen = UIScreenManager.Instance.TutorialScreen;
            tutorialScreen.OnHideDone += PostTutorial;
            UIScreenManager.Instance.OpenScreen(tutorialScreen, false, battleSO.m_SetupPhaseTutorial.m_TutorialPages);
        }
        else
        {
            PostTutorial(null);
        }

        void PostTutorial(IUIScreen screen)
        {
            if (screen != null)
                screen.OnHideDone -= PostTutorial;
            m_PlayerUnitSetup.BeginSetup(battleSO.m_PlayerStartingTiles);
        }
    }

    private CoordPair GetAvailableStartingPosition(PlayerCharacterBattleData playerBattleData, List<CoordPair> startingTiles)
    {
        PlayerClassPlacement playerClassPlacement = playerBattleData.m_ClassSO.m_PlayerClassPlacement;
        for (int r = 0; r < 3; ++r)
        {
            int currRow = (int) playerClassPlacement + r;
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                CoordPair coordPair = new CoordPair(currRow, c);
                if (startingTiles.Contains(coordPair) && !m_MapLogic.IsTileOccupied(GridType.PLAYER, coordPair))
                    return coordPair;
            }
        }
        return GetFirstUnoccupiedStartingPosition(startingTiles);
    }

    private CoordPair GetFirstUnoccupiedStartingPosition(List<CoordPair> startingTiles)
    {
        foreach (CoordPair coordPair in startingTiles)
        {
            if (!m_MapLogic.IsTileOccupied(GridType.PLAYER, coordPair))
                return coordPair;
        }
        return default;
    }

    /// <summary>
    /// Instantiate the visual representation of an enemy unit, placing it on the grid
    /// and initialising the unit
    /// </summary>
    /// <param name="unitPlacement"></param>
    /// <param name="gridType"></param>
    public void InstantiateEnemyUnit(EnemyUnitPlacement unitPlacement)
    {
        EnemyUnit enemyUnit = Instantiate(m_EnemyUnit);
        // leaving it open to give enemy units permanent debuffs/buffs depending on the state of the level
        enemyUnit.Initialise(unitPlacement.m_StatAugments, unitPlacement.m_EnemyCharacterData, new());
        m_MapLogic.PlaceUnit(GridType.ENEMY, enemyUnit, unitPlacement.m_Coordinates);
        m_TurnQueue.AddUnit(enemyUnit);
        m_AllEnemyUnits.Add(enemyUnit);
        enemyUnit.m_EnemyTags = unitPlacement.m_EnemyTags;
    }

    /// <summary>
    /// Instantiate the visual representation of a player unit, placing it on the grid
    /// and initialising the unit
    /// </summary>
    /// <param name="unitPlacement"></param>
    /// <param name="gridType"></param>
    private void InstantiatePlayerUnit(PlayerCharacterBattleData unitBattleData, CoordPair position)
    {
        PlayerUnit playerUnit = Instantiate(m_PlayerUnit);
        playerUnit.Initialise(unitBattleData, m_PermanentFatigueTokens, m_CurrMoralityPercentage);
        m_MapLogic.PlaceUnit(GridType.PLAYER, playerUnit, position);
        m_TurnQueue.AddUnit(playerUnit);
        m_AllPlayerUnits.Add(playerUnit);
        if (unitBattleData.m_CannotDieWithoutLosingBattle)
            m_TrackedPlayerUnits.Add(playerUnit);
    }
    #endregion

    #region Turns
    private void StartTurn(Unit unit)
    {
        m_MapLogic.ResetMap();
        if (unit.UnitAllegiance == UnitAllegiance.PLAYER)
        {
            EvaluateEnemyDecisions();
            m_PlayerTurnManager.BeginTurn((PlayerUnit) unit);
        }
        else
        {
            m_EnemyTurnManager.PerformTurn((EnemyUnit) unit);
        }
    }

    private void EndTurn(Unit unit)
    {
        Logger.Log(this.GetType().Name, "Finish turn", LogLevel.LOG);
        
        if (!m_WithinBattle)
            return;
        
        // don't add unit back into turn queue if they're dead at the end of turn
        if (!unit.IsDead)
            m_TurnQueue.AddUnit(unit);

        m_TurnQueue.OrderTurnQueue();
        GlobalEvents.Battle.TurnOrderUpdatedEvent?.Invoke(m_TurnQueue.GetTurnOrder());
        m_BattleTick = true;

        EvaluateEnemyDecisions();
    }

    private void EvaluateEnemyDecisions()
    {
        foreach (var u in m_AllEnemyUnits)
        {
            var enemyUnit = u as EnemyUnit;
            enemyUnit.GetActionToBePerformed(m_MapLogic);
        }
    }
    #endregion

    #region BattleOutcome
    /// <summary>
    /// Check the current battle state and determine if victory has been reached
    /// </summary>
    /// <returns></returns>
    private bool CheckForVictory()
    {
        return m_Objectives.Any(x => x.CompletionStatus == ObjectiveState.Completed && x.ObjectiveTags.HasFlag(ObjectiveTag.WinOnComplete));
    }

    /// <summary>
    /// Check the current battle state and determine if the player has been defeated
    /// </summary>
    /// <param name="felledUnit"></param>
    /// <returns></returns>
    private bool CheckForDefeat(Unit felledUnit = null)
    {
        // check for tracked player unit death
        if (felledUnit != null && m_TrackedPlayerUnits.Contains(felledUnit))
            return true;

        // check for no player units remaining
        if (m_AllPlayerUnits.Count == 0)
            return true;

        return m_Objectives.Any(x => x.CompletionStatus == ObjectiveState.Failed && x.ObjectiveTags.HasFlag(ObjectiveTag.LoseOnFail));
    }

    private void OnSceneChange(SceneEnum _, SceneEnum _2)
    {
        if (m_BattleBGM.HasValue)
        {
            SoundManager.Instance.FadeOutAndStop(m_BattleBGM.Value);
            m_BattleBGM = null;
        }
    }

    private void CompleteBattle(UnitAllegiance victoriousSide)
    {
        // once a single battle end condition has been reached, don't re-invoke this method
        if (m_HasBattleConcluded)
            return;

        m_WithinBattle = false;
        m_HasBattleConcluded = true;
        Logger.Log(this.GetType().Name, $"Side that has won: {victoriousSide}", LogLevel.LOG);
        SoundManager.Instance.FadeOutAndStop(m_BattleBGM.Value);
        m_BattleBGM = null;

        if (victoriousSide == UnitAllegiance.PLAYER)
        {
            SoundManager.Instance.Play(m_VictorySFX);
        }

        GlobalEvents.Battle.BattleEndEvent?.Invoke(victoriousSide, m_TurnQueue.GetCyclesElapsed());
    }
    #endregion

    #region Callbacks
    private void OnCompleteTurn(Unit unit)
    {
        StartCoroutine(DelayTillEndTurn(unit));
    }

    private IEnumerator DelayTillEndTurn(Unit unit)
    {
        yield return new WaitForSeconds(DELAY_TILL_NEXT_TURN);
        EndTurn(unit);
    }

    private void OnUnitDeath(Unit unit)
    {
        m_TurnQueue.RemoveUnit(unit);

        if (unit.UnitAllegiance == UnitAllegiance.PLAYER)
        {
            m_AllPlayerUnits.Remove(unit);
            m_MapLogic.RemoveUnit(GridType.PLAYER, unit);

            if (CheckForDefeat(unit))
                CompleteBattle(UnitAllegiance.ENEMY);
        }
        else
        {
            m_AllEnemyUnits.Remove(unit);
            m_MapLogic.RemoveUnit(GridType.ENEMY, unit);

            if (CheckForVictory())
            {
                CompleteBattle(UnitAllegiance.PLAYER);
            }
        }
    }

    private void OnCompleteSetup()
    {
        Logger.Log(this.GetType().Name, "Begin battle", LogLevel.LOG);
        if (m_CurrBattleSO.m_BattlePhaseTutorial != null)
        {
            IUIScreen tutorialScreen = UIScreenManager.Instance.TutorialScreen;
            tutorialScreen.OnHideDone += PostTutorial;
            UIScreenManager.Instance.OpenScreen(tutorialScreen, false, m_CurrBattleSO.m_BattlePhaseTutorial.m_TutorialPages);
        }
        else
        {
            PostTutorial(null);
        }

        void PostTutorial(IUIScreen screen)
        {
            if (screen != null)
                screen.OnHideDone -= PostTutorial;
            StartCoroutine(StartBattle());
        }
    }

    private IEnumerator StartBattle()
    {
        yield return new WaitForEndOfFrame();
        GlobalEvents.Battle.TurnOrderUpdatedEvent?.Invoke(m_TurnQueue.GetTurnOrder());
        m_WithinBattle = true;
        m_BattleTick = true;
    }

    private void OnRotateCamera(IInput input)
    {
        var hAxis = input.GetValue<float>();
        m_CameraLookAtPoint.Rotate(Vector3.up, -hAxis * CAMERA_ROTATION_SPEED * Time.deltaTime);
    }
    #endregion

    #region Tick Queue
    private void Update()
    {
        if (!m_BattleTick || !m_WithinBattle || !isBattleInitialised)
            return;

        if (m_TurnQueue.TryGetReadyUnit(out Unit readyUnit))
        {
            m_BattleTick = false;

            if (CheckForVictory())
            {
                CompleteBattle(UnitAllegiance.PLAYER);
                return;
            }

            if (CheckForDefeat())
            {
                CompleteBattle(UnitAllegiance.ENEMY);
                return;
            }

            StartTurn(readyUnit);
            return;
        }

        m_TurnQueue.Tick();
        // Logger.Log(this.GetType().Name, m_TurnQueue.ToString(), LogLevel.LOG);
    }
    #endregion
}
