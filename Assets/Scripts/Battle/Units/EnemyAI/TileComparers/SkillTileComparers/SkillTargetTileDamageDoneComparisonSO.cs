using UnityEngine;

[CreateAssetMenu(fileName = "SkillTargetTileDamageDoneComparisonSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/TileComparers/SkillTargetTileComparers/SkillTargetTileDamageDoneComparisonSO")]
public class SkillTargetTileDamageDoneComparisonSO : EnemyActiveSkillTileComparerSO
{
    public override float GetTileValue(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile, ActiveSkillSO activeSkill)
    {
        return mapLogic.GetDamageDoneBySkill(activeSkill.IsOpposingSideTarget ? GridType.PLAYER : GridType.ENEMY, enemyUnit, activeSkill, targetTile);
    }
}
