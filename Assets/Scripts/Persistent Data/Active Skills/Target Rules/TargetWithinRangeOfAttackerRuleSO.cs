using UnityEngine;

[CreateAssetMenu(fileName = "TargetWithinRangeOfAttackerRuleSO", menuName = "ScriptableObject/ActiveSkills/TargetRules/TargetWithinRangeOfAttackerRuleSO")]
public class TargetWithinRangeOfAttackerRuleSO : TargetLocationRuleSO
{
    [Header("This is valid for both types of targetting - it will work by counting the manhatten distance")]
    public RangeDefinition m_AllowedRange;

    public override bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType)
    {
        return m_AllowedRange.IsWithinRange(GridHelper.GetSameSide(attackingUnit.UnitAllegiance), targetGridType, attackingUnit.CurrPosition, targetTile);
    }
}

public static class GridDistanceHelper
{
    public static int CalculateManhattenDistance(GridType startingGridType, GridType targetGridType, CoordPair startingCoordinates, CoordPair endingCoordinates)
    {
        return CalculateRowDistance(startingGridType, targetGridType, startingCoordinates, endingCoordinates) + CalculateColDistance(startingGridType, targetGridType, startingCoordinates, endingCoordinates);
    }

    public static int CalculateRowDistance(GridType startingGridType, GridType targetGridType, CoordPair startingCoordinates, CoordPair endingCoordinates)
    {
        return startingGridType != targetGridType ? startingCoordinates.m_Row + 1 + endingCoordinates.m_Row : Mathf.Abs(endingCoordinates.m_Row - startingCoordinates.m_Row);
    }

    public static int CalculateColDistance(GridType startingGridType, GridType targetGridType, CoordPair startingCoordinates, CoordPair endingCoordinates)
    {
        return startingGridType != targetGridType ? Mathf.Abs(MapData.NUM_COLS - 1 - startingCoordinates.m_Col - endingCoordinates.m_Col) : Mathf.Abs(startingCoordinates.m_Col - endingCoordinates.m_Col);
    }

    public static bool IsSameCol(GridType startingGridType, GridType targetGridType, CoordPair startingCoordinates, CoordPair endingCoordinates)
    {
        return startingGridType == targetGridType ? startingCoordinates.m_Col == endingCoordinates.m_Col : (MapData.NUM_COLS - 1 - startingCoordinates.m_Col) == endingCoordinates.m_Col;
    }

    public static int ConvertColToSameSide(GridType startingGridType, GridType targetGridType, int col)
    {
        if (startingGridType == targetGridType)
            return col;
        return MapData.NUM_COLS - 1 - col;
    }
}
