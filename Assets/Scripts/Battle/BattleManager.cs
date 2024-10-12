using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Input;
using Game;

public enum WinCondition
{
    DEFEAT_REQUIRED,
    SURVIVE_TURNS
}

// additional lose conditions
public enum SecondaryLoseCondition
{
    TOO_MANY_TURNS
}

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
    private TurnQueue m_TurnQueue = new TurnQueue();
    // TODO: This isn't actually required but given the data is still in flux right now
    // I'm leaving this here
    private HashSet<Unit> m_AllPlayerUnits = new HashSet<Unit>();
    private HashSet<Unit> m_AllEnemyUnits = new HashSet<Unit>();

    private const float DELAY_TILL_NEXT_TURN = 0.3f;
    #endregion

    #region State
    private bool m_BattleTick = false;
    private bool m_WithinBattle = false;
    private bool m_HasBattleConcluded = false;
    #endregion

    #region Win Condition
    private WinCondition m_WinCondition;
    private float m_TurnsToSurvive;
    private HashSet<Unit> m_TrackedEnemyUnits = new HashSet<Unit>();
    #endregion

    #region Lose Condition
    private HashSet<Unit> m_TrackedPlayerUnits = new HashSet<Unit>();
    private SecondaryLoseCondition[] m_SecondaryLoseCondition;
    private float m_MaxTurns;
    #endregion

    #region Camera
    private Camera m_BattleCamera;
    private const float CAMERA_ROTATION_SPEED = 50f;
    #endregion

    #region Initialisation
    private bool isBattleInitialised = false;
    
    private void Start()
    {
        m_PlayerTurnManager = GetComponent<PlayerTurnManager>();
        m_EnemyTurnManager = GetComponent<EnemyTurnManager>();
        m_PlayerUnitSetup = GetComponent<PlayerUnitSetup>();

        m_BattleCamera = CameraManager.Instance.MainCamera;
        m_BattleCamera.transform.LookAt(m_CameraLookAtPoint);
        InputManager.Instance.PrimaryAxisInput.OnHoldEvent += OnRotateCamera;

        m_PlayerTurnManager.Initialise(OnCompleteTurn, m_MapLogic);
        m_EnemyTurnManager.Initialise(OnCompleteTurn, m_MapLogic);
        m_PlayerUnitSetup.Initialise(m_MapLogic, OnCompleteSetup);

        GlobalEvents.Scene.BattleSceneLoadedEvent?.Invoke();
    }

    protected override void HandleAwake()
    {
        base.HandleAwake();
        GlobalEvents.Battle.UnitDefeatedEvent += OnUnitDeath;
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();
        GlobalEvents.Battle.UnitDefeatedEvent -= OnUnitDeath;
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
    public void InitialiseBattle(BattleSO battleSO, List<PlayerCharacterBattleData> playerUnitData, GameObject mapBiome)
    {
        m_TurnQueue.Clear();
        m_AllPlayerUnits.Clear();
        m_AllEnemyUnits.Clear();
        m_TrackedEnemyUnits.Clear();
        m_TrackedPlayerUnits.Clear();

        m_WinCondition = battleSO.m_WinCondition;
        m_SecondaryLoseCondition = battleSO.m_AdditionalLoseConditions;
        m_TurnsToSurvive = battleSO.m_TurnsToSurvive;
        m_MaxTurns = battleSO.m_MaxTurns;

        InstantiateBiome(mapBiome);
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
            InstantiatePlayerUnit(playerUnitData[i], battleSO.m_PlayerStartingTiles[i]);
        }

        m_TurnQueue.OrderTurnQueue();
        m_PlayerUnitSetup.BeginSetup(battleSO.m_PlayerStartingTiles);
        
        isBattleInitialised = true;
    }

    private void InstantiateBiome(GameObject biomeObj)
    {
        GameObject map = Instantiate(biomeObj, m_MapBiomeParent);
        map.transform.localPosition = Vector3.zero;
        map.transform.localRotation = Quaternion.identity;
        map.transform.localScale = Vector3.one;
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
        enemyUnit.Initialise(unitPlacement.m_StatAugments, unitPlacement.m_EnemyCharacterData);
        m_MapLogic.PlaceUnit(GridType.ENEMY, enemyUnit, unitPlacement.m_Coordinates);
        m_TurnQueue.AddUnit(enemyUnit);
        m_AllEnemyUnits.Add(enemyUnit);
        if (unitPlacement.m_DefeatRequired)
            m_TrackedEnemyUnits.Add(enemyUnit);
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
        playerUnit.Initialise(unitBattleData);
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
        return m_WinCondition switch
        {
            WinCondition.DEFEAT_REQUIRED => m_TrackedEnemyUnits.Count == 0,
            WinCondition.SURVIVE_TURNS => m_TurnQueue.GetCyclesElapsed() >= m_TurnsToSurvive,
            _ => false
        };
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

        foreach (SecondaryLoseCondition loseCondition in m_SecondaryLoseCondition)
        {
            switch (loseCondition)
            {
                case SecondaryLoseCondition.TOO_MANY_TURNS:
                    if (m_TurnQueue.GetCyclesElapsed() >= m_MaxTurns)
                        return true;
                    break;
            }
        }
        return false;
    }

    private void CompleteBattle(UnitAllegiance victoriousSide)
    {
        // once a single battle end condition has been reached, don't re-invoke this method
        if (m_HasBattleConcluded)
            return;

        m_WithinBattle = false;
        m_HasBattleConcluded = true;
        Logger.Log(this.GetType().Name, $"Side that has won: {victoriousSide}", LogLevel.LOG);
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
        // TODO: move this somewhere else
        unit.Die();

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
            if (m_TrackedEnemyUnits.Contains(unit))
                m_TrackedEnemyUnits.Remove(unit);
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
        StartCoroutine(StartBattle());
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
        m_BattleCamera.transform.RotateAround(m_CameraLookAtPoint.position, new Vector3(0f, 1f, 0f), -hAxis * CAMERA_ROTATION_SPEED * Time.deltaTime);
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
        Logger.Log(this.GetType().Name, m_TurnQueue.ToString(), LogLevel.LOG);
    }
    #endregion
}
