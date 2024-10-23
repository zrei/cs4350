using System.Collections;
using System.Collections.Generic;
using Game;
using Level;
using UnityEngine;

public class WorldMapManager : MonoBehaviour
{
    [SerializeField] private CharacterToken m_PlayerToken;
    [SerializeField] private List<WorldMapNode> m_LevelNodes;

    // can handle camera here for now

    private const float TELEPORT_TIME = 1f;

    private int m_CurrLevel;

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

    private void MoveToLevel(int newLevelNumber)
    {
        if (newLevelNumber == m_CurrLevel)
            return;

        MoveToLevel(m_CurrLevel, newLevelNumber);
    }

    // literally just teleport them to the level
    private IEnumerator MoveToLevel(int currLevelNumber, int newLevelNumber)
    {
        // do a fade + block inputs
        yield return new WaitForSeconds(0.5f);
        Camera worldMapCamera = CameraManager.Instance.MainCamera;
        // move x and z, dont touch y
        float t = 0f;
        while (t < TELEPORT_TIME)
        {
            t += Time.deltaTime;
            float xLerp = Mathf.Lerp(m_LevelNodes[currLevelNumber].transform.position.x, m_LevelNodes[newLevelNumber].transform.position.x, t / TELEPORT_TIME);
            float zLerp = Mathf.Lerp(m_LevelNodes[currLevelNumber].transform.position.z, m_LevelNodes[newLevelNumber].transform.position.z, t / TELEPORT_TIME);
            worldMapCamera.transform.position = new Vector3(xLerp, m_LevelNodes[currLevelNumber].transform.position.y, zLerp);
            yield return null;
        }
        worldMapCamera.transform.position = m_LevelNodes[newLevelNumber].transform.position;
        // fade character token back and change position
        m_LevelNodes[newLevelNumber].PlacePlayerToken(m_PlayerToken);
        m_CurrLevel = newLevelNumber;
    }
}