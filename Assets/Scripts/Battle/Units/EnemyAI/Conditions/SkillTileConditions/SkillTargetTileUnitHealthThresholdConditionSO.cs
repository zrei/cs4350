using UnityEngine;

[CreateAssetMenu(fileName = "SkillTargetTileUnitHealthThresholdConditionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/SkillTargetTileConditions/SkillTargetTileUnitHealthThresholdConditionSO")]
public class SkillTargetTileUnitHealthThresholdConditionSO : EnemySkillTileConditionSO
{
    public Threshold m_HealthThreshold;
    [Tooltip("Whether this is checking the flat health amounts or not")]
    public bool m_IsFlat;
    
    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile, ActiveSkillSO activeSkill)
    {
        Unit unit = mapLogic.GetUnitAtTile(activeSkill.IsOpposingSideTarget ? GridType.PLAYER : GridType.ENEMY, targetTile);
        return m_HealthThreshold.IsSatisfied(m_IsFlat ? unit.CurrentHealth : unit.CurrentHealthProportion);
    }
}
