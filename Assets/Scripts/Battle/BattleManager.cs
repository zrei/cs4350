using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleTurn
{
    PLAYER,
    ENEMY
}

// may or may not become a singleton
[RequireComponent(typeof(PlayerTurnManager))]
[RequireComponent(typeof(EnemyTurnManager))]
public class BattleManager : MonoBehaviour
{
    #region Test
    [SerializeField] private BattleSO m_TestBattle;
    [SerializeField] private List<UnitPlacement> m_TestPlacement;
    #endregion

    [SerializeField] private MapLogic m_MapLogic;

    // the current phase based on the turn
    private BattleTurn m_CurrPhase = BattleTurn.PLAYER;
    public BattleTurn CurrPhase => m_CurrPhase;

    // turn managers for the player and enemy
    private PlayerTurnManager m_PlayerTurnManager;
    private EnemyTurnManager m_EnemyTurnManager;

    // units
    private HashSet<Unit> m_Units;
    // unit queue
    private List<Unit> m_UnitTurns;

    #region Initialisation
    private void Start()
    {
        m_PlayerTurnManager = GetComponent<PlayerTurnManager>();
        m_EnemyTurnManager = GetComponent<EnemyTurnManager>();
        m_PlayerTurnManager.Initialise(OnCompleteTurn, m_MapLogic);
        m_EnemyTurnManager.Initialise(OnCompleteTurn, m_MapLogic);
        m_UnitTurns = new List<Unit>();
        m_Units = new HashSet<Unit>();

        InitialiseBattle(m_TestBattle, m_TestPlacement);
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
        StartTurn();
    }

    /// <summary>
    /// Initialise battle with the decided upon player units and the pre-placed enemy units
    /// </summary>
    /// <param name="battleSO"></param>
    /// <param name="playerUnits"></param>
    public void InitialiseBattle(BattleSO battleSO, List<UnitPlacement> playerUnits)
    {
        m_UnitTurns.Clear();
        m_Units.Clear();
        foreach (UnitPlacement unitPlacement in battleSO.m_EnemyUnitsToSpawn)
        {
            InstantiateUnit(unitPlacement, GridType.ENEMY);
        }

        foreach (UnitPlacement unitPlacement in playerUnits)
        {
            InstantiateUnit(unitPlacement, GridType.PLAYER);
        }

        m_UnitTurns.Sort(UnitSpeedComparer);
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
        m_MapLogic.PlaceUnit(gridType, unit, unitPlacement.m_Coodinates);
        m_UnitTurns.Add(unit);
        m_Units.Add(unit);
        unit.Initialise(unitPlacement.m_Stats);
    }
    #endregion

    #region Turns
    private void StartTurn()
    {
        m_MapLogic.ResetMap();
        Unit unit = m_UnitTurns[0];
        m_UnitTurns.RemoveAt(0);
        if (unit.UnitAllegiance == UnitAllegiance.PLAYER)
        {
            m_PlayerTurnManager.BeginTurn((PlayerUnit) unit);
        }
        else
        {
            m_EnemyTurnManager.PerformTurn((EnemyUnit) unit);
        }
    }

    private void EndTurn()
    {
        if (m_UnitTurns.Count == 0)
        {
            FillTurns();
        }
        StartTurn();
    }

    private void FillTurns()
    {
        foreach (Unit unit in m_Units)
        {
            m_UnitTurns.Add(unit);
        }
        m_UnitTurns.Sort(UnitSpeedComparer);
    }
    #endregion

    #region Callbacks
    private void OnCompleteTurn()
    {
        Logger.Log(this.GetType().Name, "Finish turn", LogLevel.LOG);
        EndTurn();
    }

    private void OnUnitDeath(Unit unit)
    {
        m_Units.Remove(unit);
        if (m_UnitTurns.Contains(unit))
        {
            m_UnitTurns.Remove(unit);
        }
    }
    #endregion

    #region Helper
    private int UnitSpeedComparer(Unit unit1, Unit unit2)
    {
        return unit2.Stat.m_Speed.CompareTo(unit1.Stat.m_Speed);//unit1.Stat.m_Speed.CompareTo(unit2.Stat.m_Speed);
    }
    #endregion
}