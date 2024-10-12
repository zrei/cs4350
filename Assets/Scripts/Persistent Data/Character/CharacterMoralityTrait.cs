using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterMoralityTrait", menuName = "ScriptableObject/Characters/CharacterMoralityTraitSO")]
public class CharacterMoralityTraitSO : ScriptableObject
{
    public string m_TraitName;
    public List<MoralityThreshold> m_MoralityThresholds;

    public List<InflictedToken> GetInflictedTokens(float currMoralityPercentage)
    {
        foreach (MoralityThreshold moralityThreshold in m_MoralityThresholds)
        {
            if (moralityThreshold.IsThresholdMet(currMoralityPercentage))
                return moralityThreshold.m_Tokens;
        }
        return new();
    }
}

[System.Serializable]
public struct MoralityThreshold
{
    [Tooltip("Expressed as a percentage of the max morality")]
    [Range(0f, 1f)]
    public float m_Threshold;
    [Tooltip("Whether this is satisfied if the current morality is less than or more than the threshold")]
    public bool m_LessThan;

    public List<InflictedToken> m_Tokens;

    public bool IsThresholdMet(float currMoralityPercentage)
    {
        if (m_LessThan && currMoralityPercentage <= m_Threshold)
            return true;
        else if (!m_LessThan && currMoralityPercentage >= m_Threshold)
            return true;
        return false;
    }
}
