using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Groups together classes that belong to a singular group and notes down the unlock conditions
/// </summary>
[CreateAssetMenu(fileName = "PathGroupSO", menuName = "ScriptableObject/Classes/PathGroupSO")]
public class PathGroupSO : ScriptableObject
{
    public string m_PathName;
    public List<PathClass> m_PathClasses;
    public int NumClasses => m_PathClasses.Count;

    public int GetDefaultClassIndex()
    {
        for (int i = 0; i < m_PathClasses.Count; ++i)
        {
            if (m_PathClasses[i].m_IsDefault)
                return i;
        }
        Logger.Log(this.GetType().Name, $"Path group {this.name} has no default class", LogLevel.ERROR);
        return default;
    }

    public PlayerClassSO GetClass(int index)
    {
        return m_PathClasses[index].m_Class;
    }

    public List<bool> GetUnlockedClassIndexes(int characterLevel)
    {
        List<bool> unlockedClassIndexes = new();
        for (int i = 0; i < m_PathClasses.Count; ++i)
        {
            unlockedClassIndexes.Add(m_PathClasses[i].IsUnlocked(characterLevel));
        }
        return unlockedClassIndexes;
    }
}

[System.Serializable]
public struct PathClass
{
    public PlayerClassSO m_Class;
    public bool m_IsDefault;
    [Space]
    public UnlockCondition m_UnlockCondition;

    public bool IsUnlocked(int characterLevel) => m_UnlockCondition.IsSatisfied(characterLevel);
}

[System.Serializable]
public struct UnlockCondition
{
    public bool m_IsLevelLocked;
    [Tooltip("Unit level must be greater than or equal to this level")]
    public int m_LevelLock;
    [Space]
    public bool m_IsMoralityLocked;
    public Threshold m_MoralityThreshold;
    [Space]
    public bool m_IsFlagLocked;
    public List<string> m_Flags;

    public bool IsSatisfied(int characterLevel)
    {
        if (m_IsLevelLocked && characterLevel < m_LevelLock)
            return false;
        
        if (m_IsMoralityLocked && !m_MoralityThreshold.IsSatisfied(MoralityManager.Instance.CurrMoralityPercentage))
            return false;
        
        if (m_IsFlagLocked && !m_Flags.All(x => FlagManager.Instance.GetFlagValue(x)))
            return false;

        return true;
    }
}

[System.Serializable]
public struct Threshold
{
    public bool m_GreaterThan;
    public float m_Threshold;

    public bool IsSatisfied(float value)
    {
        if (m_GreaterThan)
            return value > m_Threshold;
        else
            return value < m_Threshold;
    }
}