using UnityEngine;

public abstract class SkillTargetRuleSO : ScriptableObject
{
    public abstract bool IsValidTargetTile(CoordPair targetTile, Unit attackingUnit, GridType targetGridType);
}
