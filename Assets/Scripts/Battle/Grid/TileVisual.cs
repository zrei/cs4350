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
    private readonly static Color SwappableColor = new(0.2f, 0.7f, 0.2f, 1);
    private readonly static Color SwappableTargetColor = new(1, 1, 1, 1);

    private readonly static Color TraversableColor = new(0.2f, 0.2f, 0.7f, 1);
    private readonly static Color TraversablePathColor = new(0.3f, 0.3f, 1, 1);

    private readonly static Color AttackableColor = new(0.7f, 0.2f, 0.2f, 1);
    private readonly static Color AttackableTargetColor = new(1f, 0.3f, 0.3f, 1);

    private readonly static Color InspectableColor = new(0.5f, 0.5f, 0, 1);

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
            TileState.SWAPPABLE => SwappableColor,
            TileState.TRAVERSABLE => TraversableColor,
            TileState.ATTACKABLE => AttackableColor,
            TileState.INSPECTABLE => InspectableColor,
            _ => new(0, 0, 0, 0)
        };
    }

    public void ToggleSwapTarget(bool isTarget)
    {
        if (m_CurrState == TileState.NONE) return;

        tileImage.color = isTarget
            ? SwappableTargetColor
            : SwappableColor;
    }

    public void TogglePath(bool isPartOfPath)
    {
        if (m_CurrState == TileState.NONE) return;

        tileImage.color = isPartOfPath
            ? TraversablePathColor
            : TraversableColor;
    }

    public void ToggleTarget(bool isTarget)
    {
        if (m_CurrState == TileState.NONE) return;

        tileImage.color = isTarget
            ? AttackableTargetColor
            : AttackableColor;
    }
    #endregion
}
