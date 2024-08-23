using UnityEngine;
using TMPro;

public enum TileState
{
    TRAVERSABLE,
    ATTACKABLE,
    NONE
}

public class TileVisual : MonoBehaviour
{
    [SerializeField] TextMeshPro m_TraverseText;
    [SerializeField] TextMeshPro m_PathText;

    private TileState m_CurrState = TileState.NONE;

    public void ResetTile()
    {
        m_CurrState = TileState.NONE;
        ToggleState(m_CurrState);
        TogglePath(false);
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
}