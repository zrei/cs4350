using UnityEngine;

[CreateAssetMenu(fileName = "TeleportTargetWithinRangeOfAttackerRuleSO", menuName = "ScriptableObject/ActiveSkills/TeleportTargetRules/TeleportTargetWithinRangeOfAttackerRuleSO")]
public class TeleportTargetWithinRangeOfAttackerRuleSO : TeleportRuleSO
{
    [Header("This is valid for both side targets - east and west cardinal directions don't work very well though")]
    public RangeDefinition m_AllowedRange;

    public override bool IsValidTeleportTile(GridType targetGridType, CoordPair initialTarget, CoordPair targetTile, Unit attackingUnit)
    {
        return m_AllowedRange.IsWithinRange(GridHelper.GetSameSide(attackingUnit.UnitAllegiance), targetGridType, attackingUnit.CurrPosition, targetTile);
    }
}

[System.Serializable]
public struct RangeDefinition
{
    [Tooltip("Whether to limit the range to only cardinal directions")]
    public bool m_LimitCardinal;

    [Header("Cardinal limits - will be IGNORED if range is not limited to cardinals\nTick the directions that are allowed")]
    [Tooltip("In direction (1, 0) - away from the center line")]
    public bool m_North;
    [Tooltip("In direction (-1, 0) - towards the center line")]
    public bool m_South;
    [Tooltip("In direction (0, 1) - with the center line behind you, towards the right")]
    public bool m_East;
    [Tooltip("In direction (0, -1) - with the center line behind you, towards the left")]
    public bool m_West;

    [Space]
    [Header("The distance that the tile must be within.")]
    public int m_Range;

    public bool IsWithinRange(GridType startingGridType, GridType endGridType, CoordPair start, CoordPair destination)
    {
        bool withinDistance = GridDistanceHelper.CalculateManhattenDistance(startingGridType, endGridType, start, destination) <= m_Range;

        if (!m_LimitCardinal)
            return withinDistance;

        int convertedEndCol = GridDistanceHelper.ConvertColToSameSide(startingGridType, endGridType, destination.m_Col);
        // limits cardinal but the destination is not within the cardinal directions of the start tile
        if (destination.m_Row != start.m_Row && convertedEndCol != start.m_Col)
            return false;

        Debug.Log("Initial destination: " + destination.m_Col);
        Debug.Log("Converted end col: " + convertedEndCol);
        Debug.Log("Start col: " + start.m_Col);
        bool onNorthSide = startingGridType == endGridType && destination.m_Row > start.m_Row && convertedEndCol == start.m_Col;
        bool onSouthSide = (startingGridType != endGridType || destination.m_Row < start.m_Row) && convertedEndCol == start.m_Col;
        bool onEastSide = destination.m_Row == start.m_Row && convertedEndCol > start.m_Col;
        bool onWestSide = destination.m_Row == start.m_Row && convertedEndCol < start.m_Col;

        if (!m_North && onNorthSide)
            return false;
        if (!m_South && onSouthSide)
            return false;
        if (!m_East && onEastSide)
            return false;
        if (!m_West && onWestSide)
            return false;

        Debug.Log("Within distance!" + endGridType + ", " + start + ", " + destination);
        return withinDistance;
    }
}