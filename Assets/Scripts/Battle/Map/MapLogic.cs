using System.Collections.Generic;
using UnityEngine;

public enum GridType
{
    ENEMY,
    PLAYER
}

/// <summary>
/// Acts as a facade to the GridLogic of both sides
/// </summary>
public class MapLogic : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private GridLogic m_PlayerGrid;
    [SerializeField] private GridLogic m_EnemyGrid;

    #region Units
    public void PlaceUnit(GridType gridType, Unit unit, CoordPair coord)
    {
        RetrieveGrid(gridType).PlaceUnit(unit, coord);
    }

    public void MoveUnit(GridType gridType, Unit unit, PathNode endPathNode, VoidEvent onCompleteMovement)
    {
        RetrieveGrid(gridType).MoveUnit(unit, endPathNode, onCompleteMovement);
    }

    public void SwapTiles(GridType gridType, CoordPair tile1, CoordPair tile2)
    {
        RetrieveGrid(gridType).SwapTiles(tile1, tile2);
    }
    #endregion

    /*public void MoveUnit(GridType gridType, Unit unit, CoordPair start, CoordPair end)
    {
        RetrieveGrid(gridType).MoveUnit(unit, start, end);
    }*/

    #region Graphics
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

    public void ColorMap(GridType gridType, HashSet<PathNode> reachableNodes)
    {
        RetrieveGrid(gridType).ColorReachablePoints(reachableNodes);
    }

    public void ColorPath(GridType gridType, PathNode end)
    {
        RetrieveGrid(gridType).ColorPath(end);
    }

    public void SetTarget(GridType gridType, ActiveSkillSO attack, CoordPair target)
    {
        RetrieveGrid(gridType).ColorTarget(attack, target);
    }
    #endregion

    #region Helper
    public bool IsTileOccupied(GridType gridType, CoordPair tile)
    {
        return RetrieveGrid(gridType).IsTileOccupied(tile);
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
            TileVisual tile = raycastHit.collider.gameObject.GetComponent<TileVisual>();
            
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

    private GridLogic RetrieveGrid(GridType gridType)
    {
        return gridType switch
        {
            GridType.ENEMY => m_EnemyGrid,
            GridType.PLAYER => m_PlayerGrid,
            _ => null
        };
    }
    #endregion

    #region Attacks
    public void PerformSkill(GridType gridType, Unit attacker, ActiveSkillSO attack, CoordPair targetTile, VoidEvent completeSkillEvent)
    {
        RetrieveGrid(gridType).PerformSkill(attacker, attack, targetTile, completeSkillEvent);
    }
    #endregion

    #region Unit Movement
    public HashSet<PathNode> CalculateReachablePoints(GridType gridType, Unit unit, int remainingMovementRange)
    {
        return RetrieveGrid(gridType).CalculateReachablePoints(unit, remainingMovementRange);
    }
    #endregion
}