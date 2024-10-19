using Game;
using Game.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum GridType
{
    ENEMY,
    PLAYER
}

public delegate void MapInputEvent(TileData data, TileVisual visual);

/// <summary>
/// Acts as a facade to the GridLogic of both sides
/// </summary>
public class MapLogic : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private GridLogic m_PlayerGrid;
    [SerializeField] private GridLogic m_EnemyGrid;

    public event MapInputEvent onTileSelect;
    public event MapInputEvent onTileSubmit;

    private void Start()
    {
        var canvas = GetComponent<Canvas>();
        canvas.worldCamera = CameraManager.Instance.MainCamera;

        m_PlayerGrid.onTileSelect += OnTileSelect;
        m_PlayerGrid.onTileSubmit += OnTileSubmit;
        m_EnemyGrid.onTileSelect += OnTileSelect;
        m_EnemyGrid.onTileSubmit += OnTileSubmit;
    }

    private void OnTileSelect(TileData data, TileVisual visual)
    {
        onTileSelect?.Invoke(data, visual);
    }

    private void OnTileSubmit(TileData data, TileVisual visual)
    {
        onTileSubmit?.Invoke(data, visual);
    }

    public void SetGridInteractable(GridType gridType, bool interactable)
    {
        RetrieveGrid(gridType).SetInteractable(interactable);
    }

    public void SetGridInteractableWhere(GridType gridType, bool interactable, Func<TileVisual, bool> condition)
    {
        RetrieveGrid(gridType).SetInteractableWhere(interactable, condition);
    }

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

    public void RemoveUnit(GridType gridType, Unit unit)
    {
        RetrieveGrid(gridType).RemoveUnit(unit);
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

    public void ShowAttackable(GridType gridType, Unit currentUnit, ActiveSkillSO skill)
    {
        RetrieveGrid(gridType).ShowAttackRange(currentUnit, skill);
    }

    public void ShowTeleportable(GridType gridType, Unit currentUnit, ActiveSkillSO skill, CoordPair initialTarget)
    {
        RetrieveGrid(gridType).ShowTeleportRange(skill, currentUnit, initialTarget);
    }

    public void SetTarget(GridType gridType, ActiveSkillSO attack, CoordPair target)
    {
        RetrieveGrid(gridType).ColorTarget(attack, target);
    }

    public void SetTeleportTarget(GridType gridType, CoordPair target, CoordPair initialTargetTile)
    {
        RetrieveGrid(gridType).ColorTeleportTarget(target, initialTargetTile);
    }

    public void ShowInspectable(GridType gridType, bool ignoreEmpty=false)
    {
        RetrieveGrid(gridType).ShowInspectable(ignoreEmpty);
    }

    public void ShowSetupTiles(GridType gridType, IEnumerable<CoordPair> validTiles)
    {
        RetrieveGrid(gridType).ShowSetupTiles(validTiles);
    }

    public void ShowAttackForecast(GridType gridType, IEnumerable<CoordPair> validTiles)
    {
        RetrieveGrid(gridType).ShowAttackForecast(validTiles);
    }
    #endregion

    #region Helper
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

    public int GetNumberOfUnitsOnGrid(GridType gridType)
    {
        return RetrieveGrid(gridType).GetNumberOfUnitsOnGrid();
    }

    public Unit GetUnitAtTile(GridType gridType, CoordPair coordPair)
    {
        return RetrieveGrid(gridType).GetUnitAtTile(coordPair);
    }

    public IEnumerable<CoordPair> GetUnoccupiedTiles(GridType gridType)
    {
        return RetrieveGrid(gridType).GetUnoccupiedTiles();
    }
    #endregion

    #region Attacks
    public void PerformSkill(GridType gridType, Unit attacker, ActiveSkillSO attack, CoordPair targetTile, VoidEvent completeSkillEvent)
    {
        RetrieveGrid(gridType).PerformSkill(attacker, attack, targetTile, completeSkillEvent);
    }

    public void PerformTeleportSkill(GridType gridType, Unit attacker, ActiveSkillSO attack, CoordPair targetTile, CoordPair teleportTargetTile, VoidEvent completeSkillEvent)
    {
        RetrieveGrid(gridType).PerformTeleportSkill(attacker, attack, targetTile, teleportTargetTile, completeSkillEvent);
    }

    public bool IsValidSkillTargetTile(ActiveSkillSO activeSkillSO, Unit unit, CoordPair targetTile, GridType gridType, bool checkOccupied = false)
    {
        return RetrieveGrid(gridType).IsValidSkillTargetTile(activeSkillSO, unit, targetTile, checkOccupied);
    }

    public bool IsValidTeleportTile(ActiveSkillSO activeSkillSO, Unit unit, CoordPair targetTile, GridType gridType)
    {
        return RetrieveGrid(gridType).IsValidTeleportTile(activeSkillSO, unit, targetTile);
    }
    #endregion

    #region Unit Movement
    public HashSet<PathNode> CalculateReachablePoints(GridType gridType, Unit unit, int remainingMovementRange)
    {
        return RetrieveGrid(gridType).CalculateReachablePoints(unit, remainingMovementRange);
    }

    public void TryReachTile(GridType gridType, Unit unit, CoordPair destination, VoidEvent onCompleteMovement)
    {
        RetrieveGrid(gridType).TryReachTile(unit, destination, onCompleteMovement);
    }
    #endregion
}