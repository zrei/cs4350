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

    public delegate void TeleportEvent(GridType teleportTargetGrid, CoordPair teleportStartTile, CoordPair teleportDestination);
    public TeleportEvent OnTeleportUnit;

    public GridType GridType => m_GridType;

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
                m_TileData[r, c] = new TileData(false);
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

    #region Tick
    public void Tick(float tickTime)
    {
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                m_TileData[r, c].Tick(tickTime, m_TileVisuals[r, c].ClearEffects);
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
        // check that this grid is the attacker's grid
        bool isSameSideAsAttacker = GridHelper.IsSameSide(currentUnit.UnitAllegiance, m_GridType);
        // if there's no attacker limitations, then there's no need to check for attacker range
        bool hasAttackerLimitations = skill.HasAttackerLimitations;
        bool isAttackerValid = !hasAttackerLimitations || skill.IsValidAttackerTile(currentUnit.CurrPosition);

        canvasGroup.interactable = true;

        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                var tileVisual = m_TileVisuals[r, c];
                var isAttackable = skill.IsValidTargetTile(tileVisual.Coordinates, currentUnit, m_GridType);
                var isAttackerOutOfPosition = isSameSideAsAttacker && hasAttackerLimitations && !skill.IsValidAttackerTile(tileVisual.Coordinates);
                if (isAttackable)
                {
                    tileVisual.selectable.interactable = isAttackerValid;
                    tileVisual.SetTileState(TileState.ATTACKABLE);
                }
                
                if (isAttackerOutOfPosition)
                {
                    tileVisual.ToggleSkillCastBlocked(true);
                }
            }
        }
    }

    public bool IsAttackerOutOfPosition(Unit unit, ActiveSkillSO skill)
    {
        return skill.HasAttackerLimitations && !skill.IsValidAttackerTile(unit.CurrPosition);
    }

    public void ShowTeleportRange(ActiveSkillSO skill, Unit unit, CoordPair initialTarget)
    {
        canvasGroup.interactable = true;
        GridType targetGridType = skill.TargetGridType(unit.UnitAllegiance);

        // color the targeted unit's tile
        if (targetGridType == m_GridType)
            m_TileVisuals[initialTarget.m_Row, initialTarget.m_Col].ToggleTarget(true);

        CoordPair teleportStartTile = skill.TeleportStartTile(unit, initialTarget);
        GridType teleportGridType = skill.TeleportTargetGrid(unit);
        // the teleported unit is the self, and it is different from the targeted unit
        if (skill.m_TeleportSelf && teleportGridType == m_GridType && (teleportGridType != targetGridType || !teleportStartTile.Equals(initialTarget)))
            m_TileVisuals[teleportStartTile.m_Row, teleportStartTile.m_Col].ToggleTarget(true);

        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                var tileVisual = m_TileVisuals[r, c];
                var isTeleportable = IsValidTeleportTile(skill, unit, initialTarget, tileVisual.Coordinates);
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
        return Pathfinder.ReachablePoints(MapData, unit.CurrPosition, remainingMovementRange, unit.CanSwapTiles, unit.MovementType, unit.TraversableTileTypes);
    }

    public void TryReachTile(Unit unit, CoordPair destination, VoidEvent onCompleteMovement)
    {
        if (!Pathfinder.TryPathfind(MapData, unit.CurrPosition, destination, out PathNode pathNode, unit.TraversableTileTypes))
        {
            onCompleteMovement?.Invoke();
            return;
        }

        MoveUnit(unit, pathNode, onCompleteMovement);
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
        int movementRange = (int) unit.GetTotalStat(StatType.MOVEMENT_RANGE);
        PathNode finalPathNode = endPathNode;

        // calculate the final path node along the desired path that the unit
        // can actually reach with their current movement range
        int count = 0;
        PathNode pointer = endPathNode;
        while (pointer != null)
        {
            if (count == movementRange + 1)
            {
                finalPathNode = finalPathNode.m_Parent;
            }
            else
            {
                ++count;
            }
            pointer = pointer.m_Parent;
        }

        CoordPair start = unit.CurrPosition;
        CoordPair end = finalPathNode.m_Coordinates;

        if (m_TileData[start.m_Row, start.m_Col].m_CurrUnit != unit)
        {
            Logger.Log(this.GetType().Name, "Unit stored in the tile data is not the same as unit to be moved", LogLevel.ERROR);
        }

        Stack<Vector3> movementCheckpoints = GetPathPositions(finalPathNode);
        
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
    public Vector3 GetTilePosition(CoordPair coordPair)
    {
        return m_TileVisuals[coordPair.m_Row, coordPair.m_Col].transform.position;
    }

    public bool IsTileOccupied(CoordPair tile)
    {
        return m_TileData[tile.m_Row, tile.m_Col].m_IsOccupied;
    }

    // assumes that tile is filled
    public Unit GetUnitAtTile(CoordPair tile)
    {   
        return m_TileData[tile.m_Row, tile.m_Col].m_CurrUnit;
    }

    // will return false if there is no unit
    public bool HasAnyUnitWithHealthThreshold(Threshold threshold, bool isFlat)
    {
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                if (m_TileData[r, c].m_IsOccupied && threshold.IsSatisfied(isFlat ? m_TileData[r, c].m_CurrUnit.CurrentHealth : m_TileData[r, c].m_CurrUnit.CurrentHealthProportion))
                    return true;
            }
        }
        return false;
    }

    // will return false if there is no unit
    public bool HasAnyUnitWithManaThreshold(Threshold threshold, bool isFlat)
    {
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                if (m_TileData[r, c].m_IsOccupied && threshold.IsSatisfied(isFlat ? m_TileData[r, c].m_CurrUnit.CurrentMana : m_TileData[r, c].m_CurrUnit.CurrentManaProportion))
                    return true;
            }
        }
        return false;
    }

    public bool HasAnyUnitWithToken(TokenType tokenType)
    {
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                if (m_TileData[r, c].m_IsOccupied && m_TileData[r, c].m_CurrUnit.HasToken(tokenType))
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks that it's a valid target tile for the active skill and also checks that at least one square within the target is occupied
    /// </summary>
    /// <param name="activeSkillSO"></param>
    /// <param name="unit"></param>
    /// <param name="targetTile"></param>
    /// <returns></returns>
    private bool IsValidSkillTargetTile(ActiveSkillSO activeSkillSO, Unit unit, CoordPair targetTile, bool checkOccupied)
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

    public bool CanPerformSkill(ActiveSkillSO activeSkillSO, Unit unit, CoordPair targetTile, bool checkOccupied)
    {
        return IsValidSkillTargetTile(activeSkillSO, unit, targetTile, checkOccupied) && activeSkillSO.IsValidAttackerTile(unit.CurrPosition);
    }

    public bool IsValidTeleportTile(ActiveSkillSO activeSkillSO, Unit unit, CoordPair initialTarget, CoordPair targetTile)
    {
        if (!activeSkillSO.IsValidTeleportTargetTile(initialTarget, targetTile, unit, m_GridType))
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

    public int GetNumberOfUnitsTargeted(ActiveSkillSO activeSkillSO, CoordPair targetTile)
    {
        int numUnits = 0;

        foreach (CoordPair tile in GetInBoundsTargetTiles(activeSkillSO, targetTile))
        {
            if (IsTileOccupied(tile))
                ++numUnits;
        }

        return numUnits;
    }

    public float GetDamageDoneBySkill(Unit unit, ActiveSkillSO activeSkillSO, CoordPair targetTile)
    {
        float totalDamage = 0f;

        foreach (CoordPair tile in GetInBoundsTargetTiles(activeSkillSO, targetTile))
        {
            if (IsTileOccupied(tile))
                totalDamage += DamageCalc.CalculateDamage(unit, m_TileData[tile.m_Row, tile.m_Col].m_CurrUnit, activeSkillSO);
        }

        return totalDamage;
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

    public IEnumerable<CoordPair> GetUnoccupiedTiles()
    {
        List<CoordPair> tiles = new();
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                if (!m_TileData[r, c].m_IsOccupied)
                    tiles.Add(new CoordPair(r, c));
            }
        }
        return tiles;
    }
    #endregion

    #region Attack
    // TODO: May want a dedicated attack handler, then this function only returns units that are hit
    /// <summary>
    /// Deals damage to all 
    /// </summary>
    /// <param name="attackPoints"></param>
    /// <param name="damage"></param>
    public void PerformSkill(Unit attacker, ActiveSkillSO activeSkill, CoordPair targetTile, BoolEvent completeSkillEvent)
    {
        PerformSkill_Shared(attacker, activeSkill, targetTile, CompleteSkill, null);

        void CompleteSkill(List<IHealth> targets, bool canExtendTurn, HashSet<Unit> additionalDeadUnits)
        {
            attacker.PostSkillEvent = null;

            TryApplySkillTileEffects(activeSkill, targetTile);

            // TODO: Clean this up further?
            IEnumerable<Unit> deadUnits = targets.Where(x => x.IsDead).Select(x => (Unit) x);
            foreach (Unit unit in deadUnits)
                additionalDeadUnits.Add(unit);

            float volumeModifier = 1f / (additionalDeadUnits.Count + (additionalDeadUnits.Contains(attacker) ? 0 : attacker.IsDead ? 1 : 0));

            foreach (Unit deadUnit in additionalDeadUnits)
            {
                if (deadUnit != attacker)
                {
                    deadUnit.PlayDeathSound(volumeModifier);
                    deadUnit.Die();
                }
            }

            // Note: Attacker is killed after targets to ensure attacker's side still gets priority at victory
            if (attacker.IsDead)
            {
                attacker.PlayDeathSound(volumeModifier);
                attacker.Die();
            }

            completeSkillEvent?.Invoke(canExtendTurn);
        }
        
    }

    public void PerformTeleportSkill(
        Unit attacker, 
        ActiveSkillSO activeSkill, 
        CoordPair targetTile, 
        CoordPair teleportTile, 
        Vector3 teleportTargetPosition,
        BoolEvent completeSkillEvent)
    {
        PerformSkill_Shared(attacker, activeSkill, targetTile, CompleteSkill, teleportTargetPosition);

        void CompleteSkill(List<IHealth> targets, bool canExtendTurn, HashSet<Unit> additionalDeadUnits)
        {
            attacker.PostSkillEvent = null;

            TryApplySkillTileEffects(activeSkill, targetTile);

            
            foreach (IHealth target in targets)
            {
                if (target.IsDead)
                {
                    Unit unit = (Unit) target;
                    additionalDeadUnits.Add(unit);
                    /*
                    unit.PlayDeathSound(volumeModifier);
                    unit.Die();
                    */
                }
                
                if (!activeSkill.m_TeleportSelf && !target.IsDead)
                {
                    OnTeleportUnit?.Invoke(activeSkill.TeleportTargetGrid(attacker), activeSkill.TeleportStartTile(attacker, targetTile), teleportTile);
                }
            }

            float volumeModifier = 1f / (additionalDeadUnits.Count + (additionalDeadUnits.Contains(attacker) ? 0 : attacker.IsDead ? 1 : 0));

            foreach (Unit deadUnit in additionalDeadUnits)
            {
                if (deadUnit != attacker)
                {
                    deadUnit.PlayDeathSound(volumeModifier);
                    deadUnit.Die();
                }
            }

            if (activeSkill.m_TeleportSelf)
            {
                OnTeleportUnit?.Invoke(activeSkill.TeleportTargetGrid(attacker), activeSkill.TeleportStartTile(attacker, targetTile), teleportTile);
            }

            // Note: Attacker is killed after targets to ensure attacker's side still gets priority at victory
            if (attacker.IsDead)
            {
                attacker.PlayDeathSound(volumeModifier);
                attacker.Die();
            }

            completeSkillEvent?.Invoke(canExtendTurn);
        }
    }

    private void PerformSkill_Shared(
        Unit attacker, 
        ActiveSkillSO activeSkill, 
        CoordPair targetTile, 
        Action<List<IHealth>, bool, HashSet<Unit>> postSkillEvent, 
        Vector3? targetMovePosition)
    {
        List<IHealth> targets = new();
        foreach (CoordPair coordPair in GetInBoundsTargetTiles(activeSkill, targetTile))
        {       
            if (IsTileOccupied(coordPair))
            {
                targets.Add(m_TileData[coordPair.m_Row, coordPair.m_Col].m_CurrUnit);
            }
        }

        attacker.PostSkillEvent += (bool canExtendTurn, HashSet<Unit> additionalDeadUnits) => postSkillEvent(targets, canExtendTurn, additionalDeadUnits);
        attacker.PerformSkill(activeSkill, targets, targetMovePosition);

        if (activeSkill.ContainsSkillType(SkillEffectType.SUMMON))
        {
            SummonUnits(activeSkill.m_Summons);
        }
    }

    public void SummonUnits(List<SummonWrapper> summonWrappers)
    {
        foreach (SummonWrapper summon in summonWrappers)
        {
            List<CoordPair> summonPositions = GetSummonPositions(summon.m_PrioritsePositions, summon.m_PrioritisedRows, summon.m_PrioritisedCols, summon.m_Adds.Count);
            for (int i = 0; i < summonPositions.Count; ++i)
            {
                // TODO: Possible animation delay
                BattleManager.Instance.InstantiateEnemyUnit(new() {m_Coordinates = summonPositions[i], m_EnemyCharacterData = summon.m_Adds[i].m_EnemyCharacterSO, m_StatAugments = summon.m_Adds[i].m_StatAugments});
            }
        }
    }

    void TryApplySkillTileEffects(ActiveSkillSO activeSkill, CoordPair initialTargetTile)
    {
        List<CoordPair> targetTiles = GetInBoundsTargetTiles(activeSkill, initialTargetTile);

        if (activeSkill.ContainsSkillType(SkillEffectType.APPLY_TILE_EFFECT))
        {
            TryApplyEffectOnTiles(targetTiles, activeSkill.m_InflictedTileEffect);
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

    #region Tile Effects
    /// <summary>
    /// Applies tile effects on a singular unit
    /// </summary>
    public void TryApplyTileEffectsOnUnit(Unit unit)
    {
        CoordPair position = unit.CurrPosition;
        m_TileData[position.m_Row, position.m_Col].TryApplyEffectOnUnit();
    }

    public void TryApplyEffectOnTiles(List<CoordPair> coordinates, InflictedTileEffect inflictedTileEffect)
    {
        foreach (CoordPair coordPair in coordinates)
        {
            if (m_TileData[coordPair.m_Row, coordPair.m_Col].TryApplyEffect(inflictedTileEffect))
                m_TileVisuals[coordPair.m_Row, coordPair.m_Col].SpawnTileEffects(inflictedTileEffect.TileEffectObjs);
        }
    }
    #endregion

    #region Token Handler
    public void ApplyStatusEffects(List<TargetBundle<StatusEffect>> statusEffects)
    {
        foreach (TargetBundle<StatusEffect> statusEffect in statusEffects)
        {
            foreach ((CoordPair coordinates, GridType gridType) in statusEffect.m_Positions)
            {
                if (gridType != m_GridType)
                    continue;
                Unit unit = GetUnitAtTile(coordinates);
                if (!unit.IsDead)
                    unit.InflictStatus(statusEffect.m_Effect);
            }
        }
    }

    public void InflictTokenBundles(List<TargetBundle<InflictedToken>> inflictedTokens, Unit inflicter)
    {
        foreach (TargetBundle<InflictedToken> inflictedToken in inflictedTokens)
        {
            foreach ((CoordPair coordinates, GridType gridType) in inflictedToken.m_Positions)
            {
                if (gridType != m_GridType)
                    continue;
                Unit unit = GetUnitAtTile(coordinates);
                if (!unit.IsDead)
                    unit.InflictToken(inflictedToken.m_Effect, inflicter);
            }
        }
    }

    public void InflictTileEffectBundles(List<TargetBundle<InflictedTileEffect>> inflictedTileEffects)
    {
        foreach (TargetBundle<InflictedTileEffect> inflictedTileEffect in inflictedTileEffects)
        {
            List<CoordPair> coordinateGroup = new();
            foreach ((CoordPair coordinates, GridType gridType) in inflictedTileEffect.m_Positions)
            {
                if (gridType != m_GridType)
                    continue;
                coordinateGroup.Add(coordinates);
            }
            TryApplyEffectOnTiles(coordinateGroup, inflictedTileEffect.m_Effect);
        }
    }

    public void ApplyPassiveChangeBundles(List<TargetBundle<PassiveChangeBundle>> flatChanges, List<TargetBundle<PassiveChangeBundle>> multChanges)
    {
        Dictionary<Unit, PassiveChangeBundle> passiveChangeAmounts = new();

        foreach (TargetBundle<PassiveChangeBundle> flatChange in flatChanges)
        {
            foreach ((CoordPair coordinates, GridType gridType) in flatChange.m_Positions)
            {
                if (gridType != m_GridType)
                    continue;
                Unit unit = GetUnitAtTile(coordinates);
                if (!passiveChangeAmounts.ContainsKey(unit))
                    passiveChangeAmounts[unit] = new PassiveChangeBundle(0f, 0f);
                passiveChangeAmounts[unit].AddBundle(flatChange.m_Effect);
            }
        }

        foreach (TargetBundle<PassiveChangeBundle> multChange in multChanges)
        {
            foreach ((CoordPair coordinates, GridType gridType) in multChange.m_Positions)
            {
                if (gridType != m_GridType)
                    continue;
                Unit unit = GetUnitAtTile(coordinates);
                if (!passiveChangeAmounts.ContainsKey(unit))
                    passiveChangeAmounts[unit] = new PassiveChangeBundle(0f, 0f);
                float actualHealthChangeAmount = multChange.m_Effect.m_HealthAmount * unit.GetTotalStat(StatType.HEALTH);
                float actualManaChangeAmount = multChange.m_Effect.m_ManaAmount * unit.GetTotalStat(StatType.MANA);
                PassiveChangeBundle actualPassiveChangeAmount = new PassiveChangeBundle(actualHealthChangeAmount, actualManaChangeAmount);
                passiveChangeAmounts[unit].AddBundle(actualPassiveChangeAmount);
            }
        }

        foreach (KeyValuePair<Unit, PassiveChangeBundle> pair in passiveChangeAmounts)
        {
            if (pair.Key.IsDead)
                continue;
            PassiveChangeBundle passiveChangeBundle = pair.Value;
            if (passiveChangeBundle.m_HealthAmount > 0)
            {
                pair.Key.Heal(passiveChangeBundle.m_HealthAmount);
            }
            else if (passiveChangeBundle.m_HealthAmount < 0)
            {
                pair.Key.TakeDamage(Mathf.Abs(passiveChangeBundle.m_HealthAmount));
            }

            pair.Key.AlterMana(passiveChangeBundle.m_ManaAmount);
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
