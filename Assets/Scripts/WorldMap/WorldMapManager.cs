using System.Collections;
using System.Collections.Generic;
using Level;
using UnityEngine;

public class WorldMapManager : MonoBehaviour
{
    [SerializeField] private CharacterToken m_PlayerToken;
    [SerializeField] private List<WorldMapNode> m_LevelNodes;

    // can handle camera here for now

    public WorldMapNode GetLevel(int levelNumber)
    {
        return m_LevelNodes[levelNumber - 1];
    }

    private void Awake()
    {
        if (FlagManager.Instance.GetFlagValue(Flags.WIN_LEVEL_FLAG))
        {
            // proceed to the next level
        }
        else
        {
            // do nothing, the level is not completed
        }

        /*
        // reset flags
        FlagManager.Instance.SetFlagValue(Flags.WIN_LEVEL_FLAG, FlagType.SESSION, false);
        FlagManager.Instance.SetFlagValue(Flags.LOSE_LEVEL_FLAG, FlagType.SESSION, false);
        */
    }

    private IEnumerator MoveToLevel(int currLevelNumber, int newLevelNumber)
    {
        // move between the positions... quickly... exit node
        while (currLevelNumber != newLevelNumber)
        {
            ++currLevelNumber;
            Vector3 position = m_LevelNodes[currLevelNumber].transform.position;
            // move towards
        }
        // enter node
        yield return null;
    }
}