using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GridLogic : MonoBehaviour
{
    [SerializeField] private Transform m_GridParent;
    [SerializeField] private GridType m_GridType;

    private TileLogic[,] m_TileVisuals = new TileLogic[MapData.NUM_ROWS, MapData.NUM_COLS];
    
    private TileData[,] m_TileData = new TileData[MapData.NUM_ROWS, MapData.NUM_COLS];

    private const float SPAWN_HEIGHT_OFFSET = 1.0f;
    private const float CHECKPOINT_MOVE_TIME = 0.5f;

    private void Start()
    {
        InitialiseTileData();
        InitialiseTileVisuals();

        /*
        // TODO: THis is all test code
        TileType[] traversableTiles = new TileType[1];
        traversableTiles[0] = TileType.NORMAL;
        HashSet<PathNode> reachablePoints = Pathfinder.ReachablePoints(new MapData(m_TileData), new CoordPair(0, 0), 3, true, traversableTiles);
        ColorMap(reachablePoints);

        PathNode ptr = null;
        foreach (PathNode node in reachablePoints)
        {
            ptr = node;
        }

        if (ptr != null)
            TracePath(ptr);
        */
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

    private void ColorMap(HashSet<PathNode> reachablePoints)
    {
        ResetMap();
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

    private void TracePath(PathNode end)
    {
        Debug.Log("End of path! Start!");
        PathNode pointer = end;
        while (pointer != null)
        {
            Debug.Log(pointer.m_Coordinates);
            CoordPair coordinates = pointer.m_Coordinates;
            m_TileVisuals[coordinates.m_Row, coordinates.m_Col].TogglePath(true);
            pointer = pointer.m_Parent;
        }
        Debug.Log("Finish path!");
    }

    public Unit PlaceUnit(UnitPlacement unitPlacement)
    {
        // probably needs some... actual rotation? haha. if it's already rotated i guess not
        Unit spawnedUnit = Instantiate(unitPlacement.m_Unit, GetTilePosition(unitPlacement.m_Coodinates), Quaternion.identity);
        Logger.Log(this.GetType().Name, spawnedUnit.gameObject.name, "Spawned unit position: " + spawnedUnit.transform.position, spawnedUnit.gameObject, LogLevel.LOG);
        return spawnedUnit;
    }

    public void MoveUnit(Unit unit, CoordPair start, CoordPair end)
    {
        unit.transform.position = GetTilePosition(end);
        //List<Vector3> checkpointPositions = new List<Vector3>();
        //StartCoroutine(MoveUnitThroughCheckpoints(unit, checkpointPositions));
    }

    private Vector3 GetTilePosition(CoordPair coordPair)
    {
        return m_TileVisuals[coordPair.m_Row, coordPair.m_Col].transform.position + new Vector3(0f, SPAWN_HEIGHT_OFFSET, 0f);
    }

    private IEnumerator MoveUnitThroughCheckpoints(Unit unit, List<Vector3> positionsToMoveThrough)
    {
        for (int i = 0; i < positionsToMoveThrough.Count; ++i)
        {
            // rotate unit
            // unit.gameObject.transform.LookAt()
            yield return null;
        }
    }
}