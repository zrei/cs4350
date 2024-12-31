using UnityEngine;

[CreateAssetMenu(fileName = "SkillTargetIsNotSelfConditionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/SkillTargetTileConditions/SkillTargetIsNotSelfConditionSO")]
public class SkillTargetIsNotSelfConditionSO : EnemySkillTileConditionSO
{
    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile, ActiveSkillSO activeSkill)
    {
        Unit unit = mapLogic.GetUnitAtTile(activeSkill.IsOpposingSideTarget ? GridType.PLAYER : GridType.ENEMY, targetTile);
        return unit != enemyUnit;
    }
}
