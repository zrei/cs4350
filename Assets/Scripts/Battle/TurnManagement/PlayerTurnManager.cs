using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerTurnState
{
    SELECTING_ACTION,
    INSPECT,
    SELECTING_MOVEMENT_SQUARE,
    SELECTING_ACTION_TARGET,
    SELECTING_TELEPORT_TARGET
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
    private Dictionary<CoordPair, PathNode> m_TileToPath = new();

    /// <summary>
    /// All reachable path nodes
    /// </summary>
    private HashSet<PathNode> m_ReachablePoints;

    private int m_TotalMovementRange;
    /// <summary>
    /// Number of movement squares remaining
    /// </summary>
    private int m_MovementRangeRemaining;
    public int MovementRangeRemaining
    {
        get => m_MovementRangeRemaining;
        private set
        {
            m_MovementRangeRemaining = value;
            OnMovementRangeRemainingChange?.Invoke(value);
        }
    }
    public event Action<int> OnMovementRangeRemainingChange;
    private CoordPair m_InitialPosition;

    public ActiveSkillSO SelectedSkill
    {
        get => selectedSkill;
        set
        {
            selectedSkill = value;
            int animationTrigger = 0;
            animationTrigger += (int)(selectedSkill.m_OverrideWeaponAnimationType ? selectedSkill.m_OverriddenWeaponAnimationType : m_CurrUnit.WeaponAnimationType);
            animationTrigger += (int)selectedSkill.m_SkillAnimationType;
            m_CurrUnit.PlaySkillStartAnimation(animationTrigger);
        }
    }
    private ActiveSkillSO selectedSkill;

    #region Special Teleport Handling
    /// <summary>
    /// Used to cache the target tile visual from active skill
    /// targeting to be used after selecting teleport tile
    /// </summary>
    private CoordPair m_CachedTargetTile;
    #endregion

    #region Start Turn
    private bool m_IsTurnInProgress;

    /// <summary>
    /// Begin the turn involving the given player unit. Pre-calculates moveable squares.
    /// </summary>
    /// <param name="playerUnit"></param>
    public void BeginTurn(PlayerUnit playerUnit)
    {
        m_IsTurnInProgress = true;

        Logger.Log(this.GetType().Name, "Start player turn with: " + playerUnit.name, LogLevel.LOG);
        GlobalEvents.Battle.PlayerTurnStartEvent?.Invoke();

        m_CurrUnit = playerUnit;
        m_CurrUnit.Tick();

        GlobalEvents.Battle.PreviewCurrentUnitEvent?.Invoke(m_CurrUnit);

        if (m_CurrUnit.IsDead)
        {
            GlobalEvents.Battle.UnitDefeatedEvent?.Invoke(m_CurrUnit);
            EndTurn();
            return;
        }

        if (!PreTurn(playerUnit))
        {
            EndTurn();
            return;
        }

        Logger.Log(this.GetType().Name, "Tile unit is on: " + m_CurrUnit.CurrPosition, LogLevel.LOG);

        m_TotalMovementRange = (int)m_CurrUnit.GetTotalStat(StatType.MOVEMENT_RANGE);
        MovementRangeRemaining = m_TotalMovementRange;
        m_InitialPosition = m_CurrUnit.CurrPosition;

        m_MapLogic.onTileSelect += OnTileSelect;
        m_MapLogic.onTileSubmit += OnTileSubmit;

        TransitToAction(PlayerTurnState.SELECTING_ACTION);
    }
    #endregion

    #region Update State
    private void OnTileSelect(TileData data, TileVisual visual)
    {
        var prevUnit = selectedTileData?.m_CurrUnit;
        if (prevUnit != null && prevUnit.UnitAllegiance == UnitAllegiance.ENEMY)
        {
            var enemyUnit = prevUnit as EnemyUnit;
            var nextAction = enemyUnit.NextAction;
            if (nextAction is EnemyActiveSkillActionWrapper enemyActiveSkillAction)
            {
                m_MapLogic.ShowAttackForecast(
                    enemyActiveSkillAction.TargetGridType,
                    new CoordPair[] { });
            }
        }

        selectedTileData = data;
        selectedTileVisual = visual;

        var currentUnit = selectedTileData.m_CurrUnit;
        GlobalEvents.Battle.PreviewUnitEvent?.Invoke(currentUnit);

        switch (m_CurrState)
        {
            case PlayerTurnState.SELECTING_ACTION:
            case PlayerTurnState.INSPECT:
                if (currentUnit != null && currentUnit.UnitAllegiance == UnitAllegiance.ENEMY)
                {
                    var enemyUnit = currentUnit as EnemyUnit;
                    var nextAction = enemyUnit.NextAction;
                    if (nextAction is EnemyActiveSkillActionWrapper enemyActiveSkillAction)
                    {
                        m_MapLogic.ShowAttackForecast(
                            enemyActiveSkillAction.TargetGridType,
                            enemyActiveSkillAction.PossibleAttackPositions);
                    }
                }
                break;
            case PlayerTurnState.SELECTING_MOVEMENT_SQUARE:
                UpdateMoveState();
                break;
            case PlayerTurnState.SELECTING_ACTION_TARGET:
                UpdateActiveSkillState();
                break;
            case PlayerTurnState.SELECTING_TELEPORT_TARGET:
                UpdateTeleportState();
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

    private void UpdateActiveSkillState()
    {
        if (selectedTileData == null || selectedTileVisual == null) return;

        if (m_MapLogic.CanPerformSkill(SelectedSkill, m_CurrUnit, selectedTileVisual.Coordinates, selectedTileVisual.GridType))
        {
            m_MapLogic.SetTarget(selectedTileVisual.GridType, SelectedSkill, selectedTileVisual.Coordinates);
        }
        else
        {
            m_MapLogic.ResetTarget();
        }
    }

    private void UpdateTeleportState()
    {
        if (selectedTileData == null || selectedTileVisual == null) return;

        if (m_MapLogic.IsValidTeleportTile(SelectedSkill, m_CurrUnit, selectedTileVisual.Coordinates, selectedTileVisual.GridType))
        {
            m_MapLogic.SetTeleportTarget(selectedTileVisual.GridType, selectedTileVisual.Coordinates, m_CachedTargetTile);
        }
        else
        {
            m_MapLogic.ResetPath();
        }
    }
    #endregion

    #region Perform Action
    private bool m_IsPerformingAction;

    private void OnTileSubmit(TileData data, TileVisual visual)
    {
        TryPerformAction();
    }

    public bool TryPerformAction()
    {
        m_IsPerformingAction = m_CurrState switch
        {
            PlayerTurnState.SELECTING_ACTION_TARGET => TryPerformSkill(),
            PlayerTurnState.SELECTING_MOVEMENT_SQUARE => TryPerformMove(),
            PlayerTurnState.INSPECT => TryInspect(),
            PlayerTurnState.SELECTING_ACTION => TryInspect(),
            PlayerTurnState.SELECTING_TELEPORT_TARGET => TryPerformTeleport(),
            _ => false
        };
        return m_IsPerformingAction;
    }

    private bool TryPerformMove()
    {
        if (selectedTileData == null || selectedTileVisual == null) return false;

        if (selectedTileVisual.GridType == GridType.PLAYER && m_TileToPath.ContainsKey(selectedTileVisual.Coordinates))
        {
            if (m_MovementRangeRemaining <= 0)
            {
                EndTurn();
                return false;
            }

            Logger.Log(this.GetType().Name, "Begin moving from " + m_CurrUnit.CurrPosition + " to " + selectedTileVisual.Coordinates, LogLevel.LOG);
            PathNode destination = m_TileToPath[selectedTileVisual.Coordinates];
            int movedDistance = destination.m_Coordinates.GetDistanceToPoint(m_CurrUnit.CurrPosition);
            MovementRangeRemaining -= movedDistance;

            m_MapLogic.ResetMap();
            m_MapLogic.MoveUnit(GridType.PLAYER, m_CurrUnit, destination, OnCompleteMove);
            return true;
        }

        return false;
    }

    private bool TryPerformSkill()
    {
        if (selectedTileData == null || selectedTileVisual == null) return false;

        if (m_MapLogic.CanPerformSkill(SelectedSkill, m_CurrUnit, selectedTileVisual.Coordinates, selectedTileVisual.GridType, true))
        {
            if (selectedSkill.ContainsSkillType(SkillEffectType.TELEPORT))
            {
                m_CachedTargetTile = selectedTileVisual.Coordinates;
                TransitToAction(PlayerTurnState.SELECTING_TELEPORT_TARGET);
                return false;
            }
            m_MapLogic.PerformSkill(
                selectedTileVisual.GridType,
                m_CurrUnit,
                SelectedSkill,
                selectedTileVisual.Coordinates,
                CompleteSkill);
            m_MapLogic.ResetMap();
            Logger.Log(this.GetType().Name, "Attack!", LogLevel.LOG);

            GlobalEvents.Battle.PreviewCurrentUnitEvent?.Invoke(null);
            GlobalEvents.Battle.PreviewUnitEvent?.Invoke(null);
            return true;
        }
        else
        {
            return false;
        }

        void CompleteSkill()
        {
            EndTurn();
            m_IsPerformingAction = false;
        }
    }

    private bool TryPerformTeleport()
    {
        if (selectedTileData == null || selectedTileVisual == null) return false;

        if (m_MapLogic.IsValidTeleportTile(SelectedSkill, m_CurrUnit, selectedTileVisual.Coordinates, selectedTileVisual.GridType))
        {
            m_MapLogic.PerformTeleportSkill(
                selectedTileVisual.GridType,
                m_CurrUnit,
                SelectedSkill,
                m_CachedTargetTile,
                selectedTileVisual.Coordinates,
                CompleteSkill);
            m_MapLogic.ResetMap();
            Logger.Log(this.GetType().Name, "Attack + Teleport!", LogLevel.LOG);
            return true;
        }
        else
        {
            return false;
        }

        void CompleteSkill()
        {
            EndTurn();
            m_IsPerformingAction = false;
        }
    }

    private bool TryInspect()
    {
        return false;
    }
    #endregion

    #region Moving
    private void OnCompleteMove()
    {
        Logger.Log(this.GetType().Name, $"Remaining movement: {m_MovementRangeRemaining}", LogLevel.LOG);

        if (m_MovementRangeRemaining > 0)
        {
            FillTraversablePoints();
            TransitToAction(PlayerTurnState.SELECTING_MOVEMENT_SQUARE);
        }
        else
        {
            //var node = new PathNode(m_CurrUnit.CurrPosition, null);
            //m_ReachablePoints.Clear();
            //m_ReachablePoints.Add(node);
            //m_TileToPath.Clear();
            //m_TileToPath.Add(node.m_Coordinates, node);
            EndTurn();
        }
        //TransitToAction(PlayerTurnState.SELECTING_MOVEMENT_SQUARE);

        m_IsPerformingAction = false;
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
    public bool TryCancelCurrentAction()
    {
        if (!m_IsTurnInProgress || m_IsPerformingAction) return false;

        TransitToAction(PlayerTurnState.SELECTING_ACTION);
        return true;
    }

    public void TransitToAction(PlayerTurnState currAction)
    {
        m_MapLogic.ResetMap();

        if (m_CurrState == PlayerTurnState.SELECTING_ACTION_TARGET && currAction != PlayerTurnState.SELECTING_ACTION_TARGET && currAction != PlayerTurnState.SELECTING_TELEPORT_TARGET)
        {
            m_CurrUnit.CancelSkillAnimation();
        }
        if (m_CurrState == PlayerTurnState.SELECTING_MOVEMENT_SQUARE && currAction != PlayerTurnState.SELECTING_MOVEMENT_SQUARE)
        {
            if (m_MovementRangeRemaining < m_TotalMovementRange)
            {
                m_CurrUnit.CancelMove();
                m_MapLogic.SwapTiles(GridType.PLAYER, m_CurrUnit.CurrPosition, m_InitialPosition);
                MovementRangeRemaining = m_TotalMovementRange;
            }
        }
        if (m_CurrState != PlayerTurnState.SELECTING_MOVEMENT_SQUARE && currAction == PlayerTurnState.SELECTING_MOVEMENT_SQUARE)
        {
            FillTraversablePoints();
        }

        m_CurrState = currAction;

        GlobalEvents.Battle.PreviewUnitEvent?.Invoke(null);

        switch (currAction)
        {
            case PlayerTurnState.SELECTING_ACTION:
            case PlayerTurnState.INSPECT:
                m_MapLogic.ShowInspectable(GridType.PLAYER);
                m_MapLogic.ShowInspectable(GridType.ENEMY);
                break;
            case PlayerTurnState.SELECTING_TELEPORT_TARGET:
                m_MapLogic.ShowTeleportable(GridType.PLAYER, m_CurrUnit, SelectedSkill, m_CachedTargetTile);
                m_MapLogic.ShowTeleportable(GridType.ENEMY, m_CurrUnit, SelectedSkill, m_CachedTargetTile);
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
        }

        Logger.Log(this.GetType().Name, "Current phase: " + m_CurrState.ToString(), LogLevel.LOG);
        GlobalEvents.Battle.PlayerPhaseUpdateEvent?.Invoke(m_CurrState);
    }
    #endregion

    #region End Turn
    public void EndTurn()
    {
        m_IsTurnInProgress = false;

        m_CompleteTurnEvent?.Invoke(m_CurrUnit);

        m_CurrUnit.CancelSkillAnimation();

        m_MapLogic.onTileSelect -= OnTileSelect;
        m_MapLogic.onTileSubmit -= OnTileSubmit;
        m_MapLogic.ResetMap();

        GlobalEvents.Battle.PreviewCurrentUnitEvent?.Invoke(null);
        GlobalEvents.Battle.PreviewUnitEvent?.Invoke(null);
    }
    #endregion
}
