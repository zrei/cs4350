using UnityEngine;

[CreateAssetMenu(fileName = "SkillTargetTileNumUnitsComparisonSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/TileComparers/SkillTargetTileComparers/SkillTargetTileNumUnitsComparisonSO")]
public class SkillTargetTileNumUnitsComparisonSO : EnemyActiveSkillTileComparerSO
{
    public override float GetTileValue(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile, ActiveSkillSO activeSkill)
    {
        return mapLogic.GetNumUnitsTargeted(activeSkill.IsOpposingSideTarget ? GridType.PLAYER : GridType.ENEMY, activeSkill, targetTile);
    }
}
