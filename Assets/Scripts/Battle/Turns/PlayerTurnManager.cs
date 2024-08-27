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
    private PlayerTurnState m_CurrState = PlayerTurnState.SELECTING_MOVEMENT_SQUARE;
    // handle units

    private bool m_WithinTurn = false;
    private PlayerUnit m_CurrUnit;
    private CoordPair m_CurrTile;
    private CoordPair m_CurrTargetTile;

    /// <summary>
    /// Maps coordinates to nodes that track paths
    /// </summary>
    private Dictionary<CoordPair, PathNode> m_TileToPath;
    private Stack<Vector3> m_CurrPath;

    private HashSet<PlayerTurnState> m_RemainingActions;
    private HashSet<PathNode> m_ReachablePoints;

    private void Start()
    {
        m_TileToPath = new Dictionary<CoordPair, PathNode>();
        m_RemainingActions = new HashSet<PlayerTurnState>();
    }

    /// <summary>
    /// Begin the turn involving the given player unit. Pre-calculates moveable squares.
    /// </summary>
    /// <param name="playerUnit"></param>
    public void BeginTurn(PlayerUnit playerUnit)
    {
        Logger.Log(this.GetType().Name, "Start turn with: " + playerUnit.name, LogLevel.LOG);
        
        m_CurrUnit = playerUnit;
        m_MapLogic.TryRetrieveTile(new Ray(m_CurrUnit.transform.position, -m_CurrUnit.transform.up), out m_CurrTile, out GridType _);
        Logger.Log(this.GetType().Name, "Tile unit is on: " + m_CurrTile, LogLevel.LOG);
        

        // TODO: Get traversable tiles for unit
        TileType[] traversableTiles = new TileType[1];
        traversableTiles[0] = TileType.NORMAL;
        // TODO: Fill in with actual unit data
        // TODO: POSSIBLY have the grid logic itself call this to color the map but return the reachable points? hm... need to update the tile data :|
        m_ReachablePoints = Pathfinder.ReachablePoints(m_MapLogic.RetrieveMapData(GridType.PLAYER), m_CurrTile, 3, true, traversableTiles);

        m_TileToPath.Clear();
        foreach (PathNode pathNode in m_ReachablePoints)
        {
            m_TileToPath.Add(pathNode.m_Coordinates, pathNode);
        }

        m_RemainingActions.Clear();

        foreach (PlayerTurnState action in Enum.GetValues(typeof(PlayerTurnState)))
        {
            m_RemainingActions.Add(action);
        }

        m_WithinTurn = true;
        m_CurrState = PlayerTurnState.SELECTING_MOVEMENT_SQUARE;
        m_MapLogic.ColorMap(GridType.PLAYER, m_ReachablePoints);
    }

    private void Update()
    {
        if (!m_WithinTurn)
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
                    m_CurrPath = m_MapLogic.TracePath(GridType.PLAYER, m_TileToPath[m_CurrTargetTile]);
                }
                else
                {
                    m_MapLogic.ResetPath();
                    m_CurrPath = null;
                }
                break;
            case PlayerTurnState.SELECTING_ATTACK_TARGET:
                // must hit last row: row 4
                if (hitGrid && gridType == GridType.ENEMY && m_CurrTargetTile.m_Row == 4)
                {
                    m_MapLogic.SetTarget(GridType.ENEMY, new() {m_CurrTargetTile, m_CurrTargetTile.MoveUp()});
                }
                else
                {
                    m_MapLogic.ResetTarget();
                }
                break;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (m_CurrState == PlayerTurnState.SELECTING_MOVEMENT_SQUARE && m_CurrPath != null)
            {
                m_CurrUnit.Move(m_CurrPath);
                m_MapLogic.MoveUnit(GridType.PLAYER, m_CurrTile, m_CurrTargetTile);
                TransitionAction(m_CurrState);
            }
        }

        // switch action type if possible
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            PlayerTurnState nextState = (PlayerTurnState) (((int) m_CurrState + 1) % Enum.GetValues(typeof(PlayerTurnState)).Length);
            if (m_RemainingActions.Contains(nextState))
            {
                m_CurrState = nextState;
                m_MapLogic.ResetMap();
                TransitToAction(m_CurrState);
            }
        }
    }

    private void EndTurn()
    {
        m_WithinTurn = false;
        m_CompleteTurnEvent?.Invoke();
    }

    private void TransitionAction(PlayerTurnState currAction)
    {
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

    private void TransitToAction(PlayerTurnState currAction)
    {
        switch (currAction)
        {
            case PlayerTurnState.SELECTING_MOVEMENT_SQUARE:
                m_MapLogic.ColorMap(GridType.PLAYER, m_ReachablePoints);
                break;
        }
    }
}