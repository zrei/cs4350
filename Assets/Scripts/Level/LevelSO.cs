using UnityEngine;

[CreateAssetMenu(fileName="LevelSO", menuName="ScriptableObject/Level/LevelSO")]
public class LevelSO : ScriptableObject
{
    public float m_TimeLimit;
    public int m_UnitLimit;
    public GameObject m_BiomeObject;
}
