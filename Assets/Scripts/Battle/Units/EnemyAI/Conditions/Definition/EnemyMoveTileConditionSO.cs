using UnityEngine;

public abstract class EnemyMoveTileConditionSO : ScriptableObject
{
    public abstract bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile);
}
