using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class EnemyMoveActionWrapper : EnemyActionWrapper
{
    private IEnumerable<CoordPair> m_CanOccupyTiles;
    private CoordPair m_CachedTarget;

    private EnemyMoveActionSO MoveAction => (EnemyMoveActionSO) m_Action;

    public override bool CanActionBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return MoveAction.CanActionBePerformed(enemyUnit, mapLogic, out m_CanOccupyTiles);
    }

    public override void PerformAction(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent completeActionEvent)
    {
        /*
        float baseWeight = 1f / m_ReachablePoints.Count;

        List<(PathNode, float)> nodeWeights = m_ReachablePoints.Select(x => (x, baseWeight)).ToList();

        for (int i = 0; i < nodeWeights.Count; ++i)
        {
            (PathNode node, float weight) = nodeWeights[i];
            float finalNodeWeight = weight * MoveAction.GetFinalWeightProportionForTile(enemyUnit, mapLogic, node.m_Coordinates);
            nodeWeights[i] = (node, finalNodeWeight);
        }

        PathNode toMoveTo = RandomHelper.GetRandomT(nodeWeights);
        */

        CoordPair finalTile = MoveAction.GetChosenTile(enemyUnit, mapLogic, m_CanOccupyTiles);

        // calculate the final tile now :)
        mapLogic.TryReachTile(GridType.ENEMY, enemyUnit, finalTile, completeActionEvent);
    }
}

[CreateAssetMenu(fileName = "EnemyMoveActionSO", menuName="ScriptableObject/Battle/Enemy/EnemyAI/Actions/EnemyMoveActionSO")]
public class EnemyMoveActionSO : EnemyActionSO
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

    public override EnemyActionWrapper GetWrapper(int priority)
    {
        return new EnemyMoveActionWrapper {m_Action = this, m_Priority = priority};
    }
}
