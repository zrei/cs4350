using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// An instance, in-battle, of a stack of tokens coming from the same group
/// </summary>
public class TokenStack :
    /*
    IFlatStatChange,
    IMultStatChange,
    IInflictStatus,
    ICritModifier,
    */
    IStatus
{
    #region Details
    protected TokenTierSO m_TokenTierData;
    public bool AllowStack => m_TokenTierData.m_AllowStack;
    private int m_NumTiers;
    public int Id => m_TokenTierData.m_Id;
    public TokenType TokenType => m_TokenTierData.TokenType;
    public bool IsBuff => m_TokenTierData.m_IsBuff;
    public bool ContainsConsumptionType(TokenConsumptionType tokenConsumptionType) => m_TokenTierData.ContainsConsumptionType(tokenConsumptionType);
    #endregion

    #region Stack
    protected List<int> m_NumTokensOfEachTier;
    #endregion
    
    #region State
    public virtual bool IsEmpty => m_NumTokensOfEachTier.All(x => x <= 0);
    private bool m_IsPermanent;
    private int m_NumActivationTimes;
    /// <summary>
    /// On the latest check for the current situation, were the conditiions met?
    /// </summary>
    private bool m_IsConditionMetThisIteration = false;
    private bool m_HasBeenCheckedThisIteration = false;
    #endregion

    #region IStatus
    public Sprite Icon => m_TokenTierData.m_Icon;
    public Color Color => m_TokenTierData.m_Color;
    public string DisplayTier => m_NumTiers == 1 ? string.Empty : TokenUtil.NumToRomanNumeral(GetMaxTier());
    public string DisplayStacks
    {
        get
        {
            var tierIndex = GetMaxTier() - 1;
            if (tierIndex < 0 || tierIndex >= m_NumTiers)
            {
                return string.Empty;
            }

            return $"<size=50%>x</size>{m_NumTokensOfEachTier[tierIndex]}<sprite name=\"Stack\">";
        }
    }
    public string Name => m_TokenTierData.m_TokenName;
    public string Description => m_TokenTierData.m_Description;
    public List<int> NumStacksPerTier => m_NumTokensOfEachTier;
    public int CurrentHighestTier => GetMaxTier();
    #endregion

    public TokenStack(TokenTierSO tokenTier, int initialTier, int initialNumber = 1, bool isPermanent = false)
    {
        m_TokenTierData = tokenTier;
        m_NumTiers = m_TokenTierData.NumTiers;
        m_NumTokensOfEachTier = new List<int>(m_NumTiers);
        for (int i = 0; i < tokenTier.NumTiers; ++i)
        {
            m_NumTokensOfEachTier.Add(0);
        }
        m_NumTokensOfEachTier[initialTier - 1] = initialNumber; 
        m_IsPermanent = isPermanent;
    }

    /// <summary>
    /// Consume a single token, taking the highest tiered one first
    /// </summary>
    public void ConsumeToken()
    {
        m_HasBeenCheckedThisIteration = false;
        if (m_IsConditionMetThisIteration)
            ++m_NumActivationTimes;
        m_IsConditionMetThisIteration = false;
        if (m_IsPermanent)
        {
            return;
        }
        if (IsEmpty)
            return;
        m_NumTokensOfEachTier[GetMaxTier() - 1]--;
    }

    private int GetMaxTier()
    {
        for (int i = m_NumTiers; i > 0; --i)
        {
            if (m_NumTokensOfEachTier[i - 1] > 0)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Add a token of a tier belonging to this group, taking into account whether
    /// stacking is allowed
    /// </summary>
    /// <param name="tier"></param>
    /// <param name="number"></param>
    public void AddToken(int tier, int number = 1)
    {
        if (!AllowStack && !IsEmpty)
        {
            Logger.Log(this.GetType().Name, $"No stacking allowed for token tier {Id}", LogLevel.LOG);
            return;
        }
        
        m_NumTokensOfEachTier[tier - 1] += number;
    }

    public float GetMultStatChange(StatType statType, Unit unit)
    {
        if (TokenType == TokenType.MULT_STAT_CHANGE)
        {
            MultStatChangeTokenTierSO mult = (MultStatChangeTokenTierSO) m_TokenTierData;
            if (!mult.m_AffectedStats.Contains(statType) || !CheckConditions(unit))
            {
                return 1f;
            }
            else
            {
                return mult.GetMultStatChange(GetMaxTier());
            }
        }
        else
        {
            return 1f;
        }
    }

    public float GetFlatStatChange(StatType statType, Unit unit)
    {
        if (TokenType == TokenType.FLAT_STAT_CHANGE)
        {
            FlatStatChangeTokenTierSO flat = (FlatStatChangeTokenTierSO) m_TokenTierData;
            if (!flat.m_AffectedStats.Contains(statType) || !CheckConditions(unit))
            {
                return 0f;
            }
            else
            {
                return flat.GetFlatStatChange(GetMaxTier());
            }
        }
        else
        {
            return 0f;
        }
    }

    public bool TryGetFlatPassiveChange(Unit unit, out PassiveChangeBundle passiveChangeBundle)
    {
        if (TokenType == TokenType.FLAT_PASSIVE_CHANGE)
        {
            FlatPassiveChangeTokenTierSO flat = (FlatPassiveChangeTokenTierSO) m_TokenTierData;
            if (!CheckConditions(unit))
            {
                passiveChangeBundle = new PassiveChangeBundle(0f, 0f);
                return false;
            }
            else
            {
                flat.GetFlatChange(GetMaxTier(), out float healthAmount, out float manaAmount);
                passiveChangeBundle = new PassiveChangeBundle(healthAmount, manaAmount);
                return true;
            }
        }
        else
        {
            passiveChangeBundle = new PassiveChangeBundle(0f, 0f);
            return false;
        }
    }

    public bool TryGetMultPassiveChange(Unit unit, out PassiveChangeBundle passiveChangeBundle)
    {
        if (TokenType == TokenType.MULT_PASSIVE_CHANGE)
        {
            MultPassiveChangeTokenTierSO flat = (MultPassiveChangeTokenTierSO) m_TokenTierData;
            if (!CheckConditions(unit))
            {
                passiveChangeBundle = new PassiveChangeBundle(0f, 0f);
                return false;
            }
            else
            {
                flat.GetMultChange(GetMaxTier(), out float healthAmount, out float manaAmount);
                passiveChangeBundle = new PassiveChangeBundle(healthAmount, manaAmount);
                return true;
            }
        }
        else
        {
            passiveChangeBundle = new PassiveChangeBundle(0f, 0f);
            return false;
        }
    }

    public bool TryGetInflictedStatusEffect(Unit unit, out StatusEffect statusEffect)
    {
        if (TokenType == TokenType.INFLICT_STATUS && CheckConditions(unit))
        {
            statusEffect = ((StatusEffectTokenTierSO) m_TokenTierData).GetInflictedStatusEffect(GetMaxTier());
            return true;
        }
        else
        {
            statusEffect = null;
            return false;
        }
    }

    public bool TryGetInflictedToken(Unit unit, out InflictedToken inflictedToken)
    {
        if (TokenType == TokenType.APPLY_TOKEN && CheckConditions(unit))
        {
            inflictedToken = ((ApplyTokenTierSO) m_TokenTierData).GetInflictedToken(GetMaxTier());
            return true;
        }
        else
        {
            inflictedToken = null;
            return false;
        }
    }

    public bool TryGetInflictedTileEffect(Unit unit, out InflictedTileEffect tileEffect)
    {
        if (TokenType == TokenType.SPAWN_TILE_EFFECT && CheckConditions(unit))
        {
            tileEffect = ((ApplyTileEffectTokenTierSO) m_TokenTierData).GetInflictedTileEffect(GetMaxTier());
            return true;
        }
        else
        {
            tileEffect = default;
            return false;
        }
    }

    public bool TryGetSummonedUnits(Unit unit, out List<SummonWrapper> summonWrappers)
    {
        if (TokenType == TokenType.SUMMON && CheckConditions(unit))
        {
            summonWrappers = ((SummonUnitsTokenTierSO) m_TokenTierData).GetSummonWrappers(GetMaxTier());
            return true;
        }
        else
        {
            summonWrappers = default;
            return false;
        }
    }

    public float GetFinalCritProportion(Unit unit)
    {
        if (TokenType == TokenType.CRIT && CheckConditions(unit))
        {
            return ((CritTokenTierSO) m_TokenTierData).GetFinalDamageModifier(GetMaxTier());
        }
        else
        {
            return 1f;
        }
    }

    public float GetLifestealProportion(Unit unit)
    {
        if (TokenType == TokenType.LIFESTEAL && CheckConditions(unit))
        {
            return ((LifestealTokenTierSO) m_TokenTierData).GetLifestealProportion(GetMaxTier());
        }
        else
        {
            return 0f;
        }
    }

    public float GetReflectProportion(Unit unit)
    {
        if (TokenType == TokenType.REFLECT && CheckConditions(unit))
        {
            return ((ReflectTokenTierSO) m_TokenTierData).GetReflectProportion(GetMaxTier());
        }
        else
        {
            return 0f;
        }
    }

    public bool CheckConditions(Unit unit, AttackInfo attackInfo = null)
    {
        if (!m_TokenTierData.m_ResetConditionMet && m_HasBeenCheckedThisIteration)
            return m_IsConditionMetThisIteration;

        m_IsConditionMetThisIteration = (!m_TokenTierData.m_LimitedActivation || m_NumActivationTimes < m_TokenTierData.m_MaxActivations) && ((m_TokenTierData.m_CannotBeDeactivated && m_NumActivationTimes > 0 ) || m_TokenTierData.IsConditionsMet(unit, BattleManager.Instance.MapLogic, attackInfo));
        m_HasBeenCheckedThisIteration = true;
        return m_IsConditionMetThisIteration;
    }
}

public class TauntTokenStack : TokenStack
{
    public Unit TauntedUnit {get; private set;}
    public override bool IsEmpty => base.IsEmpty || TauntedUnit == null || TauntedUnit.IsDead;

    public TauntTokenStack(Unit targetedUnit, TokenTierSO tokenTierSO, int initialNumber = 1) : base(tokenTierSO, 1, initialNumber)
    {
        TauntedUnit = targetedUnit;
    }
}

public class TargetOtherUnitsTokenStack : TokenStack 
{
    private TargetOtherTilesTokenTierSO TargetOtherUnitsTokenTierSO => (TargetOtherTilesTokenTierSO) m_TokenTierData;

    public TargetOtherUnitsTokenStack(TokenTierSO tokenTierSO, int tier, int initialNumber = 1, bool isPermanent = false) : base(tokenTierSO, tier, initialNumber, isPermanent) {}

    public IEnumerable<(CoordPair, GridType)> GetTargettedUnits(Unit currUnit, List<Unit> filteredUnits = null)
    {
        List<(CoordPair, GridType)> targetedCoordinates = new();

        bool requireOccupiedTiles = TargetOtherUnitsTokenTierSO.RequiresTargetedSquares;
        
        List<ActionConditionSO> targetRules = TargetOtherUnitsTokenTierSO.m_TargetRules;
        MapLogic mapLogic = BattleManager.Instance.MapLogic;

        GetTargetPositionsOnGrid(currUnit, GridType.PLAYER, requireOccupiedTiles, mapLogic, targetedCoordinates);
        GetTargetPositionsOnGrid(currUnit, GridType.ENEMY, requireOccupiedTiles, mapLogic, targetedCoordinates);

        IEnumerable<(CoordPair, GridType)> finalTargets = targetedCoordinates.Where(x => {
            Unit unitAtTile = mapLogic.GetUnitAtTile(x.Item2, x.Item1);
            return unitAtTile == null || targetRules.All(rule => rule.IsConditionMet(unitAtTile, mapLogic));
        });
            
        if (TargetOtherUnitsTokenTierSO.m_TargetFilteredUnitsOnly)
        {
            if (filteredUnits == null)
                return new List<(CoordPair, GridType)>{};
            else
                return finalTargets.Where(x => filteredUnits.Contains(mapLogic.GetUnitAtTile(x.Item2, x.Item1)));
        }
        return finalTargets;
    }

    private void GetTargetPositionsOnGrid(Unit currUnit, GridType gridType, bool requiresOccupiedTiles, MapLogic mapLogic, in List<(CoordPair, GridType)> positions)
    {
        for (int r = 0; r < MapData.NUM_ROWS; ++r)
        {
            for (int c = 0; c < MapData.NUM_COLS; ++c)
            {
                CoordPair position = new CoordPair(r, c);
                bool isOccupied = mapLogic.IsTileOccupied(gridType, position);
                if (requiresOccupiedTiles && !isOccupied)
                    continue;
                if (!AreTargetConditionsMet(currUnit, position, gridType))
                    continue;
                positions.Add((position, gridType));
            }
        }
    }

    private bool AreTargetConditionsMet(Unit currUnit, CoordPair coordPair, GridType gridType)
    {
        List<TargetLocationRuleSO> targetLocationRules = TargetOtherUnitsTokenTierSO.m_TargetLocationRules;
        List<TargetSideLimitRuleSO> targetSideLimitRules = TargetOtherUnitsTokenTierSO.m_TargetSideRules;
        return targetLocationRules.All(x => x.IsValidTargetTile(coordPair, currUnit, gridType)) && targetSideLimitRules.All(x => x.IsValidTargetTile(coordPair, currUnit, gridType));
    }
}
