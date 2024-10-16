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
    private readonly static Color SwappableTargetColor = new(0.3f, 1, 0.3f, 1);
    //private readonly static Color SwappableColor = new(0.5f, 0.5f, 0.5f, 1);
    //private readonly static Color SwappableTargetColor = new(1, 1, 1, 1);

    private readonly static Color TraversableColor = new(0.2f, 0.2f, 0.7f, 1);
    private readonly static Color TraversablePathColor = new(0.3f, 0.3f, 1, 1);

    private readonly static Color AttackableColor = new(0.7f, 0.2f, 0.2f, 1);
    private readonly static Color AttackableTargetColor = new(1f, 0.3f, 0.3f, 1);

    private readonly static Color InspectableColor = new(0.5f, 0.5f, 0, 1);
    //private readonly static Color InspectableColor = new(0.5f, 0.5f, 0.5f, 1);

    private readonly static Color IconActiveColor = new(0.8f, 0.8f, 0.8f, 0.8f);

    #region Asset References
    [SerializeField]
    private Sprite swapIcon;
    [SerializeField]
    private Sprite attackIcon;
    [SerializeField]
    private Sprite moveIcon;
    #endregion

    [SerializeField]
    private Image outline;
    [SerializeField]
    private Image icon;
    public SelectableBase selectable;

    [SerializeField]
    private ParticleSystem attackForecastParticleEffect;

    private TileState m_CurrState = TileState.NONE;

    private Color CurrentStateDefaultColor => m_CurrState switch
    {
        TileState.NONE => new(0, 0, 0, 0),
        TileState.SWAPPABLE => SwappableColor,
        TileState.TRAVERSABLE => TraversableColor,
        TileState.ATTACKABLE => AttackableColor,
        TileState.INSPECTABLE => InspectableColor,
        _ => new(0, 0, 0, 0)
    };

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
        outline.color = CurrentStateDefaultColor;

        switch (state)
        {
            case TileState.NONE:
                icon.sprite = null;
                icon.color = Color.clear;
                ToggleAttackForecast(false);
                break;
        }
    }

    public void ToggleSwapTarget(bool isTarget)
    {
        outline.color = isTarget
            ? SwappableTargetColor
            : CurrentStateDefaultColor;
        icon.sprite = isTarget ? swapIcon : null;
        icon.color = icon.sprite != null ? IconActiveColor : Color.clear;
    }

    public void TogglePath(bool isPartOfPath)
    {
        outline.color = isPartOfPath
            ? TraversablePathColor
            : CurrentStateDefaultColor;
        icon.sprite = isPartOfPath ? moveIcon : null;
        icon.color = icon.sprite != null ? IconActiveColor : Color.clear;
    }

    public void ToggleTarget(bool isTarget)
    {
        outline.color = isTarget
            ? AttackableTargetColor
            : CurrentStateDefaultColor;
        icon.sprite = isTarget ? attackIcon : null;
        icon.color = icon.sprite != null ? IconActiveColor : Color.clear;
    }

    public void ToggleAttackForecast(bool isTarget)
    {
        if (isTarget)
        {
            attackForecastParticleEffect.Play();
        }
        else
        {
            attackForecastParticleEffect.Stop();
        }
    }
    #endregion
}
