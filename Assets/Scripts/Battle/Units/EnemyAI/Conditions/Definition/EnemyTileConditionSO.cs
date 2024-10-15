using UnityEngine;

public abstract class EnemyTileConditionSO : ScriptableObject
{
    public abstract bool IsConditionMet(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair targetTile);
}
