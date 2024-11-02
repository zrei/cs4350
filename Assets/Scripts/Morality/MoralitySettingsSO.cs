using UnityEngine;

[CreateAssetMenu(fileName = "MoralitySettingsSO", menuName = "ScriptableObject/Morality/MoralitySettingsSO")]
public class MoralitySettingsSO : ScriptableObject
{
    [Tooltip("The morality will go from negative of the max morality to the max morality")]
    public int m_MaxMorality = 100;
}
