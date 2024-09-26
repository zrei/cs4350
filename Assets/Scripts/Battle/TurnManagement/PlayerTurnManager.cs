using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Input;
using System.Collections;

public enum PlayerTurnState
{
    SELECTING_ACTION,
    INSPECT,
    SELECTING_MOVEMENT_SQUARE,
    SELECTING_ACTION_TARGET
}

public class PlayerTurnManager : TurnManager
{
    #region Current State
    /// <summary>
    /// Currently controlled player unit
    /// </summary>
    private PlayerUnit m_CurrUnit;
    private PlayerTurnState m_CurrState = PlayerTurnState.SELECTING_MOVEMENT_SQUARE;
    #endregion

    #region Selected Tile
    private TileVisual selectedTileVisual;
    private TileData selectedTileData;
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

    private int m_TotalMovementRange;
    /// <summary>
    /// Number of movement squares remaining
    /// </summary>
    private int m_MovementRangeRemaining;

    public ActiveSkillSO SelectedSkill
    {
        get => selectedSkill;
        set
        {
            selectedSkill = value;
            // show attackable tiles
        }
    }
    private ActiveSkillSO selectedSkill;

    #region Initialisation
    private void Start()
    {
        m_TileToPath = new Dictionary<CoordPair, PathNode>();
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

        GlobalEvents.Battle.PreviewCurrentUnitEvent?.Invoke(m_CurrUnit);

        if (m_CurrUnit.IsDead)
        {
            GlobalEvents.Battle.UnitDefeatedEvent?.Invoke(m_CurrUnit);
            return;
        }

        Logger.Log(this.GetType().Name, "Tile unit is on: " + m_CurrUnit.CurrPosition, LogLevel.LOG);

        m_TotalMovementRange = (int) m_CurrUnit.GetTotalStat(StatType.MOVEMENT_RANGE);
        m_MovementRangeRemaining = m_TotalMovementRange;

        FillTraversablePoints();

        m_MapLogic.onTileSelect += OnTileSelect;
        m_MapLogic.onTileSubmit += OnTileSubmit;

        TransitToAction(PlayerTurnState.SELECTING_ACTION);
    }
    #endregion

    #region Update State
    private void OnTileSelect(TileData data, TileVisual visual)
    {
        selectedTileData = data;
        selectedTileVisual = visual;

        switch (m_CurrState)
        {
            case PlayerTurnState.SELECTING_ACTION_TARGET:
                UpdateAttackState();
                break;
            case PlayerTurnState.SELECTING_MOVEMENT_SQUARE:
                UpdateMoveState();
                break;
        }
    }

    private void UpdateMoveState()
    {
        if (selectedTileData == null || selectedTileVisual == null) return;

        if (selectedTileVisual.GridType == GridType.PLAYER && m_TileToPath.ContainsKey(selectedTileVisual.Coordinates))
        {
            m_MapLogic.ColorPath(GridType.PLAYER, m_TileToPath[selectedTileVisual.Coordinates]);
        }
        else
        {
            m_MapLogic.ResetPath();
        }            
    }

    private void UpdateAttackState()
    {
        if (selectedTileData == null || selectedTileVisual == null) return;

        if (SelectedSkill.IsValidTargetTile(selectedTileVisual.Coordinates, m_CurrUnit, selectedTileVisual.GridType))
        {
            m_MapLogic.SetTarget(selectedTileVisual.GridType, SelectedSkill, selectedTileVisual.Coordinates);
        }
        else
        {
            m_MapLogic.ResetTarget();
        }
    }
    #endregion

    #region Perform Action
    private void OnTileSubmit(TileData data, TileVisual visual)
    {
        TryPerformAction();
    }

    public bool TryPerformAction()
    {
        return m_CurrState switch
        {
            PlayerTurnState.SELECTING_ACTION_TARGET => TryPerformSkill(),
            PlayerTurnState.SELECTING_MOVEMENT_SQUARE => TryPerformMove(),
            PlayerTurnState.INSPECT => TryInspect(),
            PlayerTurnState.SELECTING_ACTION => TrySelectAction(),
            _ => false
        };
    }

    private bool TryPerformMove()
    {
        if (selectedTileData == null || selectedTileVisual == null) return false;

        if (selectedTileVisual.GridType == GridType.PLAYER && m_TileToPath.ContainsKey(selectedTileVisual.Coordinates))
        {
            Logger.Log(this.GetType().Name, "Begin moving from " + m_CurrUnit.CurrPosition + " to " + selectedTileVisual.Coordinates, LogLevel.LOG);
            PathNode destination = m_TileToPath[selectedTileVisual.Coordinates];
            int movedDistance = destination.m_Coordinates.GetDistanceToPoint(m_CurrUnit.CurrPosition);
            m_MovementRangeRemaining -= movedDistance;

            m_MapLogic.MoveUnit(GridType.PLAYER, m_CurrUnit, destination, OnCompleteMove);
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool TryPerformSkill()
    {
        if (selectedTileData == null || selectedTileVisual == null) return false;

        if (SelectedSkill.IsValidTargetTile(selectedTileVisual.Coordinates, m_CurrUnit, selectedTileVisual.GridType)
            && m_MapLogic.IsTileOccupied(selectedTileVisual.GridType, selectedTileVisual.Coordinates))
        {
            m_MapLogic.PerformSkill(
                selectedTileVisual.GridType,
                m_CurrUnit,
                SelectedSkill,
                selectedTileVisual.Coordinates,
                CompleteSkill);
            Logger.Log(this.GetType().Name, "Attack!", LogLevel.LOG);
            return true;
        }
        else
        {
            return false;
        }
        
        void CompleteSkill()
        {
            EndTurn();
        }
    }

    private bool TrySelectAction()
    {
        return true;
    }

    private bool TryInspect()
    {
        GlobalEvents.Battle.PreviewUnitEvent?.Invoke(selectedTileData.m_CurrUnit);
        return true;
    }

    private void StopInspect()
    {
        GlobalEvents.Battle.PreviewUnitEvent?.Invoke(null);
    }
    #endregion

    #region Moving
    private void OnCompleteMove()
    {
        Logger.Log(this.GetType().Name, $"Remaining movement: {m_MovementRangeRemaining}", LogLevel.LOG);

        if (m_MovementRangeRemaining <= 0)
        {
            EndTurn();
        }
        else
        {
            FillTraversablePoints();
            TransitToAction(PlayerTurnState.SELECTING_MOVEMENT_SQUARE);
        }
    }

    private void FillTraversablePoints()
    {
        m_ReachablePoints = m_MapLogic.CalculateReachablePoints(GridType.PLAYER, m_CurrUnit, m_MovementRangeRemaining);

        m_TileToPath.Clear();
        foreach (PathNode pathNode in m_ReachablePoints)
        {
            m_TileToPath.Add(pathNode.m_Coordinates, pathNode);
        }
    }
    #endregion

    #region Switch Action

    public void TransitToAction(PlayerTurnState currAction)
    {
        m_MapLogic.ResetMap();

        if (m_CurrState == PlayerTurnState.INSPECT && currAction != PlayerTurnState.INSPECT)
        {
            StopInspect();
        }

        m_CurrState = currAction;

        switch (currAction)
        {
            case PlayerTurnState.SELECTING_ACTION:
                //m_MapLogic.SetGridInteractable(GridType.PLAYER, false);
                //m_MapLogic.SetGridInteractable(GridType.ENEMY, false);
                break;
            case PlayerTurnState.SELECTING_ACTION_TARGET:
                if (SelectedSkill == null)
                {
                    // this should not be possible if unit has no available active skills
                    SelectedSkill = m_CurrUnit.GetAvailableActiveSkills()[0];
                }
                m_MapLogic.ShowAttackable(GridType.PLAYER, m_CurrUnit, SelectedSkill);
                m_MapLogic.ShowAttackable(GridType.ENEMY, m_CurrUnit, SelectedSkill);
                break;
            case PlayerTurnState.SELECTING_MOVEMENT_SQUARE:
                m_MapLogic.ColorMap(GridType.PLAYER, m_ReachablePoints);
                break;
            case PlayerTurnState.INSPECT:
                m_MapLogic.ShowInspectable(GridType.PLAYER);
                m_MapLogic.ShowInspectable(GridType.ENEMY);
                break;
        }

        Logger.Log(this.GetType().Name, "Current phase: " + m_CurrState.ToString(), LogLevel.LOG);
        GlobalEvents.Battle.PlayerPhaseUpdateEvent?.Invoke(m_CurrState);
    }
    #endregion

    #region End Turn
    public void EndTurn()
    {
        m_CompleteTurnEvent?.Invoke(m_CurrUnit);

        m_MapLogic.onTileSelect -= OnTileSelect;
        m_MapLogic.onTileSubmit -= OnTileSubmit;
    }
    #endregion
}