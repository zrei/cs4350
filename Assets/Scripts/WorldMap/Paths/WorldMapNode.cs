using UnityEngine;
using UnityEngine.Splines;

public enum LevelState 
{
    LOCKED,
    UNLOCKED,
    CLEARED,
}

public class WorldMapNode : MonoBehaviour
{
    [SerializeField] LevelSO m_LevelInfo;
    [SerializeField] CutsceneSpawner m_PreCutscene;
    [SerializeField] CutsceneSpawner m_PostCutscene;
    [SerializeField] WorldMapEdge m_WorldMapEdge;
    [SerializeField] WorldMapVisual m_WorldMapVisual;

    // set during initialisation
    private LevelState m_LevelState;
    public LevelState LevelState => m_LevelState;
    private bool m_IsCurrent = false;
    public bool IsCurrent => m_IsCurrent;

    public int LevelNum => m_LevelInfo.m_LevelNum;
    public LevelSO LevelSO => m_LevelInfo;

    public bool HasPreCutscene => m_PreCutscene != null;
    public CutsceneSpawner PreCutscene => m_PreCutscene;
    public bool HasPostCutscene => m_PostCutscene != null;
    public CutsceneSpawner PostCutscene => m_PostCutscene;

    public SplineContainer Spline => m_WorldMapEdge.Spline;
    public Vector3 InitialSplineForwardDirection => m_WorldMapEdge.GetInitialSplineForwardDirection();

    public Vector3 PositioningOffset => m_WorldMapVisual.TokenOffset;

    #region Initialise
    public void Initialise(LevelState initialState, bool isCurrentLevel)
    {
        m_LevelState = initialState;
        if (initialState != LevelState.LOCKED)
            m_WorldMapVisual.Initialise();
        if (initialState == LevelState.CLEARED && !isCurrentLevel)
            m_WorldMapEdge.InstantiatePath(m_WorldMapVisual.NodeRadiusOffset);
        
        ToggleCurrLevel(isCurrentLevel);

        m_WorldMapVisual.OnSelected += OnSelected;
        m_WorldMapVisual.OnDeselected += OnDeselected;
    }

    private void OnDestroy()
    {
        m_WorldMapVisual.OnSelected -= OnSelected;
        m_WorldMapVisual.OnDeselected -= OnDeselected;
    }
    #endregion

    #region Update Graphics
    public void ToggleCurrLevel(bool isCurrent)
    {
        if (m_IsCurrent == isCurrent)
            return;
        
        m_IsCurrent = isCurrent;
        m_WorldMapVisual.ToggleCurrLevel(m_IsCurrent);
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
    #endregion

    #region Path
    public void UnlockNode()
    {
        m_WorldMapVisual.UnlockNode();
    }

    public void UnlockPath(VoidEvent onCompleteInstantiate)
    {
        m_WorldMapEdge.InstantiatePath(m_WorldMapVisual.NodeRadiusOffset, false, onCompleteInstantiate);
    }
    #endregion

    #region Token
    public void PlacePlayerToken(WorldMapPlayerToken characterToken)
    {
        characterToken.transform.position = transform.position + m_WorldMapVisual.TokenOffset;

    }
    #endregion
}