using UnityEngine;
using TMPro;

public enum TileState
{
    TRAVERSABLE,
    ATTACKABLE,
    NONE
}

[RequireComponent(typeof(Collider))]
public class TileLogic : MonoBehaviour
{
    [SerializeField] TextMeshPro m_TraverseText;
    [SerializeField] TextMeshPro m_PathText;
    [SerializeField] TextMeshPro m_TargetText;

    private TileState m_CurrState = TileState.NONE;

    public GridType GridType {get; private set;}
    public Unit ContainedUnit {get; private set;}
    public CoordPair Coordinates {get; private set;}

    public void Initialise(GridType gridType, CoordPair coordinates)
    {
        GridType = gridType;
        Coordinates = coordinates;
    }

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

    private void ToggleState(TileState state)
    {
        m_TraverseText.text = state switch
        {
            TileState.NONE => string.Empty,
            TileState.TRAVERSABLE => "Traverse",
            TileState.ATTACKABLE => "Attack",
            _ => "Error"
        };
    }

    public void TogglePath(bool isPartOfPath)
    {
        m_PathText.gameObject.SetActive(isPartOfPath);
    }

    public void ToggleTarget(bool isTarget)
    {
        m_TargetText.gameObject.SetActive(isTarget);
    }

    public void SetUnit(Unit unit)
    {
        ContainedUnit = unit;
    }
}