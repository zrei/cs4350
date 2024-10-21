using UnityEngine;

public struct LevelInfo 
{
    public string LevelName;
    public string LevelDescription;

    /*
    stuff about actual level data to load like starting rations?
    */

    [Header("Dialogue")]
    /// <summary>
    /// Dialogue to play upon first reaching the level node
    /// </summary>
    public Dialogue m_PreDialogue;
    /// <summary>
    /// Dialogue to play upon completing the level node
    /// </summary>
    public Dialogue m_PostDialogue;

    public int m_LevelNumber;
}  