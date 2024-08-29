using UnityEngine;
using System.Collections.Generic;

public class GridLogic : MonoBehaviour
{
    [SerializeField] private Transform m_GridParent;
    [SerializeField] private GridType m_GridType;

    private TileLogic[,] m_TileVisuals = new TileLogic[MapData.NUM_ROWS, MapData.NUM_COLS];
    
    private TileData[,] m_TileData = new TileData[MapData.NUM_ROWS, MapData.NUM_COLS];

    private const float SPAWN_HEIGHT_OFFSET = 1.0f;

    public MapData MapData => new MapData(m_TileData);

    #region Initialisation
    private void Start()
    {
        InitialiseTileData();
        InitialiseTileVisuals();  
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
                TileLogic tile = tileTrf.GetComponent<TileLogic>();
                tile.Initialise(m_GridType, new CoordPair(r, c));
                m_TileVisuals[r, c] = tile;
                Debug.Log("Initialise tile position: " + tileTrf.position);
            }
        }
    }
    #endregion

    #region Graphics
    public void ColorMap(HashSet<PathNode> reachablePoints)
    {
        ResetPath();
        foreach (PathNode pathNode in reachablePoints)
        {
            CoordPair coordinates = pathNode.m_Coordinates;
            m_TileVisuals[coordinates.m_Row, coordinates.m_Col].SetTileState(TileState.TRAVERSABLE);
        }
    }

    public void ResetMap()
    {
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

    public void SetTarget(List<CoordPair> targetSquares)
    {
        ResetTarget();
        foreach (CoordPair coordPair in targetSquares)
        {
            m_TileVisuals[coordPair.m_Row, coordPair.m_Col].ToggleTarget(true);
        }
    }
    #endregion

    #region Path
    public Stack<Vector3> TracePath(PathNode end, bool showGraphic = true)
    {
        if (showGraphic)
            ResetPath();
        
        Stack<Vector3> pathPoints = new Stack<Vector3>();
        PathNode pointer = end;
        while (pointer != null)
        {
            pathPoints.Push(GetTilePosition(pointer.m_Coordinates));
            CoordPair coordinates = pointer.m_Coordinates;
            if (showGraphic)
                m_TileVisuals[coordinates.m_Row, coordinates.m_Col].TogglePath(true);
            pointer = pointer.m_Parent;
        }
        return pathPoints;
    }

    public HashSet<PathNode> CalculateReachablePoints(Unit unit)
    {
        return Pathfinder.ReachablePoints(new MapData(m_TileData), unit.CurrPosition, unit.Stat.m_MovementRange, unit.Stat.m_CanSwapTiles, unit.Stat.m_TraversableTileTypes);
    }
    #endregion

    #region Unit
    public Unit PlaceUnit(UnitPlacement unitPlacement)
    {
        // probably needs some... actual rotation? haha. if it's already rotated i guess not
        Unit spawnedUnit = Instantiate(unitPlacement.m_Unit, GetTilePosition(unitPlacement.m_Coodinates), Quaternion.identity);
        Logger.Log(this.GetType().Name, spawnedUnit.gameObject.name, "Spawned unit position: " + spawnedUnit.transform.position, spawnedUnit.gameObject, LogLevel.LOG);
        m_TileData[unitPlacement.m_Coodinates.m_Row, unitPlacement.m_Coodinates.m_Col].m_IsOccupied = true;
        m_TileData[unitPlacement.m_Coodinates.m_Row, unitPlacement.m_Coodinates.m_Col].m_CurrUnit = spawnedUnit;
        return spawnedUnit;
    }

    public void MoveUnit(Unit unit, CoordPair end, PathNode endPathNode, VoidEvent onCompleteMovement)
    {
        CoordPair start = unit.CurrPosition;

        if (m_TileData[start.m_Row, start.m_Col].m_CurrUnit != unit)
        {
            Logger.Log(this.GetType().Name, "Unit stored in the tile data is not the same as unit to be moved", LogLevel.ERROR);
        }

        Stack<Vector3> movementCheckpoints = TracePath(endPathNode, false);
        
        m_TileData[start.m_Row, start.m_Col].m_CurrUnit = m_TileData[end.m_Row, end.m_Col].m_CurrUnit;
        m_TileData[start.m_Row, start.m_Col].m_IsOccupied = m_TileData[start.m_Row, start.m_Col].m_CurrUnit != null;
        m_TileData[end.m_Row, end.m_Col].m_IsOccupied = true;
        m_TileData[end.m_Row, end.m_Col].m_CurrUnit = unit;

        unit.Move(end, movementCheckpoints, onCompleteMovement);
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
    public void Attack(List<CoordPair> attackPoints, float damage)
    {
        foreach (CoordPair coordPair in attackPoints)
        {
            if (m_TileData[coordPair.m_Row, coordPair.m_Col].m_IsOccupied)
            {
                m_TileData[coordPair.m_Row, coordPair.m_Col].m_CurrUnit.TakeDamage(damage);
            }
        }

        foreach (CoordPair coordPair in attackPoints)
        {
            if (m_TileData[coordPair.m_Row, coordPair.m_Col].m_IsOccupied && m_TileData[coordPair.m_Row, coordPair.m_Col].m_CurrUnit.IsDead)
            {
                Unit unit = m_TileData[coordPair.m_Row, coordPair.m_Col].m_CurrUnit;
                GlobalEvents.Battle.UnitDefeatedEvent?.Invoke(unit);
                Destroy(unit.gameObject);
                m_TileData[coordPair.m_Row, coordPair.m_Col].m_CurrUnit = null;
            }
        }
    }
    #endregion
}