using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public float m_GridYOffset;
    public SkinnedMeshRenderer[] m_AttachItems;

    public UnitModelData(GameObject model, SkinnedMeshRenderer[] attachItems, float gridYOffset)
    {
        m_Model = model;
        m_AttachItems = attachItems;
        m_GridYOffset = gridYOffset;
    }
}

public delegate void TrackedValueEvent(float change, float current, float max);

public abstract class Unit : MonoBehaviour, IHealth, ICanAttack, IFlatStatChange, IMultStatChange, ICritModifier, ITauntTarget
{
    #region Animation
    private static readonly int DirXAnimParam = Animator.StringToHash("DirX");
    private static readonly int DirYAnimParam = Animator.StringToHash("DirY");
    private static readonly int IsMoveAnimParam = Animator.StringToHash("IsMove");

    private static readonly int AttackStartAnimParam = Animator.StringToHash("AttackStart");
    private static readonly int AttackIDAnimParam = Animator.StringToHash("AttackID");

    private static readonly int PoseIDAnimParam = Animator.StringToHash("PoseID");

    public static readonly int HurtAnimParam = Animator.StringToHash("Hurt");
    private static readonly int DeathAnimParam = Animator.StringToHash("IsDead");
 
    private Animator m_Animator;
    #endregion

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
    // public IStatusManager StatusManager => m_StatusManager;
    #endregion

    #region Static Data
    public WeaponAnimationType WeaponAnimationType {get; private set;}

    protected ClassSO m_Class;
    public string ClassName => m_Class.m_ClassName;

    public bool CanSwapTiles => m_Class.m_CanSwapTiles;
    public TileType[] TraversableTileTypes => m_Class.m_TraversableTileTypes;

    public Sprite Sprite {get; private set;}

    public Vector3 GridYOffset {get; private set;}

    private const float CHECKPOINT_MOVE_TIME = 0.5f;

    public virtual UnitAllegiance UnitAllegiance => UnitAllegiance.NONE;
    #endregion

    private WeaponInstanceSO m_EquippedWeapon;
    private WeaponModel weaponModel;

    #region Initialisation
    protected void Initialise(Stats stats, ClassSO classSo, Sprite sprite, UnitModelData unitModelData, WeaponInstanceSO weaponInstanceSO)
    {
        Sprite = sprite;
        m_Stats = stats;
        m_CurrHealth = m_Stats.m_Health;
        m_CurrMana = m_Stats.m_Mana;
        m_Class = classSo;

        InstantiateModel(unitModelData, weaponInstanceSO, classSo);
    }

    /// <summary>
    /// Helps to instantiate the correct base model, and equip it with the class' equipment + weapons
    /// </summary>
    /// <param name="unitModelData"></param>
    /// <param name="weaponSO"></param>
    private void InstantiateModel(UnitModelData unitModelData, WeaponInstanceSO weaponSO, ClassSO classSO)
    {
        GameObject model = Instantiate(unitModelData.m_Model, Vector3.zero, Quaternion.identity, this.transform);
        EquippingArmor equipArmor = model.GetComponent<EquippingArmor>();
        equipArmor.Initialize(unitModelData.m_AttachItems);
        
        m_EquippedWeapon = weaponSO;
        WeaponModel weaponModelPrefab = m_EquippedWeapon.m_WeaponModel;
        if (weaponModelPrefab != null)
        {
            weaponModel = Instantiate(weaponModelPrefab);
            var attachPoint = weaponModel.attachmentType switch
            {
                WeaponModelAttachmentType.RIGHT_HAND => equipArmor.RightArmBone,
                WeaponModelAttachmentType.LEFT_HAND => equipArmor.LeftArmBone,
                _ => null,
            };
            weaponModel.transform.SetParent(attachPoint, false);
        }

        m_Animator = model.GetComponentInChildren<Animator>();
        if (m_Animator == null)
        {
            Logger.Log(this.GetType().Name, this.name, "No animator found!", this.gameObject, LogLevel.WARNING);
        }

        WeaponAnimationType = classSO.WeaponAnimationType;
        m_Animator.SetInteger(PoseIDAnimParam, (int)WeaponAnimationType);

        GridYOffset = new Vector3(0f, unitModelData.m_GridYOffset, 0f);
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

    public void Heal(float healAmount)
    {
        Logger.Log(this.GetType().Name, $"Unit {name} heals {healAmount}", name, this.gameObject, LogLevel.LOG);
        var max = MaxHealth;
        var value = Mathf.Min(max, m_CurrHealth + healAmount);
        var change = value - m_CurrHealth;
        m_CurrHealth = value;
        OnHealthChange?.Invoke(change, m_CurrHealth, max);
    }

    void IHealth.SetHealth(float health)
    {
        var change = health - m_CurrHealth;
        m_CurrHealth = health;
        OnHealthChange?.Invoke(change, m_CurrHealth, MaxHealth);
    }

    public void TakeDamage(float damage)
    {
        // if ()
        Logger.Log(this.GetType().Name, $"Unit {name} took {damage} damage", name, this.gameObject, LogLevel.LOG);
        var value = Mathf.Max(0f, m_CurrHealth - damage);
        var change = value - m_CurrHealth;
        m_CurrHealth = value;
        OnHealthChange?.Invoke(change, value, MaxHealth);
    }

    public event TrackedValueEvent OnHealthChange;
    #endregion

    #region Movement
    public void Move(CoordPair endPosition, Stack<Vector3> positionsToMoveThrough, VoidEvent onCompleteMovement)
    {
        m_Animator.SetBool(IsMoveAnimParam, true);
        StartCoroutine(MoveThroughCheckpoints(positionsToMoveThrough, FinishMovement));
        m_CurrPosition = endPosition;

        void FinishMovement()
        {
            ConsumeTokens(TokenConsumptionType.CONSUME_ON_MOVE);
            m_Animator.SetBool(IsMoveAnimParam, false);
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
            m_Animator.SetFloat(DirXAnimParam, directionVec.x);
            m_Animator.SetFloat(DirYAnimParam, directionVec.z);
            print($"x: {m_Animator.GetFloat(DirXAnimParam)}; z: {m_Animator.GetFloat(DirYAnimParam)}");
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
    #endregion

    #region Tick
    public void Tick()
    {
        m_StatusManager.Tick(this);
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
        return m_StatusManager.HasTokenType(tokenType);
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
        return m_StatusManager.GetInflictedStatusEffects(consumeType);
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
        return new Stats(GetTotalStat(StatType.HEALTH), GetTotalStat(StatType.MANA), GetTotalStat(StatType.PHYS_ATTACK), GetTotalStat(StatType.MAG_ATTACK), GetTotalStat(StatType.PHYS_DEFENCE), GetTotalStat(StatType.MAG_DEFENCE), GetTotalStat(StatType.SPEED), (int) GetTotalStat(StatType.MOVEMENT_RANGE));
    }

    // for preview purposes: ADD THE WEAPON STUFF
    public float GetFlatStatChange(StatType statType)
    {
        return m_StatusManager.GetFlatStatChange(statType);
    }

    // for preview purposes
    public float GetMultStatChange(StatType statType)
    {
        return m_StatusManager.GetMultStatChange(statType);
    }

    public float GetBaseAttackModifier()
    {
        return m_EquippedWeapon.m_BaseAttackModifier;
    }

    public float GetBaseHealModifier()
    {
        return m_EquippedWeapon.m_BaseHealModifier;
    }

    public float GetTotalStat(StatType statType, float externalBaseModifier = 1f)
    {
        return (m_Stats.GetStat(statType) * externalBaseModifier + m_StatusManager.GetFlatStatChange(statType)) * m_StatusManager.GetMultStatChange(statType);
    }
    #endregion

    #region Damage Modifier
    public float GetFinalCritProportion()
    {
        return m_StatusManager.GetCritAmount();
    }
    #endregion

    #region Status
    public bool CanPerformTurn()
    {
        return !m_StatusManager.IsStunned();
    }

    public bool HasReflect()
    {
        return m_StatusManager.CanReflect();
    }

    public float GetReflectProportion()
    {
        return m_StatusManager.GetReflectProportion();
    }

    public void InflictStatus(StatusEffect statusEffect)
    {
        m_StatusManager.AddEffect(statusEffect);
    }

    public void InflictStatus(List<StatusEffect> statusEffects)
    {
        foreach (StatusEffect statusEffect in statusEffects)
            InflictStatus(statusEffect);
    }

    public bool IsTaunted(out Unit forceTarget)
    {
        return m_StatusManager.IsTaunted(out forceTarget);
    }

    public bool CanEvade()
    {
        return m_StatusManager.CanEvade();
    }
    #endregion

    #region Token
    public void InflictToken(InflictedToken token, Unit inflicter)
    {
        m_StatusManager.AddToken(token, inflicter);
    }

    public void InflictTokens(List<InflictedToken> tokens, Unit inflicter)
    {
        foreach (InflictedToken token in tokens)
            InflictToken(token, inflicter);
    }
    #endregion

    #region Mana
    public float CurrentMana => m_CurrMana;
    public float MaxMana => GetTotalStat(StatType.MANA);

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
        m_Animator.SetBool(DeathAnimParam, true);
        Destroy(gameObject, 2f);
    }
    #endregion

    #region Attack Animations
    public void PlayAnimations(int triggerID)
    {
        m_Animator.SetTrigger(triggerID);
    }

    public void PlaySkillAnimation(int attackID)
    {
        m_Animator.SetInteger(AttackIDAnimParam, attackID);
        m_Animator.SetTrigger(AttackStartAnimParam);
        weaponModel.PlayAttackAnimation();
    }
    #endregion

    #region Skills
    public void PerformSkill(ActiveSkillSO attackSO, List<IHealth> targets)
    {
        float dealtDamage = 0f;

        List<StatusEffect> inflictedStatusEffects = new();
        
        if (attackSO.IsPhysicalAttack)
            inflictedStatusEffects.AddRange(GetInflictedStatusEffects(TokenConsumptionType.CONSUME_ON_PHYS_ATTACK));
        else if (attackSO.IsMagicAttack)
            inflictedStatusEffects.AddRange(GetInflictedStatusEffects(TokenConsumptionType.CONSUME_ON_MAG_ATTACK));

        if (attackSO.ContainsSkillType(SkillEffectType.DEALS_STATUS_OR_TOKENS))
            inflictedStatusEffects.AddRange(attackSO.m_InflictedStatusEffects.Select(x => new StatusEffect(x.m_StatusEffect, x.m_Stack)));
        
        List<InflictedToken> inflictedTokens = attackSO.m_InflictedTokens;

        GlobalEvents.Battle.CompleteAttackAnimationEvent += CompleteAttackAnimationEvent;
        GlobalEvents.Battle.AttackAnimationEvent?.Invoke(attackSO, this, targets.Select(x => (Unit) x).ToList());

        bool isOpposingSideTarget = attackSO.IsOpposingSideTarget;

        foreach (Unit target in targets)
        {
            // completely evade the effects // TODO: This does not handle animations
            if (isOpposingSideTarget && target.CanEvade())
            {
                target.ConsumeTokens(TokenType.EVADE);
                continue;
            }

            if (attackSO.DealsDamage)
            {
                float damage = DamageCalc.CalculateDamage(this, target, attackSO);
                dealtDamage += damage;
                target.TakeDamage(damage);

                if (target.HasReflect())
                {
                    TakeDamage(damage * target.GetReflectProportion());
                }
                target.ConsumeTokens(attackSO.IsMagic ? TokenConsumptionType.CONSUME_ON_MAG_DEFEND : TokenConsumptionType.CONSUME_ON_PHYS_DEFEND);
            }
            
            if (attackSO.IsHeal)
            {
                target.Heal(DamageCalc.CalculateHealAmount(this, attackSO));
            }

            if (attackSO.ContainsSkillType(SkillEffectType.ALTER_MANA))
            {
                target.AlterMana(this.GetTotalStat(StatType.MAG_ATTACK) * attackSO.m_ManaAlterProportion);
            }
            
            if (!target.IsDead)
            {
                target.InflictStatus(inflictedStatusEffects);
                target.InflictTokens(inflictedTokens, this);
            }
        }

        if (!IsDead)
        {
            AlterMana(-attackSO.m_ConsumedMana);

            if (attackSO.DealsDamage && HasToken(TokenType.LIFESTEAL))
                Heal(m_StatusManager.GetLifestealProportion() * dealtDamage);

            if (attackSO.IsMagicAttack)
                ConsumeTokens(TokenConsumptionType.CONSUME_ON_MAG_ATTACK);
            else if (attackSO.IsPhysicalAttack)
                ConsumeTokens(TokenConsumptionType.CONSUME_ON_PHYS_ATTACK);

            if (attackSO.IsHeal)
                ConsumeTokens(TokenConsumptionType.CONSUME_ON_HEAL);
            
            if (attackSO.ContainsSkillType(SkillEffectType.ALTER_MANA))
                ConsumeTokens(TokenConsumptionType.CONSUME_ON_MANA_ALTER);
        }

        void CompleteAttackAnimationEvent()
        {
            GlobalEvents.Battle.CompleteAttackAnimationEvent -= CompleteAttackAnimationEvent;
            PostSkillEvent?.Invoke();
        }
    }

    public VoidEvent PostSkillEvent;
    #endregion
}
