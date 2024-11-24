using Game.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum UnitAllegiance
{
    PLAYER,
    ENEMY,
    NONE
}

public struct UnitModelData
{
    public GameObject m_Model;
    public Material[] m_BaseMaterial;
    public float m_GridYOffset;
    public SkinnedMeshRenderer[] m_AttachItems;

    public UnitModelData(GameObject model, Material[] BaseMaterial, SkinnedMeshRenderer[] attachItems, float gridYOffset)
    {
        m_Model = model;
        m_BaseMaterial = BaseMaterial;
        m_AttachItems = attachItems;
        m_GridYOffset = gridYOffset;
    }
}

public delegate void TrackedValueEvent(float change, float current, float max);

public abstract class Unit : MonoBehaviour, IHealth, ICanAttack, IFlatStatChange, IMultStatChange, ICritModifier, ITauntTarget
{
    [SerializeField] private ArmorVisual m_ArmorVisual;
    public AnimationEventHandler AnimationEventHandler => m_ArmorVisual.AnimationEventHandler;
    public Transform BodyCenter => m_ArmorVisual.BodyCenter;

    [SerializeField] private UnitMarker m_UnitMarker;
    public UnitMarker UnitMarker => m_UnitMarker;

    #region Current Status
    // current health
    protected float m_CurrHealth;

    protected float m_CurrMana;

    /// <summary>
    /// This is used to store what their stats SHOULD be,
    /// accounting for base stats + class bonuses so far.
    /// It DOES NOT account for any transient stat changes
    /// due to buffs/debuffs.
    /// </summary>
    protected Stats m_Stats;

    private CoordPair m_CurrPosition;
    public CoordPair CurrPosition => m_CurrPosition;

    protected StatusManager m_StatusManager = new StatusManager();
    public IStatusManager StatusManager => m_StatusManager;

    protected SkillCooldownTracker m_SkillCooldownTracker = new();
    #endregion

    #region Static Data
    public int CharacterSOInstanceID { get; protected set; }

    public WeaponAnimationType WeaponAnimationType { get; private set; }

    protected ClassSO m_Class;
    public string ClassName => m_Class.m_ClassName;

    public bool CanSwapTiles => m_Class.m_CanSwapTiles;
    public TileType[] TraversableTileTypes => m_Class.m_TraversableTileTypes;

    public string CharacterName { get; protected set; }
    public string DisplayName => !string.IsNullOrEmpty(CharacterName) ? $"{CharacterName} / {ClassName}" : ClassName;
    private AudioDataSO m_HurtSFX;
    private AudioDataSO m_DeathSFX;

    public Sprite Sprite { get; private set; }

    public Vector3 GridYOffset { get; private set; }

    private const float CHECKPOINT_MOVE_TIME = 0.5f;

    public virtual UnitAllegiance UnitAllegiance => UnitAllegiance.NONE;
    #endregion

    private WeaponInstanceSO m_EquippedWeapon;
    private readonly List<TokenStack> m_PermanentTokens = new();
    public IEnumerable<TokenStack> PermanentTokens => m_PermanentTokens;

    public List<WeaponModel> WeaponModels => m_ArmorVisual.WeaponModels;

    #region Initialisation
    private void Awake()
    {
        GlobalEvents.Battle.UnitDefeatedEvent += OnUnitDeath;
    }

    private void OnDestroy()
    {
        GlobalEvents.Battle.UnitDefeatedEvent -= OnUnitDeath;
    }

    private void OnUnitDeath(Unit defeatedUnit)
    {
        if (defeatedUnit.Equals(this))
            return;
        m_StatusManager.TryClearTauntToken(defeatedUnit);
    }

    protected void Initialise(Stats stats, RaceSO raceSO, ClassSO classSo, Sprite sprite, UnitModelData unitModelData, WeaponInstanceSO weaponInstanceSO, List<InflictedToken> permanentTokens)
    {
        m_ArmorVisual.InstantiateModel(unitModelData, weaponInstanceSO, classSo);
        m_EquippedWeapon = weaponInstanceSO;
        WeaponAnimationType = classSo.WeaponAnimationType;
        GridYOffset = new Vector3(0f, unitModelData.m_GridYOffset, 0f);

        // initialise permanent tokens first just in case there's something that affects max mana or max health
        InitialisePermanentTokens(weaponInstanceSO, permanentTokens);

        Sprite = sprite;
        m_Stats = stats;
        m_CurrHealth = GetTotalStat(StatType.HEALTH);
        m_CurrMana = GetTotalStat(StatType.MANA);
        m_Class = classSo;

        m_HurtSFX = raceSO.m_HurtSound;
        m_DeathSFX = raceSO.m_DeathSound;
    }

    private void InitialisePermanentTokens(WeaponInstanceSO weaponInstanceSO, List<InflictedToken> permanentTokens)
    {
        m_PermanentTokens.Clear();
        foreach (InflictedToken inflictedToken in weaponInstanceSO.m_PassiveTokens)
        {
            m_PermanentTokens.Add(new TokenStack(inflictedToken.m_TokenTierData, inflictedToken.m_Tier));
        }
        foreach (InflictedToken inflictedToken in permanentTokens)
        {
            m_PermanentTokens.Add(new TokenStack(inflictedToken.m_TokenTierData, inflictedToken.m_Tier));
        }
    }
    #endregion

    #region Rendering
    public void FadeMesh(float targetOpacity, float targetDuration)
    {
        m_ArmorVisual.FadeMesh(targetOpacity, targetDuration);
    }    
    #endregion

    #region Placement
    public virtual void PlaceUnit(CoordPair coordinates, Vector3 worldPosition)
    {
        m_CurrPosition = coordinates;
        transform.position = worldPosition + GridYOffset;
    }
    #endregion

    #region Health and Damage
    public float CurrentHealth => m_CurrHealth;
    public float MaxHealth => GetTotalStat(StatType.HEALTH);
    public float CurrentHealthProportion => m_CurrHealth / MaxHealth;

    public void Heal(float healAmount)
    {
        Logger.Log(this.GetType().Name, $"Unit {name} heals {healAmount}", name, this.gameObject, LogLevel.LOG);
        var max = MaxHealth;
        var value = Mathf.Min(max, m_CurrHealth + healAmount);
        var change = value - m_CurrHealth;
        m_CurrHealth = value;
        OnHealthChange?.Invoke(change, m_CurrHealth, max);

        DamageDisplayManager.Instance?.ShowDamage($"{change:F1}", new(0.25f, 1, 0.25f), transform);
    }

    void IHealth.SetHealth(float health)
    {
        var change = health - m_CurrHealth;
        m_CurrHealth = health;
        OnHealthChange?.Invoke(change, m_CurrHealth, MaxHealth);
    }

    public void TakeDamage(float damage)
    {
        Logger.Log(this.GetType().Name, $"Unit {name} took {damage} damage", name, this.gameObject, LogLevel.LOG);
        var value = Mathf.Max(0f, m_CurrHealth - damage);
        var change = value - m_CurrHealth;
        m_CurrHealth = value;
        OnHealthChange?.Invoke(change, value, MaxHealth);

        DamageDisplayManager.Instance?.ShowDamage($"{change:F1}", new(1, 0.25f, 0.25f), transform);
    }

    public event TrackedValueEvent OnHealthChange;
    #endregion

    #region Movement
    private Coroutine moveCoroutine;

    public void Move(CoordPair endPosition, Stack<Vector3> positionsToMoveThrough, VoidEvent onCompleteMovement)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        m_ArmorVisual.SetMoveAnimator(true);
        m_CurrPosition = endPosition;
        moveCoroutine = StartCoroutine(MoveThroughCheckpoints(positionsToMoveThrough, FinishMovement));

        void FinishMovement()
        {
            moveCoroutine = null;
            m_ArmorVisual.SetMoveAnimator(false);
            onCompleteMovement?.Invoke();
        }
    }

    private IEnumerator MoveThroughCheckpoints(Stack<Vector3> positionsToMoveThrough, VoidEvent onCompleteMovement)
    {
        Vector3 currDirectionVec = new Vector3(-1f, -1f, -1f);
        string currentParam = string.Empty;

        while (positionsToMoveThrough.Count > 0)
        {
            float time = 0f;
            Vector3 currPos = transform.position;
            Vector3 nextPos = positionsToMoveThrough.Pop() + GridYOffset;

            if (currPos == nextPos)
                continue;

            // TODO: Handle this better later
            Vector3 directionVec = (nextPos - currPos).normalized;
            Vector3 localVec = transform.InverseTransformVector(directionVec);
            m_ArmorVisual.SetDirAnim(ArmorVisual.DirXAnimParam, directionVec.x);
            m_ArmorVisual.SetDirAnim(ArmorVisual.DirYAnimParam, directionVec.z);
            print($"x: {m_ArmorVisual.GetDirAnim(ArmorVisual.DirXAnimParam)}; z: {m_ArmorVisual.GetDirAnim(ArmorVisual.DirYAnimParam)}");
            currDirectionVec = directionVec;
            while (time < CHECKPOINT_MOVE_TIME)
            {
                time += Time.deltaTime;
                float l = time / CHECKPOINT_MOVE_TIME;
                float x = Mathf.Lerp(currPos.x, nextPos.x, l);
                float y = Mathf.Lerp(currPos.y, nextPos.y, l);
                float z = Mathf.Lerp(currPos.z, nextPos.z, l);
                transform.position = new Vector3(x, y, z);
                yield return null;
            }
            transform.position = nextPos;
        }
        onCompleteMovement?.Invoke();
    }

    public void CancelMove()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
            m_ArmorVisual.SetMoveAnimator(false);
        }
    }
    #endregion

    #region Tick
    /// <summary>
    /// Tick at the start of the turn
    /// </summary>
    public void PreTick()
    {
        m_StatusManager.Tick(this);
    }

    /// <summary>
    /// Tick at the end of the turn
    /// </summary>
    public void PostTick()
    {
        m_SkillCooldownTracker.Tick();
    }
    #endregion

    #region Tokens
    /*
    public IEnumerable<Token> GetTokens(ConsumeType consumeType, TokenType tokenType)
    {
        return m_StatusManager.GetTokens(consumeType, tokenType);
    }
    */
    public bool HasToken(TokenType tokenType)
    {
        return m_StatusManager.HasTokenType(tokenType) || m_PermanentTokens.Any(x => x.TokenType == tokenType);
    }

    public void ConsumeTokens(TokenConsumptionType consumeType)
    {
        m_StatusManager.ConsumeTokens(consumeType);
    }

    public void ConsumeTokens(TokenType tokenType)
    {
        m_StatusManager.ConsumeTokens(tokenType);
    }

    public List<StatusEffect> GetInflictedStatusEffects(TokenConsumptionType consumeType)
    {
        List<StatusEffect> inflictedStatusEffects = m_StatusManager.GetInflictedStatusEffects(consumeType);
        foreach (TokenStack token in m_PermanentTokens)
        {
            if (token.TryGetInflictedStatusEffect(out StatusEffect statusEffect))
                inflictedStatusEffects.Add(statusEffect);
        }
        return inflictedStatusEffects;
    }
    #endregion

    #region Stats
    public Stats Stat => m_Stats;

    /// <summary>
    /// Returned total stat accounts for all buffs and debuffs
    /// </summary>
    /// <returns></returns>
    public Stats GetTotalStats()
    {
        return new Stats(GetTotalStat(StatType.HEALTH), GetTotalStat(StatType.MANA), GetTotalStat(StatType.PHYS_ATTACK), GetTotalStat(StatType.MAG_ATTACK), GetTotalStat(StatType.PHYS_DEFENCE), GetTotalStat(StatType.MAG_DEFENCE), GetTotalStat(StatType.SPEED), (int)GetTotalStat(StatType.MOVEMENT_RANGE));
    }

    // for preview purposes: ADD THE WEAPON STUFF
    public float GetFlatStatChange(StatType statType)
    {
        float flatStatChange = m_StatusManager.GetFlatStatChange(statType);
        foreach (TokenStack tokenStack in m_PermanentTokens)
        {
            flatStatChange += tokenStack.GetFlatStatChange(statType);
        }
        return flatStatChange;
    }

    // for preview purposes
    public float GetMultStatChange(StatType statType)
    {
        float multStatChange = m_StatusManager.GetMultStatChange(statType);
        foreach (TokenStack tokenStack in m_PermanentTokens)
        {
            multStatChange *= tokenStack.GetMultStatChange(statType);
        }
        return multStatChange;
    }

    public float GetBaseAttackModifier()
    {
        Debug.Log($"Current Weapon: {m_EquippedWeapon.m_WeaponName}");
        return m_EquippedWeapon.m_BaseAttackModifier;
    }

    public float GetBaseHealModifier()
    {
        return m_EquippedWeapon.m_BaseHealModifier;
    }

    public float GetTotalStat(StatType statType, float externalBaseModifier = 1f)
    {
        return (m_Stats.GetStat(statType) * externalBaseModifier + GetFlatStatChange(statType)) * GetMultStatChange(statType);
    }
    #endregion

    #region Damage Modifier
    public float GetFinalCritProportion()
    {
        float critProportion = m_StatusManager.GetCritAmount();
        foreach (TokenStack tokenStack in m_PermanentTokens)
        {
            critProportion *= tokenStack.GetFinalCritProportion();
        }
        return critProportion;
    }
    #endregion

    #region Status
    public bool CanPerformTurn()
    {
        return !HasToken(TokenType.STUN);
    }

    public bool HasReflect()
    {
        return HasToken(TokenType.REFLECT);
    }

    public float GetReflectProportion()
    {
        float reflectProportion = m_StatusManager.GetReflectProportion();
        foreach (TokenStack tokenStack in m_PermanentTokens)
        {
            reflectProportion += tokenStack.GetReflectProportion();
        }
        return reflectProportion;
    }

    public float GetLifestealProportion()
    {
        float lifestealProportion = m_StatusManager.GetLifestealProportion();
        foreach (TokenStack tokenStack in m_PermanentTokens)
        {
            lifestealProportion += tokenStack.GetLifestealProportion();
        }
        return lifestealProportion;
    }

    public void InflictStatus(StatusEffect statusEffect)
    {
        m_StatusManager.AddEffect(statusEffect);
        DamageDisplayManager.Instance?.ShowDamage($"<sprite name=\"{statusEffect.Icon.name}\" tint>", statusEffect.Color, transform);
    }

    public void InflictStatus(List<StatusEffect> statusEffects)
    {
        var sb = new StringBuilder();
        var color = Color.clear;
        foreach (StatusEffect statusEffect in statusEffects)
        {
            m_StatusManager.AddEffect(statusEffect);
            sb.Append($"<sprite name=\"{statusEffect.Icon.name}\" color=#{ColorUtility.ToHtmlStringRGB(statusEffect.Color)}> ");
            color += statusEffect.Color;
        }
        color /= statusEffects.Count;
        DamageDisplayManager.Instance?.ShowDamage(sb.ToString(), color, transform);
    }

    public bool IsTaunted(out Unit forceTarget)
    {
        return m_StatusManager.IsTaunted(out forceTarget);
    }

    public bool CanEvade()
    {
        return HasToken(TokenType.EVADE);
    }

    private void Cleanse(List<StatusType> statusTypes)
    {
        m_StatusManager.CleanseStatusTypes(statusTypes);
    }
    #endregion

    #region Token
    public void InflictToken(InflictedToken token, Unit inflicter)
    {
        m_StatusManager.AddToken(token, inflicter);
        DamageDisplayManager.Instance?.ShowDamage($"<sprite name=\"{token.m_TokenTierData.m_Icon.name}\" tint>", token.m_TokenTierData.m_Color, transform);
    }

    public void InflictTokens(List<InflictedToken> tokens, Unit inflicter)
    {
        var sb = new StringBuilder();
        var color = Color.clear;
        foreach (InflictedToken token in tokens)
        {
            m_StatusManager.AddToken(token, inflicter);
            sb.Append($"<sprite name=\"{token.m_TokenTierData.m_Icon.name}\" color=#{ColorUtility.ToHtmlStringRGB(token.m_TokenTierData.m_Color)}> ");
            color += token.m_TokenTierData.m_Color;
        }
        color /= tokens.Count;
        DamageDisplayManager.Instance?.ShowDamage(sb.ToString(), color, transform);
    }
    #endregion

    #region Mana
    public float CurrentMana => m_CurrMana;
    public float MaxMana => GetTotalStat(StatType.MANA);
    public float CurrentManaProportion => m_CurrMana / MaxMana;

    public bool HasEnoughManaForSkill(ActiveSkillSO skill)
    {
        return CurrentMana >= skill.m_ConsumedMana;
    }

    private void AlterMana(float amount)
    {
        Logger.Log(this.GetType().Name, $"Add {amount} mana to {name}", name, this.gameObject, LogLevel.LOG);
        var max = MaxMana;
        var value = Mathf.Clamp(m_CurrMana + amount, 0f, max);
        var change = value - m_CurrMana;
        m_CurrMana = value;
        OnManaChange?.Invoke(change, value, max);
    }

    public event TrackedValueEvent OnManaChange;
    #endregion

    #region Death
    public event VoidEvent OnDeath;
    public bool IsDead => m_CurrHealth <= 0;

    public void Die()
    {
        OnDeath?.Invoke();
        m_ArmorVisual.Die(null);
        m_UnitMarker.SetActive(false);
        GlobalEvents.Battle.UnitDefeatedEvent?.Invoke(this);
    }
    #endregion

    #region Attack Animations
    public void PlayAnimations(int triggerID)
    {
        m_ArmorVisual.PlayAnimations(triggerID);
    }

    public void PlaySkillStartAnimation(int skillID)
    {
        m_ArmorVisual.PlaySkillStartAnimation(skillID);
    }

    public void PlaySkillExecuteAnimation()
    {
        m_ArmorVisual.PlaySkillExecuteAnimation();
    }

    public void CancelSkillAnimation()
    {
        m_ArmorVisual.CancelSkillAnimation();
    }
    #endregion

    #region Skills
    public abstract IEnumerable<ActiveSkillSO> GetActiveSkills();

    public void PerformSkill(ActiveSkillSO attackSO, List<IHealth> targets)
    {
        GlobalEvents.Battle.CompleteAttackAnimationEvent += CompleteAttackAnimationEvent;
        GlobalEvents.Battle.AttackAnimationEvent?.Invoke(attackSO, this, targets.Select(x => (Unit) x).ToList());

        m_SkillCooldownTracker.UtiliseSkill(attackSO);

        void CompleteAttackAnimationEvent()
        {
            GlobalEvents.Battle.CompleteAttackAnimationEvent -= CompleteAttackAnimationEvent;
            PostSkillEvent?.Invoke();
        }
    }

    public void ApplySkillEffects(ActiveSkillSO skill, List<Unit> targets)
    {
        float dealtDamage = 0f;

        List<StatusEffect> inflictedStatusEffects = new();

        if (skill.IsPhysicalAttack)
            inflictedStatusEffects.AddRange(GetInflictedStatusEffects(TokenConsumptionType.CONSUME_ON_PHYS_ATTACK));
        else if (skill.IsMagicAttack)
            inflictedStatusEffects.AddRange(GetInflictedStatusEffects(TokenConsumptionType.CONSUME_ON_MAG_ATTACK));

        if (skill.ContainsSkillType(SkillEffectType.DEALS_STATUS_OR_TOKENS))
            inflictedStatusEffects.AddRange(skill.m_InflictedStatusEffects.Select(x => new StatusEffect(x.m_StatusEffect, x.m_Stack)));

        List<InflictedToken> inflictedTokens = skill.m_InflictedTokens;

        bool isOpposingSideTarget = skill.IsOpposingSideTarget;

        foreach (Unit target in targets)
        {
            // completely evade the effects // TODO: This does not handle animations
            if (isOpposingSideTarget && target.CanEvade())
            {
                target.ConsumeTokens(TokenType.EVADE);
                continue;
            }

            if (skill.DealsDamage)
            {
                float damage = DamageCalc.CalculateDamage(this, target, skill);
                dealtDamage += damage;
                target.TakeDamage(damage);

                if (target.HasReflect())
                {
                    TakeDamage(damage * target.GetReflectProportion());
                }
                target.ConsumeTokens(skill.IsMagic ? TokenConsumptionType.CONSUME_ON_MAG_DEFEND : TokenConsumptionType.CONSUME_ON_PHYS_DEFEND);
            }

            if (skill.IsHeal)
            {
                target.Heal(DamageCalc.CalculateHealAmount(this, skill));
            }

            if (skill.ContainsSkillType(SkillEffectType.ALTER_MANA))
            {
                target.AlterMana(DamageCalc.CalculateManaAlterAmount(this, skill));
            }

            if (!target.IsDead)
            {
                target.InflictStatus(inflictedStatusEffects);
                target.InflictTokens(inflictedTokens, this);
            }

            if (skill.ContainsSkillType(SkillEffectType.CLEANSE))
            {
                target.Cleanse(skill.m_CleansedStatusTypes);
            }
        }

        if (!IsDead)
        {
            AlterMana(-skill.m_ConsumedMana);

            if (skill.DealsDamage && HasToken(TokenType.LIFESTEAL))
                Heal(GetLifestealProportion() * dealtDamage);

            if (skill.IsMagicAttack)
                ConsumeTokens(TokenConsumptionType.CONSUME_ON_MAG_ATTACK);
            else if (skill.IsPhysicalAttack)
                ConsumeTokens(TokenConsumptionType.CONSUME_ON_PHYS_ATTACK);

            if (skill.IsHeal)
                ConsumeTokens(TokenConsumptionType.CONSUME_ON_HEAL);

            if (skill.ContainsSkillType(SkillEffectType.ALTER_MANA))
                ConsumeTokens(TokenConsumptionType.CONSUME_ON_MANA_ALTER);

            if (skill.IsSelfTarget)
            {
                ConsumeTokens(TokenConsumptionType.CONSUME_ON_SELF_TARGET);
            }
            else if (skill.IsOpposingSideTarget)
            {
                ConsumeTokens(TokenConsumptionType.CONSUME_ON_OPPOSING_TARGET);
            }
            else
            {
                ConsumeTokens(TokenConsumptionType.CONSUME_ON_ALLY_TARGET);
            }
        }
    }

    public bool CanPerformSkill(ActiveSkillSO activeSkillSO)
    {
        return m_SkillCooldownTracker.CanUtiliseSkill(activeSkillSO);
    }

    public int GetSkillCooldown(ActiveSkillSO activeSkillSO)
    {
        return m_SkillCooldownTracker.GetCooldown(activeSkillSO);
    }

    public float GetSkillCooldownProportion(ActiveSkillSO activeSkillSO)
    {
        return m_SkillCooldownTracker.GetCooldownProportion(activeSkillSO);
    }

    public VoidEvent PostSkillEvent;
    #endregion

    #region SFX
    public void PlayHurtSound(float volumeModifier = 1f)
    {
        SoundManager.Instance.Play(m_HurtSFX, volumeModifier);
    }

    public void PlayDeathSound(float volumeModifier = 1f)
    {
        SoundManager.Instance.Play(m_DeathSFX, volumeModifier);
    }
    #endregion
}
