using System.Collections.Generic;
using UnityEngine;

public enum GridType
{
    ENEMY,
    PLAYER
}

// manage both grids and pass function calls down through a facade
public class MapLogic : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private GridLogic m_PlayerGrid;
    [SerializeField] private GridLogic m_EnemyGrid;

    public Unit PlaceUnit(GridType gridType, UnitPlacement unit)
    {
        return RetrieveGrid(gridType).PlaceUnit(unit);
    }

    /*public void MoveUnit(GridType gridType, Unit unit, CoordPair start, CoordPair end)
    {
        RetrieveGrid(gridType).MoveUnit(unit, start, end);
    }*/

    public void ResetMap()
    {
        m_PlayerGrid.ResetMap();
        m_EnemyGrid.ResetMap();
    }

    public void ResetPath()
    {
        m_PlayerGrid.ResetPath();
        m_EnemyGrid.ResetPath();
    }

    public void ResetTarget()
    {
        m_PlayerGrid.ResetTarget();
        m_EnemyGrid.ResetTarget();
    }

    private GridLogic RetrieveGrid(GridType gridType)
    {
        return gridType switch
        {
            GridType.ENEMY => m_EnemyGrid,
            GridType.PLAYER => m_PlayerGrid,
            _ => null
        };
    }

    /// <summary>
    /// Given a ray, tries to retrieve a tile that the ray connects with. Will also return
    /// the grid type of the hit ray, helpful for attacks.
    /// Tiles must have a collider and be on the layer "Grid"
    /// </summary>
    /// <param name="ray"></param>
    /// <param name="coordPair"></param>
    /// <param name="gridType"></param>
    /// <returns></returns>
    public bool TryRetrieveTile(Ray ray, out CoordPair coordPair, out GridType gridType)
    {
        RaycastHit[] raycastHits = Physics.RaycastAll(ray, Mathf.Infinity, LayerMask.GetMask("Grid"));
        //Debug.DrawRay(ray.origin, ray.direction * 100, Color.white, 100f, false); 
        foreach (RaycastHit raycastHit in raycastHits)
        {
            TileLogic tile = raycastHit.collider.gameObject.GetComponent<TileLogic>();
            
            if (!tile)
                continue;

            coordPair = tile.Coordinates;
            gridType = tile.GridType;
            return true;
        }
        coordPair = default;
        gridType = default;
        
        return false;
    }

    public void ColorMap(GridType gridType, HashSet<PathNode> reachableNodes)
    {
        RetrieveGrid(gridType).ColorMap(reachableNodes);
    }

    public MapData RetrieveMapData(GridType gridType)
    {
        return RetrieveGrid(gridType).MapData;
    }

    public Stack<Vector3> TracePath(GridType gridType, PathNode end)
    {
        return RetrieveGrid(gridType).TracePath(end);
    }

    public void SetTarget(GridType gridType, List<CoordPair> targets)
    {
        RetrieveGrid(gridType).SetTarget(targets);
    }
}