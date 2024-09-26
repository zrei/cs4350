using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Class that maintains the internal representation of the grid (tile types, which tiles are occupied and with what units),
/// and the visual representation of the grid
/// </summary>
public class GridLogic : MonoBehaviour
{
    [SerializeField] private Transform m_GridParent;
    [SerializeField] private GridType m_GridType;

    #region Tiles
    private TileVisual[,] m_TileVisuals = new TileVisual[MapData.NUM_ROWS, MapData.NUM_COLS];
    private TileData[,] m_TileData = new TileData[MapData.NUM_ROWS, MapData.NUM_COLS];
    private MapData MapData => new MapData(m_TileData);
    #endregion

    private const float SPAWN_HEIGHT_OFFSET = 1.0f;

    public event MapInputEvent onTileSelect;
    public event MapInputEvent onTileSubmit;

    private CanvasGroup canvasGroup;

    #region Initialisation
    private void Start()
    {
        InitialiseTileData();
        InitialiseTileVisuals();

        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;
    }

    public void SetInteractable(bool interactable)
    {
        canvasGroup.interactable = interactable;
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                m_TileVisuals[r, c].selectable.interactable = interactable;
            }
        }
    }

    public void SetInteractableWhere(bool interactable, Func<TileVisual, bool> condition)
    {
        canvasGroup.interactable = interactable;
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                var tile = m_TileVisuals[r, c];
                tile.selectable.interactable = condition(tile);
            }
        }
    }

    private void InitialiseTileData()
    {   
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                m_TileData[r, c] = new TileData(TileType.NORMAL, false);
            }
        }
    }

    private void InitialiseTileVisuals()
    {
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            Transform row = m_GridParent.GetChild(r);
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                Transform tileTrf = row.GetChild(c);
                TileVisual tile = tileTrf.GetComponent<TileVisual>();
                tile.Initialise(m_GridType, new CoordPair(r, c));
                TileData data = m_TileData[r, c];
                m_TileVisuals[r, c] = tile;

                tile.selectable.onSelect.RemoveAllListeners();
                tile.selectable.onSelect.AddListener(() =>
                {
                    onTileSelect?.Invoke(data, tile);
                });
                tile.selectable.onSubmit.RemoveAllListeners();
                tile.selectable.onSubmit.AddListener(() =>
                {
                    onTileSubmit?.Invoke(data, tile);
                });
            }
        }
    }
    #endregion

    #region Graphics
    public void ColorReachablePoints(HashSet<PathNode> reachablePoints)
    {
        ResetPath();
        canvasGroup.interactable = true;
        foreach (PathNode pathNode in reachablePoints)
        {
            CoordPair coordinates = pathNode.m_Coordinates;
            var tile = m_TileVisuals[coordinates.m_Row, coordinates.m_Col];
            tile.SetTileState(TileState.TRAVERSABLE);
            tile.selectable.interactable = true;
        }
    }

    public void ShowAttackRange(Unit currentUnit, ActiveSkillSO skill)
    {
        canvasGroup.interactable = true;
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                var tileVisual = m_TileVisuals[r, c];
                var isAttackable = skill.IsValidTargetTile(tileVisual.Coordinates, currentUnit, m_GridType);
                if (isAttackable)
                {
                    tileVisual.selectable.interactable = true;
                    tileVisual.SetTileState(TileState.ATTACKABLE);
                }
            }
        }
    }

    public void ShowInspectable(bool ignoreEmpty)
    {
        canvasGroup.interactable = true;
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                var tileVisual = m_TileVisuals[r, c];
                var tileData = m_TileData[r, c];
                if (!ignoreEmpty || tileData.m_CurrUnit != null)
                {
                    tileVisual.selectable.interactable = true;
                    tileVisual.SetTileState(TileState.INSPECTABLE);
                }
            }
        }
    }

    public void ShowSetupTiles(List<CoordPair> validTiles)
    {
        canvasGroup.interactable = true;
        var set = new HashSet<CoordPair>(validTiles);
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                var tileVisual = m_TileVisuals[r, c];
                var tileData = m_TileData[r, c];
                if (set.Contains(tileVisual.Coordinates))
                {
                    tileVisual.selectable.interactable = true;
                    tileVisual.SetTileState(TileState.SWAPPABLE);
                }
            }
        }
    }

    public void ResetMap()
    {
        canvasGroup.interactable = false;
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                m_TileVisuals[r, c].ResetTile();
            }
        }
    }

    public void ResetPath()
    {
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                m_TileVisuals[r, c].TogglePath(false);
            }
        }
    }

    public void ResetTarget()
    {
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                m_TileVisuals[r, c].ToggleTarget(false);
            }
        }
    }

    public void ColorTarget(ActiveSkillSO attack, CoordPair targetTile)
    {
        ResetTarget();

        List<CoordPair> targetTiles = attack.ConstructAttackTargetTiles(targetTile);

        foreach (CoordPair target in targetTiles)
        {
            if (!MapData.WithinBounds(target))
                continue;
            m_TileVisuals[target.m_Row, target.m_Col].ToggleTarget(true);
        }
    }

    public void ColorPath(PathNode end)
    {
        ResetPath();
        
        PathNode pointer = end;
        while (pointer != null)
        {
            CoordPair coordinates = pointer.m_Coordinates;
            m_TileVisuals[coordinates.m_Row, coordinates.m_Col].TogglePath(true);
            pointer = pointer.m_Parent;
        }
    }
    #endregion

    #region Path
    private Stack<Vector3> GetPathPositions(PathNode end)
    {
        Stack<Vector3> pathPoints = new Stack<Vector3>();
        PathNode pointer = end;
        while (pointer != null)
        {
            pathPoints.Push(GetTilePosition(pointer.m_Coordinates));
            pointer = pointer.m_Parent;
        }
        return pathPoints;
    }

    public HashSet<PathNode> CalculateReachablePoints(Unit unit, int remainingMovementRange)
    {
        return Pathfinder.ReachablePoints(MapData, unit.CurrPosition, remainingMovementRange, unit.Stat.m_CanSwapTiles, unit.Stat.m_TraversableTileTypes);
    }
    #endregion

    #region Unit
    public void PlaceUnit(Unit unit, CoordPair coordinates)
    {
        unit.transform.parent = transform;
        unit.PlaceUnit(coordinates, GetTilePosition(coordinates));
        Logger.Log(this.GetType().Name, unit.gameObject.name, $"Placed unit at tile {coordinates} with world position {unit.transform.position}", unit.gameObject, LogLevel.LOG);
        m_TileData[coordinates.m_Row, coordinates.m_Col].m_IsOccupied = true;
        m_TileData[coordinates.m_Row, coordinates.m_Col].m_CurrUnit = unit;
    }

    public void MoveUnit(Unit unit, PathNode endPathNode, VoidEvent onCompleteMovement)
    {
        CoordPair start = unit.CurrPosition;
        CoordPair end = endPathNode.m_Coordinates;

        if (m_TileData[start.m_Row, start.m_Col].m_CurrUnit != unit)
        {
            Logger.Log(this.GetType().Name, "Unit stored in the tile data is not the same as unit to be moved", LogLevel.ERROR);
        }

        Stack<Vector3> movementCheckpoints = GetPathPositions(endPathNode);
        
        m_TileData[start.m_Row, start.m_Col].m_CurrUnit = m_TileData[end.m_Row, end.m_Col].m_CurrUnit;
        m_TileData[start.m_Row, start.m_Col].m_IsOccupied = m_TileData[start.m_Row, start.m_Col].m_CurrUnit != null;
        m_TileData[end.m_Row, end.m_Col].m_IsOccupied = true;
        m_TileData[end.m_Row, end.m_Col].m_CurrUnit = unit;

        // TODO: Handle actually VISUALLY swapping the other unit. When should it happen?
        unit.Move(end, movementCheckpoints, onCompleteMovement);
    }

    public void SwapTiles(CoordPair tile1, CoordPair tile2)
    {
        Unit tile1Unit = m_TileData[tile1.m_Row, tile1.m_Col].m_CurrUnit;
        m_TileData[tile1.m_Row, tile1.m_Col].m_CurrUnit = m_TileData[tile2.m_Row, tile2.m_Col].m_CurrUnit;
        m_TileData[tile1.m_Row, tile1.m_Col].m_IsOccupied = m_TileData[tile1.m_Row, tile1.m_Col].m_CurrUnit != null;
        m_TileData[tile2.m_Row, tile2.m_Col].m_CurrUnit = tile1Unit;
        m_TileData[tile2.m_Row, tile2.m_Col].m_IsOccupied = tile1Unit != null;

        if (m_TileData[tile1.m_Row, tile1.m_Col].m_IsOccupied)
        {
            m_TileData[tile1.m_Row, tile1.m_Col].m_CurrUnit.PlaceUnit(tile1, GetTilePosition(tile1));
        }

        if (m_TileData[tile2.m_Row, tile2.m_Col].m_IsOccupied)
        {
            m_TileData[tile2.m_Row, tile2.m_Col].m_CurrUnit.PlaceUnit(tile2, GetTilePosition(tile2));
        }
    }
    #endregion

    #region Helper
    private Vector3 GetTilePosition(CoordPair coordPair)
    {
        return m_TileVisuals[coordPair.m_Row, coordPair.m_Col].transform.position + new Vector3(0f, SPAWN_HEIGHT_OFFSET, 0f);
    }

    public bool IsTileOccupied(CoordPair tile)
    {
        return m_TileData[tile.m_Row, tile.m_Col].m_IsOccupied;
    }
    #endregion

    #region Attack
    // TODO: May want a dedicated attack handler, then this function only returns units that are hit
    /// <summary>
    /// Deals damage to all 
    /// </summary>
    /// <param name="attackPoints"></param>
    /// <param name="damage"></param>
    public void PerformSkill(Unit attacker, ActiveSkillSO attack, CoordPair targetTile, VoidEvent completeSkillEvent)
    {
        List<CoordPair> targetTiles = attack.ConstructAttackTargetTiles(targetTile);
        List<IHealth> targets = new();
        foreach (CoordPair coordPair in targetTiles)
        {
            if (!MapData.WithinBounds(coordPair))
                continue;
            
            if (m_TileData[coordPair.m_Row, coordPair.m_Col].m_IsOccupied)
            {
                targets.Add(m_TileData[coordPair.m_Row, coordPair.m_Col].m_CurrUnit);
            }
        }

        attacker.PostAttackEvent += CompleteSkill;
        attacker.PerformSKill(attack, targets);

        void CompleteSkill()
        {
            attacker.PostAttackEvent -= CompleteSkill;

            // TODO: Clean this up further?
            List<Unit> deadUnits = targets.Where(x => x.IsDead).Select(x => (Unit) x).ToList();

            foreach (Unit deadUnit in deadUnits)
            {
                CoordPair coordinates = deadUnit.CurrPosition;
                m_TileData[coordinates.m_Row, coordinates.m_Col].m_CurrUnit = null;
                m_TileData[coordinates.m_Row, coordinates.m_Col].m_IsOccupied = false;
                GlobalEvents.Battle.UnitDefeatedEvent?.Invoke(deadUnit);
            }

            completeSkillEvent?.Invoke();
        }
        
    }
    #endregion
}

public static class GridHelper
{
    public static bool IsSameSide(UnitAllegiance unitSide, GridType targetType)
    {
        return (unitSide == UnitAllegiance.PLAYER && targetType == GridType.PLAYER) || (unitSide == UnitAllegiance.ENEMY && targetType == GridType.ENEMY);
    }

    public static bool IsOpposingSide(UnitAllegiance unitSide, GridType targetType)
    {
        return (unitSide == UnitAllegiance.PLAYER && targetType == GridType.ENEMY) || (unitSide == UnitAllegiance.ENEMY && targetType == GridType.PLAYER);
    }

    public static GridType GetTargetType(ActiveSkillSO activeSkillSO, UnitAllegiance unitSide)
    {
        if (activeSkillSO.IsSelfTarget)
            return GetSameSide(unitSide);
        else if (activeSkillSO.IsOpposingSideTarget)
            return GetOpposingSide(unitSide);
        else
            return GetSameSide(unitSide);
    }   

    public static GridType GetSameSide(UnitAllegiance unitSide)
    {
        return unitSide switch
        {
            UnitAllegiance.PLAYER => GridType.PLAYER,
            UnitAllegiance.ENEMY => GridType.ENEMY,
            _ => GridType.PLAYER
        };
    }

    public static GridType GetOpposingSide(UnitAllegiance unitSide)
    {
        return unitSide switch
        {
            UnitAllegiance.PLAYER => GridType.ENEMY,
            UnitAllegiance.ENEMY => GridType.PLAYER,
            _ => GridType.PLAYER
        };
    }
}
