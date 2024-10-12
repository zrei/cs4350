using UnityEngine;

[CreateAssetMenu(fileName = "MoralitySettings", menuName = "ScriptableObject/Morality/MoralitySettings")]
public class MoralitySettings : ScriptableObject
{
    public int m_MaxMorality = 100;
    public int m_StartingMorality;
}
