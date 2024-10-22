using UnityEngine;

public abstract class EnemySkillTileConditionSO : ScriptableObject
{
    public abstract bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile, ActiveSkillSO activeSkill);
}
