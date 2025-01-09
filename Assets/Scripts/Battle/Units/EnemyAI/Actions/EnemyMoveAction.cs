using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class EnemyMoveActionWrapper : EnemyActionWrapper
{
    private IEnumerable<CoordPair> m_CanOccupyTiles;

    private EnemyMoveAction MoveAction => (EnemyMoveAction) m_Action;

    public override bool ShouldBreakOut(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return !MoveAction.CanActionBePerformed(enemyUnit, mapLogic, out m_CanOccupyTiles);
    }

    public override void Run(EnemyUnit enemyUnit, MapLogic mapLogic, BoolEvent completeActionEvent)
    {
        // calculate the final tile that the unit wants to move towards
        CoordPair finalTile = MoveAction.GetChosenTile(enemyUnit, mapLogic, m_CanOccupyTiles);
        
        mapLogic.TryReachTile(GridType.ENEMY, enemyUnit, finalTile, () => OnMoveComplete(enemyUnit, mapLogic, completeActionEvent));
    }

    private void OnMoveComplete(EnemyUnit enemyUnit, MapLogic mapLogic, BoolEvent completeActionEvent)
    {
        enemyUnit.ConsumeTokens(TokenConsumptionType.CONSUME_ON_MOVE);
        mapLogic.ApplyTileEffectOnUnit(GridType.ENEMY, enemyUnit);
        if (enemyUnit.IsDead)
            enemyUnit.Die();

        completeActionEvent?.Invoke(false);
    }

    public override HashSet<ActiveSkillSO> GetNestedActiveSkills()
    {
        return new();
    }
}

public class EnemyMoveAction : EnemyActionInstance
{
    public List<EnemyMoveTileConditionSO> m_TargetConditions;
    public List<EnemyMoveTileComparerSO> m_TileComparers;

    public bool CanActionBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic, out IEnumerable<CoordPair> reachablePositions)
    {
        // check if there's any space to move to
        reachablePositions = mapLogic.GetUnoccupiedTiles(GridType.ENEMY).Where(coord => m_TargetConditions.All(cond => cond.IsConditionMet(enemyUnit, mapLogic, coord)));
        return reachablePositions.Count() > 0;
    }

    // can perform action here... hm
    public CoordPair GetChosenTile(EnemyUnit enemyUnit, MapLogic mapLogic, IEnumerable<CoordPair> reachablePositions)
    {
        IEnumerable<CoordPair> finalTiles = reachablePositions;
        
        if (m_TileComparers.Count > 0)
        {
            EnemyMoveTileComparerSO firstTileComparer = m_TileComparers[0];
            IOrderedEnumerable<CoordPair> sortedCoordPair = finalTiles.OrderBy(tile => firstTileComparer.GetTileValue(enemyUnit, mapLogic, tile));
            for (int i = 1; i < m_TileComparers.Count; ++i)
            {
                sortedCoordPair = sortedCoordPair.ThenBy(tile => m_TileComparers[i].GetTileValue(enemyUnit, mapLogic, tile));
            }

            finalTiles = sortedCoordPair;
        }
            
        return finalTiles.First();
    }

    public override IConcreteAction GenerateConcreteAction()
    {
        return new EnemyMoveActionWrapper {m_Action = this};
    }
}
