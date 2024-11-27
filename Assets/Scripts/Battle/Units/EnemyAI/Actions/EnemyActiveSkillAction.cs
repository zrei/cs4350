using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class EnemyActiveSkillActionWrapper : EnemyActionWrapper
{
    public IEnumerable<CoordPair> PossibleAttackPositions => m_PossibleAttackPositionsIgnoreOccupied;

    private IEnumerable<CoordPair> m_PossibleAttackPositions;
    private IEnumerable<CoordPair> m_PossibleAttackPositionsIgnoreOccupied;

    private EnemyActiveSkillAction ActiveSkillAction => (EnemyActiveSkillAction) m_Action;
    private ActiveSkillSO ActiveSkill => ActiveSkillAction.m_ActiveSkill;
    public GridType TargetGridType => ActiveSkillAction.TargetGridType;

    public override bool ShouldBreakOut(EnemyUnit enemyUnit, MapLogic mapLogic)
    {
        return !ActiveSkillAction.CanActionBePerformed(enemyUnit, mapLogic, out m_PossibleAttackPositions, out m_PossibleAttackPositionsIgnoreOccupied);
    }

    public override void Run(EnemyUnit enemyUnit, MapLogic mapLogic, VoidEvent completeActionEvent)
    {
        CoordPair finalTarget = ActiveSkillAction.GetChosenTargetTile(enemyUnit, mapLogic, m_PossibleAttackPositions);
        var attackDelay = 1.5f;

        IEnumerator PlayActionWithAnimation()
        {
            int animationTrigger = 0;
            animationTrigger += (int)(ActiveSkill.m_OverrideWeaponAnimationType ? ActiveSkill.m_OverriddenWeaponAnimationType : enemyUnit.WeaponAnimationType);
            animationTrigger += (int)ActiveSkill.m_SkillAnimationType;

            enemyUnit.PlaySkillStartAnimation(animationTrigger);

            mapLogic.ShowAttackForecast(TargetGridType, new List<CoordPair>() {finalTarget});
            yield return new WaitForSeconds(attackDelay);
            mapLogic.ShowAttackForecast(TargetGridType, new List<CoordPair>() { });

            if (ActiveSkill.ContainsAllSkillTypes(SkillEffectType.TELEPORT))
                mapLogic.PerformTeleportSkill(TargetGridType, enemyUnit, ActiveSkill, finalTarget, ActiveSkillAction.GetChosenTeleportTile(enemyUnit, mapLogic, finalTarget), completeActionEvent);
            else
                mapLogic.PerformSkill(TargetGridType, enemyUnit, ActiveSkill, finalTarget, completeActionEvent);
        }

        CoroutineManager.Instance.StartCoroutine(PlayActionWithAnimation());
    }

    public override HashSet<ActiveSkillSO> GetNestedActiveSkills()
    {
        return new() {ActiveSkill};
    }
}

public class EnemyActiveSkillAction : EnemyActionInstance
{
    public ActiveSkillSO m_ActiveSkill;
    public List<EnemySkillTileConditionSO> m_TargetConditions;
    public List<EnemyActiveSkillTileComparerSO> m_TileComparers;

    [Header("Special teleport handling - ignore if skill is not teleport type")]
    public List<EnemyTeleportTileConditionSO> m_TeleportTileConditions;
    public List<EnemyTeleportTileComparerSO> m_TeleportTileComparers;

    public GridType TargetGridType => m_ActiveSkill.TargetGridType(UnitAllegiance.ENEMY);
    
    public override IConcreteAction GenerateConcreteAction()
    {
        return new EnemyActiveSkillActionWrapper {m_Action = this};
    }

    public bool CanActionBePerformed(EnemyUnit enemyUnit, MapLogic mapLogic, out IEnumerable<CoordPair> targetablePositions, out IEnumerable<CoordPair> targetablePositionsIgnoreOccupiedAndAdditionalConditions)
    {
        targetablePositions = new List<CoordPair>();
        targetablePositionsIgnoreOccupiedAndAdditionalConditions = new List<CoordPair>();

        if (!enemyUnit.CanPerformSkill(m_ActiveSkill))
            return false;

        if (m_ActiveSkill.m_ConsumedMana > enemyUnit.CurrentMana)
            return false;

        bool isTeleportSkill = m_ActiveSkill.ContainsSkillType(SkillEffectType.TELEPORT);

        bool hasPossibleAttackPosition = false;

        GridType targetGridType = TargetGridType;
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                CoordPair coordinates = new CoordPair(r, c);
                if (mapLogic.CanPerformSkill(m_ActiveSkill, enemyUnit, coordinates, targetGridType, true) && m_TargetConditions.All(cond => cond.IsConditionMet(enemyUnit, mapLogic, coordinates, m_ActiveSkill)) && (!isTeleportSkill || IsValidTeleportTargetTile(enemyUnit, mapLogic, coordinates)))
                {
                    targetablePositions = targetablePositions.Append(coordinates);
                    hasPossibleAttackPosition = true;
                }
                if (mapLogic.CanPerformSkill(m_ActiveSkill, enemyUnit, coordinates, targetGridType, false))
                {
                    targetablePositionsIgnoreOccupiedAndAdditionalConditions = targetablePositionsIgnoreOccupiedAndAdditionalConditions.Append(coordinates);
                }
            }
        }

        return hasPossibleAttackPosition;
    }

    private bool IsValidTeleportTargetTile(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair initialTarget)
    {
        return GetValidTeleportTiles(enemyUnit, mapLogic, initialTarget).Count() > 0;
    }

    public CoordPair GetChosenTargetTile(EnemyUnit enemyUnit, MapLogic mapLogic, IEnumerable<CoordPair> targetablePositions)
    {
        IEnumerable<CoordPair> finalTiles = targetablePositions;
        
        if (m_TileComparers.Count > 0)
        {
            EnemyActiveSkillTileComparerSO firstTileComparer = m_TileComparers[0];
            IOrderedEnumerable<CoordPair> sortedCoordPair = finalTiles.OrderBy(tile => firstTileComparer.GetTileValue(enemyUnit, mapLogic, tile, m_ActiveSkill));
            for (int i = 1; i < m_TileComparers.Count; ++i)
            {
                sortedCoordPair = sortedCoordPair.ThenBy(tile => m_TileComparers[i].GetTileValue(enemyUnit, mapLogic, tile, m_ActiveSkill));
            }

            finalTiles = sortedCoordPair;
        }
            
        return finalTiles.First();
    }

    private IEnumerable<CoordPair> GetValidTeleportTiles(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair initialTarget)
    {
        IEnumerable<CoordPair> possibleTeleportTargetTiles = new List<CoordPair>();
        GridType targetTeleportGrid = m_ActiveSkill.TeleportTargetGrid(enemyUnit);
        CoordPair finalTeleportedTile = m_ActiveSkill.TeleportStartTile(enemyUnit, initialTarget);
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                CoordPair teleportTargetTile = new CoordPair(r, c);
                if (mapLogic.IsValidTeleportTile(m_ActiveSkill, enemyUnit, finalTeleportedTile, teleportTargetTile, targetTeleportGrid) && m_TeleportTileConditions.All(x => x.IsConditionMet(enemyUnit, mapLogic, targetTeleportGrid, teleportTargetTile, finalTeleportedTile)))
                {
                    possibleTeleportTargetTiles = possibleTeleportTargetTiles.Append(teleportTargetTile);
                }
            }
        }
        return possibleTeleportTargetTiles;
    }

    public CoordPair GetChosenTeleportTile(EnemyUnit enemyUnit, MapLogic mapLogic, CoordPair initialTarget)
    {
        IEnumerable<CoordPair> finalTiles = GetValidTeleportTiles(enemyUnit, mapLogic, initialTarget);
        GridType targetTeleportGrid = m_ActiveSkill.TeleportTargetGrid(enemyUnit);
        CoordPair teleportStartTile = m_ActiveSkill.TeleportStartTile(enemyUnit, initialTarget);

        if (m_TileComparers.Count > 0)
        {
            EnemyTeleportTileComparerSO firstTileComparer = m_TeleportTileComparers[0];
            IOrderedEnumerable<CoordPair> sortedCoordPair = finalTiles.OrderBy(tile => firstTileComparer.GetTileValue(enemyUnit, mapLogic, tile, teleportStartTile, targetTeleportGrid));
            for (int i = 1; i < m_TileComparers.Count; ++i)
            {
                sortedCoordPair = sortedCoordPair.ThenBy(tile => m_TeleportTileComparers[i].GetTileValue(enemyUnit, mapLogic, tile, teleportStartTile, targetTeleportGrid));
            }

            finalTiles = sortedCoordPair;
        }
            
        return finalTiles.First();
    }
}
