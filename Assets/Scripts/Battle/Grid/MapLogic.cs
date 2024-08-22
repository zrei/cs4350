using UnityEngine;
using System.Collections.Generic;

public class MapLogic : MonoBehaviour
{
    [SerializeField] private Transform m_GridParent;

    private TileVisual[,] m_TileVisuals = new TileVisual[MapData.NUM_ROWS, MapData.NUM_COLS];
    
    private TileData[,] m_TileData = new TileData[MapData.NUM_ROWS, MapData.NUM_COLS];

    public void Start()
    {
        InitialiseTileData();
        InitialiseTileVisuals();

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
                Transform tile = row.GetChild(c);
                m_TileVisuals[r, c] = tile.GetComponent<TileVisual>();
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

    private void ResetMap()
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
}