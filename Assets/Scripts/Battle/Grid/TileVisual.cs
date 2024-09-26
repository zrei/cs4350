using UnityEngine;
using UnityEngine.UI;
using Game.UI;

public enum TileState
{
    SWAPPABLE,
    TRAVERSABLE,
    ATTACKABLE,
    INSPECTABLE,
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

    public GridType GridType { get; private set;}
    public CoordPair Coordinates { get; private set;}

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
        selectable.interactable = false;
        m_CurrState = TileState.NONE;
        ToggleState(m_CurrState);
    }

    public void SetTileState(TileState state)
    {
        m_CurrState = state;
        ToggleState(m_CurrState);
    }
    #endregion

    #region Graphics
    private void ToggleState(TileState state)
    {
        tileImage.color = state switch
        {
            TileState.NONE => new(0, 0, 0, 0),
            TileState.SWAPPABLE => new(0, 0.5f, 0, 1),
            TileState.TRAVERSABLE => new(0.1f, 0.1f, 0.5f, 1),
            TileState.ATTACKABLE => new(0.5f, 0.1f, 0.1f, 1),
            TileState.INSPECTABLE => new(0.5f, 0.5f, 0, 1),
            _ => new(0, 0, 0, 0)
        };
    }

    public void ToggleSwapTarget(bool isTarget)
    {
        if (m_CurrState == TileState.NONE) return;

        tileImage.color = isTarget ? new(0.5f, 1, 0.5f, 1) : new(0, 0.5f, 0, 1);
    }

    public void TogglePath(bool isPartOfPath)
    {
        if (m_CurrState == TileState.NONE) return;

        tileImage.color = isPartOfPath ? new(0, 1, 1, 1) : new(0.1f, 0.1f, 0.5f, 1);
    }

    public void ToggleTarget(bool isTarget)
    {
        if (m_CurrState == TileState.NONE) return;

        tileImage.color = isTarget ? new(1, 0.2f, 0.2f, 1) : new(0.5f, 0.1f, 0.1f, 1);
    }
    #endregion
}
