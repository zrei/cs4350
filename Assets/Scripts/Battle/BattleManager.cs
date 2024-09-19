using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Input;
using Game;

// may or may not become a singleton
[RequireComponent(typeof(PlayerTurnManager))]
[RequireComponent(typeof(EnemyTurnManager))]
[RequireComponent(typeof(PlayerUnitSetup))]
public class BattleManager : MonoBehaviour
{
    #region Test
    [SerializeField] private BattleSO m_TestBattle;
    [SerializeField] private List<Unit> m_TestPlacement;
    [SerializeField] private List<Stats> m_TestStats;

    private IEnumerator TestStart()
    {
        yield return new WaitForEndOfFrame();
        GlobalEvents.Battle.TurnOrderUpdatedEvent?.Invoke(m_TurnQueue.GetTurnOrder());
        m_WithinBattle = true;
        m_BattleTick = true;
    }
    #endregion

    [Header("References")]
    [SerializeField] private MapLogic m_MapLogic;
    [SerializeField] private Transform m_CameraLookAtPoint;

    // for initial battlefield setup
    private PlayerUnitSetup m_PlayerUnitSetup;

    // turn managers for the player and enemy
    private PlayerTurnManager m_PlayerTurnManager;
    private EnemyTurnManager m_EnemyTurnManager;

    // turn queue
    private TurnQueue m_TurnQueue = new TurnQueue();
    private HashSet<Unit> m_EnemyUnits = new HashSet<Unit>();
    private HashSet<Unit> m_PlayerUnits = new HashSet<Unit>();

    private bool m_BattleTick = false;
    private bool m_WithinBattle = false;

    // camera
    private Camera m_BattleCamera;
    private const float CAMERA_ROTATION_SPEED = 50f;

    #region Initialisation
    private void Start()
    {
        m_PlayerTurnManager = GetComponent<PlayerTurnManager>();
        m_EnemyTurnManager = GetComponent<EnemyTurnManager>();
        m_PlayerUnitSetup = GetComponent<PlayerUnitSetup>();

        // TODO: Handle this separately if need be
        m_BattleCamera = CameraManager.Instance.MainCamera;
        m_BattleCamera.transform.LookAt(m_CameraLookAtPoint);
        InputManager.Instance.PrimaryAxisInput.OnHoldEvent += OnRotateCamera;

        m_PlayerTurnManager.Initialise(OnCompleteTurn, m_MapLogic);
        m_EnemyTurnManager.Initialise(OnCompleteTurn, m_MapLogic);
        m_PlayerUnitSetup.Initialise(m_MapLogic, OnCompleteSetup);

        // TODO: This is test code
        InitialiseBattle(m_TestBattle, m_TestPlacement, m_TestStats);
    }

    private void Awake()
    {
        GlobalEvents.Battle.UnitDefeatedEvent += OnUnitDeath;
    }

    private void OnDestroy()
    {
        GlobalEvents.Battle.UnitDefeatedEvent -= OnUnitDeath;
    }

    /// <summary>
    /// Initialise battle with the decided upon player units and the pre-placed enemy units
    /// </summary>
    /// <param name="battleSO"></param>
    /// <param name="playerUnits"></param>
    // TODO: Bundle the players in a better way OR intialise them in the level FIRST
    public void InitialiseBattle(BattleSO battleSO, List<Unit> playerUnits, List<Stats> playerStats)
    {
        m_TurnQueue.Clear();
        m_EnemyUnits.Clear();
        m_PlayerUnits.Clear();

        m_MapLogic.ResetMap();

        foreach (UnitPlacement unitPlacement in battleSO.m_EnemyUnitsToSpawn)
        {
            InstantiateUnit(unitPlacement, GridType.ENEMY);
        }

        if (playerUnits.Count > battleSO.m_PlayerStartingTiles.Count)
            Logger.Log(this.GetType().Name, "There are more player units than there are tiles to put them!", LogLevel.ERROR);

        for (int i = 0; i < playerUnits.Count; ++i)
        {
            InstantiateUnit(new UnitPlacement {m_Coodinates = battleSO.m_PlayerStartingTiles[i], m_Unit = playerUnits[i], m_Stats = playerStats[i]}, GridType.PLAYER);
        }

        m_TurnQueue.OrderTurnQueue();
        m_PlayerUnitSetup.BeginSetup(battleSO.m_PlayerStartingTiles);
    }

    /// <summary>
    /// Instantiate the visual representation of the unit, placing it on the grid
    /// and initialising the unit
    /// </summary>
    /// <param name="unitPlacement"></param>
    /// <param name="gridType"></param>
    private void InstantiateUnit(UnitPlacement unitPlacement, GridType gridType)
    {
        Unit unit = Instantiate(unitPlacement.m_Unit);
        unit.Initialise(unitPlacement.m_Stats, unitPlacement.m_Class);
        m_MapLogic.PlaceUnit(gridType, unit, unitPlacement.m_Coodinates);
        m_TurnQueue.AddUnit(unit);
        if (unit.UnitAllegiance == UnitAllegiance.PLAYER)
            m_PlayerUnits.Add(unit);
        else
            m_EnemyUnits.Add(unit);
    }
    #endregion

    #region Turns
    private void StartTurn(Unit unit)
    {
        m_MapLogic.ResetMap();
        if (unit.UnitAllegiance == UnitAllegiance.PLAYER)
        {
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
        
        m_TurnQueue.AddUnit(unit);
        m_TurnQueue.OrderTurnQueue();
        GlobalEvents.Battle.TurnOrderUpdatedEvent?.Invoke(m_TurnQueue.GetTurnOrder());
        m_BattleTick = true;
    }
    #endregion

    #region BattleOutcome
    private void CompleteBattle(UnitAllegiance victoriousSide)
    {
        m_WithinBattle = false;
        Logger.Log(this.GetType().Name, $"Side that has won: {victoriousSide}", LogLevel.LOG);
        GlobalEvents.Battle.BattleEndEvent?.Invoke(victoriousSide);
    }
    #endregion

    #region Callbacks
    private void OnCompleteTurn(Unit unit)
    {
        EndTurn(unit);
    }

    // this might be responsible for actually destroying the g9ame object/ returning it to pool or whatever
    // to be consistent
    private void OnUnitDeath(Unit unit)
    {
        m_TurnQueue.RemoveUnit(unit);
        // TODO: move this somewhere else
        unit.Die();
        // Destroy(unit.gameObject);

        if (unit.UnitAllegiance == UnitAllegiance.PLAYER)
        {
            m_PlayerUnits.Remove(unit);
            if (m_PlayerUnits.Count <= 0)
            {
                CompleteBattle(UnitAllegiance.ENEMY);
            }
        }
        else
        {
            m_EnemyUnits.Remove(unit);
            if (m_EnemyUnits.Count <= 0)
            {
                CompleteBattle(UnitAllegiance.PLAYER);
            }
        }
    }

    private void OnCompleteSetup()
    {
        Logger.Log(this.GetType().Name, "Begin battle", LogLevel.LOG);
        StartCoroutine(TestStart());
    }

    private void OnRotateCamera(IInput input)
    {
        var inputVector = input.GetValue<Vector2>();
        var hAxis = inputVector.x;
        m_BattleCamera.transform.RotateAround(m_CameraLookAtPoint.position, new Vector3(0f, 1f, 0f), -hAxis * CAMERA_ROTATION_SPEED * Time.deltaTime);
    }
    #endregion

    #region Tick Queue
    private void Update()
    {
        if (!m_BattleTick || !m_WithinBattle)
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