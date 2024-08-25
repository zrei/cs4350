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

    public void MoveUnit(GridType gridType, Unit unit, CoordPair start, CoordPair end)
    {
        RetrieveGrid(gridType).MoveUnit(unit, start, end);
    }

    public void ResetMap()
    {
        m_PlayerGrid.ResetMap();
        m_EnemyGrid.ResetMap();
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

    public bool RetrieveClickedTile(Ray ray, out CoordPair coordPair, out GridType gridType)
    {
        //Debug.Log("World position: " + worldPosition);
        RaycastHit[] raycastHits = Physics.RaycastAll(ray, Mathf.Infinity, LayerMask.GetMask("Grid")); //, Camera.main.transform.forward, Mathf.Infinity, LayerMask.GetMask("Grid"));
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.white, 100f, false); 
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
}