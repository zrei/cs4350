using UnityEngine;

[CreateAssetMenu(fileName = "MoralitySettingsSO", menuName = "ScriptableObject/Morality/MoralitySettingsSO")]
public class MoralitySettingsSO : ScriptableObject
{
    [Tooltip("The morality will go from negative of the max morality to the max morality")]
    public int m_MaxMorality = 100;
    [Tooltip("Percentage of morality to start from")]
    [Range(-1f, 1f)]
    public float m_StartingMoralityPercentage;
}
