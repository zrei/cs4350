using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevellingSO", menuName = "ScriptableObject/LevellingSO")]
public class LevellingSO : ScriptableObject
{
    public int MAX_LEVEL = 6;

    [Tooltip("Cumulative exp amounts for each level, indexed by level. E.g. index 1 is for level 1")]
    public List<int> m_RequiredExpAmounts;

    public int GetRequiredExpAmount(int level)
    {
        if (level > MAX_LEVEL)
            return 0;
        return m_RequiredExpAmounts[level];
    }
}
