using UnityEngine;

[CreateAssetMenu(fileName = "TeleportTargetWithinRangeOfAttackerRuleSO", menuName = "ScriptableObject/ActiveSkills/TeleportTargetRules/TeleportTargetWithinRangeOfAttackerRuleSO")]
public class TeleportTargetWithinRangeOfAttackerRuleSO : TeleportRuleSO
{
    [Header("NOTE THAT THIS IS ONLY VALID FOR SAME-SIDE TARGETTING RULES")]
    public RangeDefinition m_AllowedRange;

    public override bool IsValidTeleportTile(CoordPair initialTarget, CoordPair targetTile, Unit attackingUnit)
    {
        return m_AllowedRange.IsWithinRange(attackingUnit.CurrPosition, targetTile);
    }
}

[System.Serializable]
public struct RangeDefinition
{
    [Tooltip("Whether to limit the range to only cardinal directions")]
    public bool m_LimitCardinal;

    [Header("Cardinal limits - will be IGNORED if range is not limited to cardinals\nTick the directions that are allowed")]
    [Tooltip("In direction (1, 0)")]
    public bool m_North;
    [Tooltip("In direction (-1, 0)")]
    public bool m_South;
    [Tooltip("In direction (0, 1)")]
    public bool m_East;
    [Tooltip("In direction (0, -1)")]
    public bool m_West;

    [Space]
    [Header("The distance that the tile must be within.")]
    public int m_Range;

    public bool IsWithinRange(CoordPair start, CoordPair destination)
    {
        bool withinDistance = start.GetDistanceToPoint(destination) <= m_Range;

        if (!m_LimitCardinal)
            return withinDistance;

        // limits cardinal but the destination is not within the cardinal directions of the start tile
        if (destination.m_Row != start.m_Row && destination.m_Col != start.m_Col)
            return false;

        bool onNorthSide = destination.m_Row > start.m_Row && destination.m_Col == start.m_Col;
        bool onSouthSide = destination.m_Row < start.m_Row && destination.m_Col == start.m_Col;
        bool onEastSide = destination.m_Row == start.m_Row && destination.m_Col > start.m_Col;
        bool onWestSide = destination.m_Row == start.m_Row && destination.m_Col < start.m_Col;

        if (!m_North && onNorthSide)
            return false;
        if (!m_South && onSouthSide)
            return false;
        if (!m_East && onEastSide)
            return false;
        if (!m_West && onWestSide)
            return false;

        return withinDistance;
    }
}