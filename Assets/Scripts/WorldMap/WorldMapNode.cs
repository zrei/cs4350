using Level;
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
    [SerializeField] LevelInfo m_LevelInfo;
    [SerializeField] Transform m_CharacterPosition;
    [SerializeField] Spline m_Spline;

    // set during initialisation
    private LevelState m_LevelState;
    private bool m_IsCurrent;
    private int m_LevelNumber;

    public void Initialise(LevelState initialState, bool isCurrentLevel)
    {
        m_LevelState = initialState;
        m_IsCurrent = isCurrentLevel;
    }

    public void EnterNode()
    {
        // display level UI
        // difference depending on if it's cleared or not (cannot replay level?)
        // expand node
    }

    public void ExitNode()
    {
        // exit level UI
    }

    // on... click
    public void OnLevelSelected()
    {
        // do stuff depending on state
    }
    // move through selection or just next and previous?

    public void PlacePlayerToken(CharacterToken characterToken)
    {
        characterToken.transform.position = m_CharacterPosition.transform.position;
    }

    public Vector3 GetPathPosition(float timeProportion)
    {
        return m_Spline.EvaluatePosition(timeProportion);
    }
}