using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterMoralityTraitSO", menuName = "ScriptableObject/Characters/CharacterMoralityTraitSO")]
public class CharacterMoralityTraitSO : ScriptableObject
{
    public string m_TraitName;

    [Header("Thresholds - Remember to sort after editing!")]
    [Tooltip("List of all thresholds that, once morality falls below, will have an effect.")]
    public List<MoralityThreshold> m_LessThanMoralityThresholds;
    [Tooltip("List of all thresholds that, once morality rises above, will have an effect.")]
    public List<MoralityThreshold> m_GreaterThanMoralityThresholds;

    /// <summary>
    /// Gets all permanent tokens to be inflicted based on the current morality percentage.
    /// Assumption is that there are no conflicting morality thresholds in the data.
    /// </summary>
    /// <param name="currMoralityPercentage"></param>
    /// <returns></returns>
    public IEnumerable<InflictedToken> GetInflictedTokens(float currMoralityPercentage)
    {
        foreach (MoralityThreshold moralityThreshold in m_LessThanMoralityThresholds)
        {
            if (moralityThreshold.IsThresholdMet(currMoralityPercentage, true))
                return moralityThreshold.m_Tokens;
        }
        foreach (MoralityThreshold moralityThreshold in m_GreaterThanMoralityThresholds)
        {
            if (moralityThreshold.IsThresholdMet(currMoralityPercentage, false))
                return moralityThreshold.m_Tokens;
        }
        return new List<InflictedToken>();
    }

#if UNITY_EDITOR
    public void SortThresholds()
    {
        m_LessThanMoralityThresholds.Sort((x, y) => x.m_Threshold.CompareTo(y.m_Threshold));
        m_GreaterThanMoralityThresholds.Sort((x, y) => y.m_Threshold.CompareTo(x.m_Threshold));
        EditorUtility.SetDirty(this);
    }
#endif
}

[System.Serializable]
public struct MoralityThreshold
{
    [Tooltip("Expressed as a percentage of the max morality")]
    [Range(-1f, 1f)]
    public float m_Threshold;

    public List<InflictedToken> m_Tokens;

    /// <summary>
    /// Checks whether the threshold has been met
    /// </summary>
    /// <param name="currMoralityPercentage"></param>
    /// <param name="lessThan">Whether to check for less than the thresold or greater than the threshold</param>
    /// <returns></returns>
    public bool IsThresholdMet(float currMoralityPercentage, bool lessThan)
    {
        if (lessThan && currMoralityPercentage <= m_Threshold)
            return true;
        else if (!lessThan && currMoralityPercentage >= m_Threshold)
            return true;
        return false;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CharacterMoralityTraitSO))]
public class CharacterMoralityTraitSOHelper : Editor
{
    private CharacterMoralityTraitSO m_CharacterMoralityTraitSO;

    private void OnEnable()
    {
        m_CharacterMoralityTraitSO = (CharacterMoralityTraitSO) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(30f);

        if (GUILayout.Button("Sort thresholds"))
        {
            m_CharacterMoralityTraitSO.SortThresholds();
        }
    }
}
#endif
