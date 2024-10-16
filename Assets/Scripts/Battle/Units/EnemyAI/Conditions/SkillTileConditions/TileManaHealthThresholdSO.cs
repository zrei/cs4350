using UnityEngine;

public class TileManaHealthThresholdSO : EnemySkillTileConditionSO
{
    public float m_ManaThreshold;
    [Tooltip("Whether to check if the tile unit's health is greater or less than the threshold")]
    public bool m_GreaterThan;
    public override bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile, ActiveSkillSO activeSkill)
    {
        Unit unit = mapLogic.GetUnitAtTile(activeSkill.IsOpposingSideTarget ? GridType.PLAYER : GridType.ENEMY, targetTile);
        if (m_GreaterThan)
        {
            return unit.CurrentMana > m_ManaThreshold;
        }
        else
        {

            return unit.CurrentMana < m_ManaThreshold;
        }
    }
}
