using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Input;

public enum PlayerTurnState
{
    SELECTING_MOVEMENT_SQUARE,
    SELECTING_ATTACK_TARGET
}

public class PlayerTurnManager : TurnManager
{
    #region Test
    [SerializeField] AttackSO m_TestAttackSO;
    #endregion

    #region Current State
    /// <summary>
    /// Currently controlled player unit
    /// </summary>
    private PlayerUnit m_CurrUnit;
    private PlayerTurnState m_CurrState = PlayerTurnState.SELECTING_MOVEMENT_SQUARE;
    /// <summary>
    /// Whether it's currently the player's turn
    /// </summary>
    private bool m_WithinTurn = false;
    #endregion

    #region Input and Selected Tile
    private CoordPair m_CurrTargetTile;
    private bool m_HasHitGrid;
    private GridType m_CurrTileSide;
    #endregion

    /// <summary>
    /// Maps coordinates to nodes that track paths that end at those coordinates
    /// Used to store the moveable points which is calculated at the start of the turn
    /// </summary>
    private Dictionary<CoordPair, PathNode> m_TileToPath;

    /// <summary>
    /// All reachable path nodes
    /// </summary>
    private HashSet<PathNode> m_ReachablePoints;

    /*
    /// <summary>
    /// Remaining actions that can still be taken by the player
    /// </summary>
    private HashSet<PlayerTurnState> m_RemainingActions;
    */

    #region Initialisation
    private void Start()
    {
        m_TileToPath = new Dictionary<CoordPair, PathNode>();
        //m_RemainingActions = new HashSet<PlayerTurnState>();
    }
    #endregion

    #region Start Turn
    /// <summary>
    /// Begin the turn involving the given player unit. Pre-calculates moveable squares.
    /// </summary>
    /// <param name="playerUnit"></param>
    public void BeginTurn(PlayerUnit playerUnit)
    {
        Logger.Log(this.GetType().Name, "Start player turn with: " + playerUnit.name, LogLevel.LOG);
        GlobalEvents.Battle.PlayerTurnStartEvent?.Invoke();

        m_CurrUnit = playerUnit;
        m_CurrUnit.Tick();

        if (m_CurrUnit.IsDead)
        {
            GlobalEvents.Battle.UnitDefeatedEvent?.Invoke(m_CurrUnit);
            return;
        }

        Logger.Log(this.GetType().Name, "Tile unit is on: " + m_CurrUnit.CurrPosition, LogLevel.LOG);

        m_ReachablePoints = m_MapLogic.CalculateReachablePoints(GridType.PLAYER, m_CurrUnit);

        m_TileToPath.Clear();
        foreach (PathNode pathNode in m_ReachablePoints)
        {
            m_TileToPath.Add(pathNode.m_Coordinates, pathNode);
        }

        /*
        m_RemainingActions.Clear();
        foreach (PlayerTurnState action in Enum.GetValues(typeof(PlayerTurnState)))
        {
            m_RemainingActions.Add(action);
        }
        */

        m_WithinTurn = true;

        TransitToAction(PlayerTurnState.SELECTING_MOVEMENT_SQUARE);

        InputManager.Instance.EndTurn.OnPressEvent += OnEndTurn;
        InputManager.Instance.SwitchAction.OnPressEvent += OnSwitchAction;
        InputManager.Instance.PointerPosition.OnChangeEvent += OnPointerPosition;
        InputManager.Instance.PointerSelect.OnPressEvent += OnPointerSelect;
    }
    #endregion

    #region Inputs
    private void OnEndTurn(IInput input)
    {
        EndTurn();
    }

    private void OnSwitchAction(IInput input)
    {
        SwitchAction();
    }

    private void OnPointerPosition(IInput input)
    {
        var inputVector = input.GetValue<Vector2>();
        Vector3 mousePos = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane);
        m_HasHitGrid = m_MapLogic.TryRetrieveTile(Camera.main.ScreenPointToRay(mousePos), out m_CurrTargetTile, out m_CurrTileSide);

        UpdateState();
    }

    private void OnPointerSelect(IInput input)
    {
        if (TryPerformAction())
        {
            m_WithinTurn = false;
            return;
        }
    }
    #endregion

    #region Update State
    private void UpdateState()
    {
        switch (m_CurrState)
        {
            case PlayerTurnState.SELECTING_ATTACK_TARGET:
                UpdateAttackState();
                break;
            case PlayerTurnState.SELECTING_MOVEMENT_SQUARE:
                UpdateMoveState();
                break;
        }
    }

    private void UpdateMoveState()
    {
        if (m_HasHitGrid && m_CurrTileSide == GridType.PLAYER && m_TileToPath.ContainsKey(m_CurrTargetTile))
        {
            m_MapLogic.ColorPath(GridType.PLAYER, m_TileToPath[m_CurrTargetTile]);
        }
        else
        {
            m_MapLogic.ResetPath();
        }            
    }

    private void UpdateAttackState()
    {
        if (m_HasHitGrid && m_CurrTileSide == GridType.ENEMY && m_TestAttackSO.IsValidTargetTile(m_CurrTargetTile, m_CurrUnit))
        {
            m_MapLogic.SetTarget(GridType.ENEMY, m_TestAttackSO, m_CurrTargetTile);
        }
        else
        {
            m_MapLogic.ResetTarget();
        }
    }
    #endregion

    #region Perform Action
    private bool TryPerformAction()
    {
        return m_CurrState switch
        {
            PlayerTurnState.SELECTING_ATTACK_TARGET => TryPerformAttack(),
            PlayerTurnState.SELECTING_MOVEMENT_SQUARE => TryPerformMove(),
            _ => false
        };
    }

    private bool TryPerformMove()
    {
        if (m_HasHitGrid && m_CurrTileSide == GridType.PLAYER && m_TileToPath.ContainsKey(m_CurrTargetTile))
        {
            Logger.Log(this.GetType().Name, "Begin moving from " + m_CurrUnit.CurrPosition + " to " + m_CurrTargetTile, LogLevel.LOG);
            m_MapLogic.MoveUnit(GridType.PLAYER, m_CurrUnit, m_TileToPath[m_CurrTargetTile], EndTurn);
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool TryPerformAttack()
    {
        if (m_HasHitGrid && m_CurrTileSide == GridType.ENEMY && m_TestAttackSO.IsValidTargetTile(m_CurrTargetTile, m_CurrUnit))
        {
            m_MapLogic.Attack(GridType.ENEMY, m_CurrUnit, m_TestAttackSO, m_CurrTargetTile);
            Logger.Log(this.GetType().Name, "Attack!", LogLevel.LOG);
            EndTurn();
            return true;
        }
        else
        {
            return false;
        }
        
    }
    #endregion

    #region Switch Action
    /*
    private void TransitionAction(PlayerTurnState currAction)
    {
        m_BlockInputs = false;
        m_RemainingActions.Remove(currAction);

        if (m_RemainingActions.Count > 0)
        {
            m_RemainingActions.TryGetValue(PlayerTurnState.SELECTING_ATTACK_TARGET, out m_CurrState);
            m_MapLogic.ResetMap();
            TransitToAction(m_CurrState);
        }
        else
        {
            EndTurn();
        }
    }
    */
    private void SwitchAction()
    {
        PlayerTurnState nextState = (PlayerTurnState) (((int) m_CurrState + 1) % Enum.GetValues(typeof(PlayerTurnState)).Length);
        TransitToAction(nextState);
    }

    private void TransitToAction(PlayerTurnState currAction)
    {
        m_MapLogic.ResetMap();
        m_CurrState = currAction;

        switch (currAction)
        {
            case PlayerTurnState.SELECTING_MOVEMENT_SQUARE:
                m_MapLogic.ColorMap(GridType.PLAYER, m_ReachablePoints);
                break;
        }

        Logger.Log(this.GetType().Name, "Current phase: " + m_CurrState.ToString(), LogLevel.LOG);
        GlobalEvents.Battle.PlayerPhaseUpdateEvent?.Invoke(m_CurrState);
    }
    #endregion

    #region End Turn
    private void EndTurn()
    {
        m_WithinTurn = false;
        m_CompleteTurnEvent?.Invoke(m_CurrUnit);

        InputManager.Instance.EndTurn.OnPressEvent -= OnEndTurn;
        InputManager.Instance.SwitchAction.OnPressEvent -= OnSwitchAction;
        InputManager.Instance.PointerPosition.OnChangeEvent -= OnPointerPosition;
        InputManager.Instance.PointerSelect.OnPressEvent -= OnPointerSelect;
    }
    #endregion
}