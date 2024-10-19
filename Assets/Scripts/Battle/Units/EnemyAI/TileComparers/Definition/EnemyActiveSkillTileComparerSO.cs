using UnityEngine;

public abstract class EnemyActiveSkillTileComparerSO : ScriptableObject
{
    public abstract float GetTileValue(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile, ActiveSkillSO activeSkill);
}
