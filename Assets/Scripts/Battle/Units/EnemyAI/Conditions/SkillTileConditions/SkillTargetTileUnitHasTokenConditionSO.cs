using UnityEngine;

[CreateAssetMenu(fileName = "SkillTargetTileUnitHasTokenConditionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/SkillTargetTileConditions/SkillTargetTileUnitHasTokenConditionSO")]
public class SkillTargetTileUnitHasTokenConditionSO : EnemySkillTileConditionSO
{
    public TokenType m_TokenType;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile, ActiveSkillSO activeSkill)
    {
        Unit unit = mapLogic.GetUnitAtTile(activeSkill.IsOpposingSideTarget ? GridType.PLAYER : GridType.ENEMY, targetTile);
        return unit.HasToken(m_TokenType);
    }
}
