using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum BattleTurn
{
    PLAYER,
    ENEMY
}

public class TurnCalc
{
    private const float DISTANCE_THRESHOLD = 50f;
    private const float TICK_AMOUNT = 1f;

    private class TurnWrapper
    {
        public float m_TimeRemaining;
        public Unit m_Unit;

        public TurnWrapper(float timeRemaining, Unit unit)
        {
            m_TimeRemaining = timeRemaining;
            m_Unit = unit;
        }

        public override string ToString()
        {
            return $"Unit {m_Unit.name} with time {m_TimeRemaining} remaining to act";
        }
    }

    private List<TurnWrapper> m_Turns = new List<TurnWrapper>();

    public void Tick()
    {
        if (m_Turns.Count <= 0)
            return;

        float tick = Mathf.Min(TICK_AMOUNT, m_Turns[0].m_TimeRemaining);
        foreach (TurnWrapper turnWrapper in m_Turns)
        {
            turnWrapper.m_TimeRemaining -= tick;
        }
    }

    public bool TryGetReadyUnit(out Unit readyUnit)
    {
        if (m_Turns.Count <= 0)
        {
            readyUnit = null;
            return false;
        }

        if (m_Turns[0].m_TimeRemaining == 0)
        {
            readyUnit = m_Turns[0].m_Unit;
            m_Turns.RemoveAt(0);
            return true;
        }
        else
        {
            readyUnit = null;
            return false;
        }
    }

    public void RemoveUnitFromTurns(Unit unit)
    {
        int idx = -1;
        for (int i = 0; i < m_Turns.Count; ++i)
        {
            if (m_Turns[i].m_Unit == unit)
            {
                idx = i;
                break;
            }
        }

        if (idx >= 0)
            m_Turns.RemoveAt(idx);
    }

    public void AddUnit(Unit unit)
    {
        m_Turns.Add(new TurnWrapper(DISTANCE_THRESHOLD / unit.Stat.m_Speed, unit));
    }

    public void OrderTurns()
    {
        m_Turns.Sort(UnitSpeedComparer);
    }

    public void ClearTurns()
    {
        m_Turns.Clear();
    }

    // TODO: Decide on timebreaker for units with the same time remaining
    private int UnitSpeedComparer(TurnWrapper unit1, TurnWrapper unit2)
    {
        return unit1.m_TimeRemaining.CompareTo(unit2.m_TimeRemaining); //unit1.Stat.m_Speed.CompareTo(unit2.Stat.m_Speed);
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder("Current state of the turn order:\n");
        foreach (TurnWrapper turnWrapper in m_Turns)
        {
            stringBuilder.Append(turnWrapper + "\n");
        }
        return stringBuilder.ToString();
    }
}

// may or may not become a singleton
[RequireComponent(typeof(PlayerTurnManager))]
[RequireComponent(typeof(EnemyTurnManager))]
public class BattleManager : MonoBehaviour
{
    #region Test
    [SerializeField] private BattleSO m_TestBattle;
    [SerializeField] private List<Unit> m_TestPlacement;
    [SerializeField] private List<Stats> m_TestStats;
    #endregion

    [SerializeField] private MapLogic m_MapLogic;

    // the current phase based on the turn
    private BattleTurn m_CurrPhase = BattleTurn.PLAYER;
    public BattleTurn CurrPhase => m_CurrPhase;

    // turn managers for the player and enemy
    private PlayerTurnManager m_PlayerTurnManager;
    private EnemyTurnManager m_EnemyTurnManager;

    // unit queue
    private TurnCalc m_TurnCalc = new TurnCalc();
    private bool m_BattleTick = false;

    #region Initialisation
    private void Start()
    {
        m_PlayerTurnManager = GetComponent<PlayerTurnManager>();
        m_EnemyTurnManager = GetComponent<EnemyTurnManager>();
        m_PlayerTurnManager.Initialise(OnCompleteTurn, m_MapLogic);
        m_EnemyTurnManager.Initialise(OnCompleteTurn, m_MapLogic);

        InitialiseBattle(m_TestBattle, m_TestPlacement, m_TestStats);
        StartCoroutine(TestStart());
    }

    private void Awake()
    {
        GlobalEvents.Battle.UnitDefeatedEvent += OnUnitDeath;
    }

    private void OnDestroy()
    {
        GlobalEvents.Battle.UnitDefeatedEvent -= OnUnitDeath;
    }

    private IEnumerator TestStart()
    {
        yield return new WaitForEndOfFrame();
        m_BattleTick = true;
    }

    /// <summary>
    /// Initialise battle with the decided upon player units and the pre-placed enemy units
    /// </summary>
    /// <param name="battleSO"></param>
    /// <param name="playerUnits"></param>
    public void InitialiseBattle(BattleSO battleSO, List<Unit> playerUnits, List<Stats> playerStats)
    {
        m_TurnCalc.ClearTurns();

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

        m_TurnCalc.OrderTurns();
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
        unit.Initialise(unitPlacement.m_Stats);
        m_MapLogic.PlaceUnit(gridType, unit, unitPlacement.m_Coodinates);
        m_TurnCalc.AddUnit(unit);
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
    #endregion

    #region Callbacks
    private void OnCompleteTurn(Unit unit)
    {
        Logger.Log(this.GetType().Name, "Finish turn", LogLevel.LOG);
        
        m_TurnCalc.AddUnit(unit);
        m_TurnCalc.OrderTurns();
        m_BattleTick = true;
    }

    private void OnUnitDeath(Unit unit)
    {
        m_TurnCalc.RemoveUnitFromTurns(unit);
    }
    #endregion

    #region Helper
    
    #endregion

    private void Update()
    {
        if (!m_BattleTick)
            return;

        if (m_TurnCalc.TryGetReadyUnit(out Unit readyUnit))
        {
            m_BattleTick = false;
            StartTurn(readyUnit);
            return;
        }

        m_TurnCalc.Tick();
        Logger.Log(this.GetType().Name, m_TurnCalc.ToString(), LogLevel.LOG);
    }
}