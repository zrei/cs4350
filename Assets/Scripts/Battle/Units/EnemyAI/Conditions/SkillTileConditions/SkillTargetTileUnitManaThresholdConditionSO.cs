using UnityEngine;

[CreateAssetMenu(fileName = "SkillTargetTileUnitManaThresholdConditionSO", menuName = "ScriptableObject/Battle/Enemy/EnemyAI/Conditions/SkillTargetTileConditions/SkillTargetTileUnitManaThresholdConditionSO")]
public class SkillTargetTileUnitManaThresholdConditionSO : EnemySkillTileConditionSO
{
    public Threshold m_ManaThreshold;
    [Tooltip("Whether this is checking the flat mana amounts or not")]
    public bool m_IsFlat;

    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile, ActiveSkillSO activeSkill)
    {
        Unit unit = mapLogic.GetUnitAtTile(activeSkill.IsOpposingSideTarget ? GridType.PLAYER : GridType.ENEMY, targetTile);
        return m_ManaThreshold.IsSatisfied(m_IsFlat ? unit.CurrentMana : unit.CurrentManaProportion);
    }
}
