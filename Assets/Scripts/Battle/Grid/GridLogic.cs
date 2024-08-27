using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.Tilemaps;

public class GridLogic : MonoBehaviour
{
    [SerializeField] private Transform m_GridParent;
    [SerializeField] private GridType m_GridType;

    private TileLogic[,] m_TileVisuals = new TileLogic[MapData.NUM_ROWS, MapData.NUM_COLS];
    
    private TileData[,] m_TileData = new TileData[MapData.NUM_ROWS, MapData.NUM_COLS];

    private const float SPAWN_HEIGHT_OFFSET = 1.0f;

    public MapData MapData => new MapData(m_TileData);

    private void Start()
    {
        InitialiseTileData();
        InitialiseTileVisuals();

        /*PathNode ptr = null;
        foreach (PathNode node in reachablePoints)
        {
            ptr = node;
        }

        if (ptr != null)
            TracePath(ptr);*/
        
    }

    private void InitialiseTileData()
    {   
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                m_TileData[r, c] = new TileData(TileType.NORMAL, (r + c) % 2 == 0);
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

    public Stack<Vector3> TracePath(PathNode end)
    {
        ResetPath();
        Stack<Vector3> pathPoints = new Stack<Vector3>();
        Debug.Log("End of path! Start!");
        PathNode pointer = end;
        while (pointer != null)
        {
            Debug.Log(pointer.m_Coordinates);
            pathPoints.Push(GetTilePosition(pointer.m_Coordinates));
            CoordPair coordinates = pointer.m_Coordinates;
            m_TileVisuals[coordinates.m_Row, coordinates.m_Col].TogglePath(true);
            pointer = pointer.m_Parent;
        }
        Debug.Log("Finish path!");
        return pathPoints;
    }

    public Unit PlaceUnit(UnitPlacement unitPlacement)
    {
        // probably needs some... actual rotation? haha. if it's already rotated i guess not
        Unit spawnedUnit = Instantiate(unitPlacement.m_Unit, GetTilePosition(unitPlacement.m_Coodinates), Quaternion.identity);
        Logger.Log(this.GetType().Name, spawnedUnit.gameObject.name, "Spawned unit position: " + spawnedUnit.transform.position, spawnedUnit.gameObject, LogLevel.LOG);
        m_TileData[unitPlacement.m_Coodinates.m_Row, unitPlacement.m_Coodinates.m_Col].m_IsOccupied = true;
        return spawnedUnit;
    }

    public void MoveUnit(CoordPair start, CoordPair end)
    {
        m_TileData[start.m_Row, start.m_Col].m_IsOccupied = false;
        m_TileData[end.m_Row, end.m_Col].m_IsOccupied = true;
    }

    private Vector3 GetTilePosition(CoordPair coordPair)
    {
        return m_TileVisuals[coordPair.m_Row, coordPair.m_Col].transform.position + new Vector3(0f, SPAWN_HEIGHT_OFFSET, 0f);
    }

    public void SetTarget(List<CoordPair> targetSquares)
    {
        ResetTarget();
        foreach (CoordPair coordPair in targetSquares)
        {
            m_TileVisuals[coordPair.m_Row, coordPair.m_Col].ToggleTarget(true);
        }
    }
}