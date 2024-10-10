using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Input;
using Game;

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
    private HashSet<Unit> m_EnemyUnits = new HashSet<Unit>();
    private HashSet<Unit> m_PlayerUnits = new HashSet<Unit>();

    private const float DELAY_TILL_NEXT_TURN = 0.3f;
    #endregion

    #region State
    private bool m_BattleTick = false;
    private bool m_WithinBattle = false;
    private bool m_HasBattleConcluded = false;
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
        m_EnemyUnits.Clear();
        m_PlayerUnits.Clear();

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
        m_EnemyUnits.Add(enemyUnit);
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
        m_PlayerUnits.Add(playerUnit);
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
        foreach (var u in m_EnemyUnits)
        {
            var enemyUnit = u as EnemyUnit;
            enemyUnit.GetActionToBePerformed(m_MapLogic);
        }
    }
    #endregion

    #region BattleOutcome
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
            m_PlayerUnits.Remove(unit);
            m_MapLogic.RemoveUnit(GridType.PLAYER, unit);

            if (m_PlayerUnits.Count <= 0)
            {
                CompleteBattle(UnitAllegiance.ENEMY);
            }
        }
        else
        {
            m_EnemyUnits.Remove(unit);
            m_MapLogic.RemoveUnit(GridType.ENEMY, unit);

            if (m_EnemyUnits.Count <= 0)
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
            StartTurn(readyUnit);
            return;
        }

        m_TurnQueue.Tick();
        Logger.Log(this.GetType().Name, m_TurnQueue.ToString(), LogLevel.LOG);
    }
    #endregion
}