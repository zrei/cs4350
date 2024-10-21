using UnityEngine;

public enum LevelState 
{
    LOCKED,
    UNLOCKED,
    CLEARED,
}

public class WorldMapNode : MonoBehaviour
{
    [SerializeField] LevelInfo m_LevelInfo;

    // set during initialisation
    private LevelState m_LevelState;
    private bool m_IsCurrent;
    private int m_LevelNumber;

    public void EnterNode()
    {
        // display level UI
        // difference depending on if it's cleared or not (cannot replay level?)
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
}