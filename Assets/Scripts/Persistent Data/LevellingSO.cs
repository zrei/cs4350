using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevellingSO", menuName = "ScriptableObject/LevellingSO")]
public class LevellingSO : ScriptableObject
{
    public static int MAX_LEVEL = 50;

    [Tooltip("Required exp amounts for each level, indexed by level. E.g. index 1 is for level 1")]
    public List<int> m_RequiredExpAmounts;

    public int GetRequiredExpAmount(int level)
    {
        return m_RequiredExpAmounts[level];
    }
}
