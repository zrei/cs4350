using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerTurnState
{
    SELECTING_MOVEMENT_SQUARE,
    SELECTING_ATTACK_TARGET
}

public class PlayerTurnManager : TurnManager
{
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
    /// <summary>
    /// Block any input
    /// </summary>
    private bool m_BlockInputs = false;
    #endregion

    // handle units
    private CoordPair m_CurrTargetTile;
    private List<CoordPair> m_AttackPoints = null;

    /// <summary>
    /// Maps coordinates to nodes that track paths that end at those coordinates
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
        
        m_CurrUnit = playerUnit;
        Logger.Log(this.GetType().Name, "Tile unit is on: " + m_CurrUnit.CurrPosition, LogLevel.LOG);
    
        m_ReachablePoints = m_MapLogic.CalculateReachablePoints(GridType.PLAYER, m_CurrUnit);

        m_TileToPath.Clear();
        foreach (PathNode pathNode in m_ReachablePoints)
        {
            m_TileToPath.Add(pathNode.m_Coordinates, pathNode);
        }

        // m_RemainingActions.Clear();

        /*
        foreach (PlayerTurnState action in Enum.GetValues(typeof(PlayerTurnState)))
        {
            m_RemainingActions.Add(action);
        }
        */

        m_WithinTurn = true;
        m_BlockInputs = false;
        m_CurrState = PlayerTurnState.SELECTING_MOVEMENT_SQUARE;
        m_MapLogic.ColorMap(GridType.PLAYER, m_ReachablePoints);
        Logger.Log(this.GetType().Name, "Current phase: " + m_CurrState.ToString(), LogLevel.LOG);
    }
    #endregion

    #region Inputs
    private void Update()
    {
        if (!m_WithinTurn || m_BlockInputs)
            return;

        //Debug.Log("WHEEE!");

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            EndTurn();
        }

        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
        bool hitGrid = m_MapLogic.TryRetrieveTile(Camera.main.ScreenPointToRay(mousePos), out m_CurrTargetTile, out GridType gridType);

        switch (m_CurrState)
        {
            case PlayerTurnState.SELECTING_MOVEMENT_SQUARE:
                if (hitGrid && gridType == GridType.PLAYER && m_TileToPath.ContainsKey(m_CurrTargetTile))
                {
                    m_MapLogic.ColorPath(GridType.PLAYER, m_TileToPath[m_CurrTargetTile]);
                }
                else
                {
                    m_MapLogic.ResetPath();
                }
                break;
            case PlayerTurnState.SELECTING_ATTACK_TARGET:
                // must hit last row: row 4
                if (hitGrid && gridType == GridType.ENEMY && m_CurrTargetTile.m_Row == 4 && m_MapLogic.IsTileOccupied(GridType.ENEMY, m_CurrTargetTile))
                {
                    m_AttackPoints = new() {m_CurrTargetTile, m_CurrTargetTile.MoveUp()};
                    m_MapLogic.SetTarget(GridType.ENEMY, m_AttackPoints);
                }
                else
                {
                    m_MapLogic.ResetTarget();
                    m_AttackPoints = null;
                }
                break;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (m_CurrState == PlayerTurnState.SELECTING_MOVEMENT_SQUARE && m_TileToPath.ContainsKey(m_CurrTargetTile))
            {
                Logger.Log(this.GetType().Name, "Begin moving from " + m_CurrUnit.CurrPosition + " to " + m_CurrTargetTile, LogLevel.LOG);
                m_BlockInputs = true;
                m_MapLogic.MoveUnit(GridType.PLAYER, m_CurrUnit, m_TileToPath[m_CurrTargetTile], EndTurn);
            }
            else if (m_CurrState == PlayerTurnState.SELECTING_ATTACK_TARGET && m_AttackPoints != null)
            {
                m_MapLogic.Attack(GridType.ENEMY, m_AttackPoints, 10f);
                Logger.Log(this.GetType().Name, "Attack!", LogLevel.LOG);
                EndTurn();
            }
        }

        // switch action type if possible
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            PlayerTurnState nextState = (PlayerTurnState) (((int) m_CurrState + 1) % Enum.GetValues(typeof(PlayerTurnState)).Length);
            m_CurrState = nextState;
            m_MapLogic.ResetMap();
            TransitToAction(m_CurrState);
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

    private void TransitToAction(PlayerTurnState currAction)
    {
        switch (currAction)
        {
            case PlayerTurnState.SELECTING_MOVEMENT_SQUARE:
                m_MapLogic.ColorMap(GridType.PLAYER, m_ReachablePoints);
                break;
        }

        Logger.Log(this.GetType().Name, "Current phase: " + currAction.ToString(), LogLevel.LOG);
    }
    #endregion

    #region End Turn
    private void EndTurn()
    {
        m_WithinTurn = false;
        m_CompleteTurnEvent?.Invoke(m_CurrUnit);
    }
    #endregion
}