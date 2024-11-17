using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TargetSO", menuName = "ScriptableObject/ActiveSkills/TargetSO")]
public class TargetSO : ScriptableObject
{
    [Tooltip("These are tiles that will also be targeted, represented as offsets from the target square")]
    public List<CoordPair> m_TargetTiles;

    public string m_Description;

    [Tooltip("The sprite to represent the target tiles")]
    public Sprite m_TargetRepSprite;

    public bool IsAoe => m_TargetTiles.Count > 0;

    public List<CoordPair> ConstructAttackTargetTiles(CoordPair target)
    {
        List<CoordPair> attackTargetTiles = new() {target};

        foreach (CoordPair offset in m_TargetTiles)
        {
            attackTargetTiles.Add(target.Offset(offset));
        }

        return attackTargetTiles;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        HashSet<CoordPair> previousTargets = new();
        foreach (CoordPair coordPair in m_TargetTiles)
        {
            if (coordPair.Equals(new CoordPair(0, 0)))
            {
                Logger.Log(this.GetType().Name, $"Target tiles for {name} repeats origin", LogLevel.WARNING);
            }
            else if (previousTargets.Contains(coordPair))
            {
                Logger.Log(this.GetType().Name, $"Repeated target tile: {coordPair} for {name}", LogLevel.WARNING);
            }
            else
            {
                previousTargets.Add(coordPair);
            }
        }
    }
#endif
}
