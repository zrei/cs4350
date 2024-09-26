using UnityEngine;
using UnityEngine.UI;
using Game.UI;

public enum TileState
{
    TRAVERSABLE,
    ATTACKABLE,
    NONE
}

/// <summary>
/// Manages the visuals of the tile, e.g. highlighting the tile
/// </summary>
public class TileVisual : MonoBehaviour
{
    [SerializeField] RawImage tileImage;
    public SelectableBase selectable;

    private TileState m_CurrState = TileState.NONE;

    public GridType GridType {get; private set;}
    public Unit ContainedUnit {get; private set;}
    public CoordPair Coordinates {get; private set;}

    #region Initialisation
    public void Initialise(GridType gridType, CoordPair coordinates)
    {
        GridType = gridType;
        Coordinates = coordinates;
    }
    #endregion

    #region State
    public void ResetTile()
    {
        m_CurrState = TileState.NONE;
        ToggleState(m_CurrState);
        TogglePath(false);
        ToggleTarget(false);
    }

    public void SetTileState(TileState state)
    {
        m_CurrState = state;
        ToggleState(m_CurrState);
    }

    public void SetUnit(Unit unit)
    {
        ContainedUnit = unit;
    }
    #endregion

    #region Graphics
    private void ToggleState(TileState state)
    {
        tileImage.color = state switch
        {
            TileState.NONE => new(0, 0, 0, 0),
            TileState.TRAVERSABLE => new(0, 0.75f, 1, 1),
            TileState.ATTACKABLE => new(1, 0.125f, 0, 1),
            _ => new(0, 0, 0, 0)
        };
    }

    public void TogglePath(bool isPartOfPath)
    {
        selectable.interactable = m_CurrState == TileState.TRAVERSABLE && isPartOfPath;
    }

    public void ToggleTarget(bool isTarget)
    {
        selectable.interactable = m_CurrState == TileState.ATTACKABLE && isTarget;
    }
    #endregion
}
