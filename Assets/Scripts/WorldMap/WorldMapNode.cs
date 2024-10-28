using UnityEngine;

public enum LevelState 
{
    LOCKED,
    UNLOCKED,
    CLEARED,
}

public class WorldMapNode : MonoBehaviour
{
    [SerializeField] LevelSO m_LevelInfo;
    [SerializeField] Transform m_CharacterPosition;
    [SerializeField] WorldMapEdge m_WorldMapEdge;
    [SerializeField] WorldMapVisual m_WorldMapVisual;

    // set during initialisation
    private LevelState m_LevelState;
    private bool m_IsCurrent = false;
    public bool IsCurrent => m_IsCurrent;

    public void Initialise(LevelState initialState, bool isCurrentLevel)
    {
        m_LevelState = initialState;
        m_IsCurrent = isCurrentLevel;
        if (initialState != LevelState.LOCKED)
            m_WorldMapVisual.Initialise();
        if (initialState == LevelState.CLEARED && !isCurrentLevel)
            m_WorldMapEdge.InstantiatePath(m_WorldMapVisual.NodeRadiusOffset);
        
        if (isCurrentLevel)
            m_WorldMapVisual.ToggleCurrLevel(true);

        m_WorldMapVisual.OnSelected += OnSelected;
        m_WorldMapVisual.OnDeselected += OnDeselected;
    }

    private void OnSelected()
    {
        if (!m_IsCurrent)
            m_WorldMapVisual.ToggleSelected(true);
    }

    private void OnDeselected()
    {
        m_WorldMapVisual.ToggleSelected(false);
    }

    // on... click
    public void OnGoToLevel()
    {
        // do stuff depending on state
    }
    // move through selection or just next and previous?

    public void PlacePlayerToken(WorldMapPlayerToken characterToken)
    {
        characterToken.transform.position = m_CharacterPosition.transform.position;
    }
}