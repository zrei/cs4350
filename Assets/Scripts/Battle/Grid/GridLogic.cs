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

    public void ShowTeleportRange(ActiveSkillSO skill, Unit unit, CoordPair initialTarget)
    {
        canvasGroup.interactable = true;

        // color the targeted unit's tile
        if (GridHelper.GetTargetType(skill, unit.UnitAllegiance) == m_GridType)
            m_TileVisuals[initialTarget.m_Row, initialTarget.m_Col].ToggleTarget(true);
        
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                var tileVisual = m_TileVisuals[r, c];
                var isTeleportable = IsValidTeleportTile(skill, unit, new CoordPair(r, c));
                if (isTeleportable)
                {
                    tileVisual.selectable.interactable = true;
                    tileVisual.SetTileState(TileState.TRAVERSABLE);
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

    public void ShowSetupTiles(IEnumerable<CoordPair> validTiles)
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

    public void ShowAttackForecast(IEnumerable<CoordPair> validTiles)
    {
        var set = new HashSet<CoordPair>(validTiles);
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                var tileVisual = m_TileVisuals[r, c];
                var tileData = m_TileData[r, c];
                tileVisual.ToggleAttackForecast(set.Contains(tileVisual.Coordinates));
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

    /// <summary>
    /// Reset path
    /// </summary>
    /// <param name="ignored">Coordinates for tiles that should be ignored</param>
    public void ResetPath(params CoordPair[] ignored)
    {
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                if (!ignored.Contains(new CoordPair(r, c)))
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

    public void ColorTeleportTarget(CoordPair targetTile, CoordPair initialTargetTile)
    {
        // do not reset the target unit's tile
        ResetPath(initialTargetTile);

        if (MapData.WithinBounds(targetTile))
            m_TileVisuals[targetTile.m_Row, targetTile.m_Col].TogglePath(true);
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
        return Pathfinder.ReachablePoints(MapData, unit.CurrPosition, remainingMovementRange, unit.CanSwapTiles, unit.TraversableTileTypes);
    }
    #endregion

    #region Unit
    public void PlaceUnit(Unit unit, CoordPair coordinates)
    {
        unit.transform.parent = transform;
        unit.transform.localRotation = Quaternion.identity;
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

    public void RemoveUnit(Unit unit)
    {
        m_TileData[unit.CurrPosition.m_Row, unit.CurrPosition.m_Col].m_CurrUnit = null;
        m_TileData[unit.CurrPosition.m_Row, unit.CurrPosition.m_Col].m_IsOccupied = false;
    }
    #endregion

    #region Helper
    private Vector3 GetTilePosition(CoordPair coordPair)
    {
        return m_TileVisuals[coordPair.m_Row, coordPair.m_Col].transform.position;
    }

    private bool IsTileOccupied(CoordPair tile)
    {
        return m_TileData[tile.m_Row, tile.m_Col].m_IsOccupied;
    }

    // assumes that tile is filled
    public Unit GetUnitAtTile(CoordPair tile)
    {   
        return m_TileData[tile.m_Row, tile.m_Col].m_CurrUnit;
    }

    // threshold object... should return false if there is no unit at all
    // but hey that shouldn't be possible to begin with
    public bool HasAnyUnitWithHealthThreshold(float threshold, bool greaterThan)
    {

    }

    /// <summary>
    /// Checks that it's a valid target tile for the active skill and also checks that at least one square within the target is occupied
    /// </summary>
    /// <param name="activeSkillSO"></param>
    /// <param name="unit"></param>
    /// <param name="targetTile"></param>
    /// <returns></returns>
    public bool IsValidSkillTargetTile(ActiveSkillSO activeSkillSO, Unit unit, CoordPair targetTile, bool checkOccupied)
    {
        if (!activeSkillSO.IsValidTargetTile(targetTile, unit, m_GridType))
            return false;

        if (!checkOccupied)
            return true;

        foreach (CoordPair tile in GetInBoundsTargetTiles(activeSkillSO, targetTile))
        {
            if (IsTileOccupied(tile))
                return true;
        }

        return false;
    }

    public bool IsValidTeleportTile(ActiveSkillSO activeSkillSO, Unit unit, CoordPair targetTile)
    {
        if (!activeSkillSO.IsValidTeleportTargetTile(targetTile, unit, m_GridType))
            return false;

        if (IsTileOccupied(targetTile))
            return false;

        return true;
    }

    private List<CoordPair> GetInBoundsTargetTiles(ActiveSkillSO activeSkillSO, CoordPair targetTile)
    {
        List<CoordPair> targetTiles = new();
        foreach (CoordPair coordPair in activeSkillSO.ConstructAttackTargetTiles(targetTile))
        {
            if (!MapData.WithinBounds(coordPair))
                continue;

            targetTiles.Add(coordPair);
        }
        return targetTiles;
    }

    public int GetNumberOfUnitsOnGrid()
    {
        int numUnits = 0;
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                if (m_TileData[r, c].m_IsOccupied)
                    ++numUnits;
            }
        }
        return numUnits;
    }
    #endregion

    #region Attack
    // TODO: May want a dedicated attack handler, then this function only returns units that are hit
    /// <summary>
    /// Deals damage to all 
    /// </summary>
    /// <param name="attackPoints"></param>
    /// <param name="damage"></param>
    public void PerformSkill(Unit attacker, ActiveSkillSO activeSkill, CoordPair targetTile, VoidEvent completeSkillEvent)
    {
        PerformSkill_Shared(attacker, activeSkill, targetTile, CompleteSkill);

        void CompleteSkill(List<IHealth> targets)
        {
            attacker.PostSkillEvent = null;

            // TODO: Clean this up further?
            List<Unit> deadUnits = targets.Where(x => x.IsDead).Select(x => (Unit) x).ToList();

            foreach (Unit deadUnit in deadUnits)
            {
                GlobalEvents.Battle.UnitDefeatedEvent?.Invoke(deadUnit);
            }

            // Note: Attacker is killed after targets to ensure attacker's side still gets priority at victory
            if (attacker.IsDead)
            {
                GlobalEvents.Battle.UnitDefeatedEvent?.Invoke(attacker);
            }

            completeSkillEvent?.Invoke();
        }
        
    }

    public void PerformTeleportSkill(Unit attacker, ActiveSkillSO activeSkill, CoordPair targetTile, CoordPair teleportTile, VoidEvent completeSkillEvent)
    {
        PerformSkill_Shared(attacker, activeSkill, targetTile, CompleteSkill);

        void CompleteSkill(List<IHealth> targets)
        {
            attacker.PostSkillEvent = null;

            foreach (IHealth target in targets)
            {
                if (target.IsDead)
                    GlobalEvents.Battle.UnitDefeatedEvent?.Invoke((Unit) target);
                else
                    SwapTiles(targetTile, teleportTile);
            }

            // Note: Attacker is killed after targets to ensure attacker's side still gets priority at victory
            if (attacker.IsDead)
            {
                GlobalEvents.Battle.UnitDefeatedEvent?.Invoke(attacker);
            }

            completeSkillEvent?.Invoke();
        }
    }

    private void PerformSkill_Shared(Unit attacker, ActiveSkillSO activeSkill, CoordPair targetTile, Action<List<IHealth>> postSkillEvent)
    {
        List<IHealth> targets = new();
        foreach (CoordPair coordPair in GetInBoundsTargetTiles(activeSkill, targetTile))
        {       
            if (IsTileOccupied(coordPair))
            {
                targets.Add(m_TileData[coordPair.m_Row, coordPair.m_Col].m_CurrUnit);
            }
        }

        attacker.PostSkillEvent += () => postSkillEvent(targets);
        attacker.PerformSkill(activeSkill, targets);

        if (activeSkill.ContainsSkillType(SkillEffectType.SUMMON))
        {
            foreach (SummonWrapper summon in activeSkill.m_Summons)
            {
                List<CoordPair> summonPositions = GetSummonPositions(summon.m_PrioritsePositions, summon.m_PrioritisedRows, summon.m_PrioritisedCols, summon.m_Adds.Count);
                for (int i = 0; i < summonPositions.Count; ++i)
                {
                    // TODO: Possible animation delay
                    BattleManager.Instance.InstantiateEnemyUnit(new() {m_Coordinates = summonPositions[i], m_EnemyCharacterData = summon.m_Adds[i].m_EnemyCharacterSO, m_StatAugments = summon.m_Adds[i].m_StatAugments});
                }
            }
        }
    }
    #endregion

    #region Summon Helper
    private List<CoordPair> GetSummonPositions(bool willPrioritse, List<int> prioritisedRows, List<int> prioritisedCols, int numUnits)
    {
        HashSet<CoordPair> possibleTiles = new();

        if (!willPrioritse)
        {
            for (int r = 0; r < MapData.NUM_ROWS; ++r)
            {
                for (int c = 0; c < MapData.NUM_COLS; ++c)
                {
                    CoordPair tile = new CoordPair(r, c);
                    if (!IsTileOccupied(tile))
                        possibleTiles.Add(new CoordPair(r, c));
                }
            }
        }
        else
        {
            foreach (int row in prioritisedRows)
            {
                for (int c = 0; c < MapData.NUM_COLS; ++c)
                {
                    CoordPair tile = new CoordPair(row, c);
                    if (!possibleTiles.Contains(tile) && !IsTileOccupied(tile))
                        possibleTiles.Add(tile);
                }
            }

            foreach (int col in prioritisedCols)
            {
                for (int r = 0; r < MapData.NUM_ROWS; ++r)
                {
                    CoordPair tile = new CoordPair(r, col);
                    if (!possibleTiles.Contains(tile) && !IsTileOccupied(tile))
                        possibleTiles.Add(tile);
                }
            }
        }

        List<CoordPair> possibleTileList = possibleTiles.ToList();
        List<CoordPair> spawnTiles = new();

        for (int i = 0; i < numUnits; ++i)
        {
            if (possibleTileList.Count == 0)
                break;

            int index = UnityEngine.Random.Range(0, possibleTileList.Count - 1);
            spawnTiles.Add(possibleTileList[index]);
            possibleTileList.RemoveAt(index);
        }

        return spawnTiles;
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
