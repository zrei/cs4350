using UnityEngine;

[CreateAssetMenu(fileName="LevelSO", menuName="ScriptableObject/Level/LevelSO")]
public class LevelSO : ScriptableObject
{
    public int m_LevelId;
    public string m_LevelName;
    [TextArea]
    public string m_LevelDescription;
    public float m_TimeLimit;
    public int m_UnitLimit;
    public GameObject m_BiomeObject;
    public Dialogue m_PreDialogue;
    public Dialogue m_PostDialogue;

    public string PreDialogueFlag => "PRE_DIALOGUE_" + m_LevelId;
    public string PostDialogueFlag => "POST_DIALOGUE_" + m_LevelId;
    public string LevelCompleteFlag => "LEVEL_COMPLETE_" + m_LevelId;
}
