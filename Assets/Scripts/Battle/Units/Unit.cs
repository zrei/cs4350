using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
public struct AnimationState
{
    public string 
}
*/

public enum UnitAllegiance
{
    PLAYER,
    ENEMY,
    NONE
}

public delegate void TrackedValueEvent(float change, float current, float max);

// TODO: Store position here so we don't have to keep raycasting :|
public abstract class Unit : MonoBehaviour, IHealth, ICanAttack, IStatChange
{
    private const string DirXAnimParam = "DirX";
    private const string DirYAnimParam = "DirY";
    private const string IsMoveAnimParam = "IsMove";

    public static readonly int SwordAttackAnimHash = Animator.StringToHash("SwordAttack");
    public static readonly int MagicAttackAnimHash = Animator.StringToHash("MagicAttack");
    public static readonly int MagicSupportAnimHash = Animator.StringToHash("MagicSupport");
    public static readonly int HurtAnimHash = Animator.StringToHash("Hurt");
    private static readonly int DeathAnimHash = Animator.StringToHash("Death");

    [SerializeField] Animator m_Animator;

    // current health
    protected float m_Health;
    public bool IsDead => m_Health <= 0;

    protected float m_Mana;
    public float RemainingMana => m_Mana;

    /// <summary>
    /// This is used to store what their stats SHOULD be,
    /// accounting for base stats + growths + class bonuses so far.
    /// It DOES NOT account for any transient stat changes
    /// due to buffs/debuffs.
    /// </summary>
    protected Stats m_Stats;
    public Stats Stat => m_Stats;

    public virtual UnitAllegiance UnitAllegiance => UnitAllegiance.NONE;

    private const float CHECKPOINT_MOVE_TIME = 0.5f;

    private CoordPair m_CurrPosition;
    public CoordPair CurrPosition => m_CurrPosition;

    protected StatusManager m_StatusManager = new StatusManager();

    protected ClassSO m_Class;

    public VoidEvent PostAttackEvent;

    #region Initialisation
    protected void Initialise(Stats stats, ClassSO classSo/*, GameObject baseModel*/)
    {
        m_Stats = stats;
        m_Health = m_Stats.m_Health;
        m_Mana = m_Stats.m_Mana;
        m_Class = classSo;
    }
    #endregion

    #region Placement
    public virtual void PlaceUnit(CoordPair coordinates, Vector3 worldPosition)
    {
        m_CurrPosition = coordinates;
        transform.position = worldPosition;
    }
    #endregion

    #region Health and Damage
    public float CurrentHealth => m_Health;
    public float MaxHealth => GetTotalStat(StatType.HEALTH);

    public void Heal(float healAmount)
    {
        var max = MaxHealth;
        var value = Mathf.Min(max, m_Health + healAmount);
        var change = value - m_Health;
        m_Health = value;
        OnHealthChange?.Invoke(change, m_Health, max);
    }

    void IHealth.SetHealth(float health)
    {
        var change = health - m_Health;
        m_Health = health;
        OnHealthChange?.Invoke(change, m_Health, MaxHealth);
    }

    // account for status conditions/inflicted tokens here
    public void TakeDamage(float damage)
    {
        Logger.Log(this.GetType().Name, $"Unit {name} took {damage} damage", name, this.gameObject, LogLevel.LOG);
        var value = Mathf.Max(0f, m_Health - damage);
        var change = value - m_Health;
        m_Health = value;
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
            Vector3 nextPos = positionsToMoveThrough.Pop();

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
    public IEnumerable<Token> GetTokens(ConsumeType consumeType, TokenType tokenType)
    {
        return m_StatusManager.GetTokens(consumeType, tokenType);
    }

    public void ClearTokens(ConsumeType consumeType)
    {
        m_StatusManager.ClearTokens(consumeType);
    }

    public List<StatusEffect> GetInflictedStatusEffects(ConsumeType consumeType)
    {
        return m_StatusManager.GetInflictedStatusEffects(consumeType);
    }
    #endregion

    #region Stats
    // for preview purposes, account for buffs!
    public Stats GetTotalStats()
    {
        return m_Stats;
    }

    // for preview purposes
    public float GetFlatStatChange(StatType statType)
    {
        return m_StatusManager.GetFlatStatChange(statType);
    }

    // for preview purposes
    public float GetMultStatChange(StatType statType)
    {
        return m_StatusManager.GetMultStatChange(statType);
    }

    public float GetTotalStat(StatType statType, float baseModifier = 1f)
    {
        return (m_Stats.GetStat(statType) * baseModifier + m_StatusManager.GetFlatStatChange(statType)) * m_StatusManager.GetMultStatChange(statType);
    }
    #endregion

    #region Status
    public void InflictStatus(StatusEffect statusEffect)
    {
        m_StatusManager.AddEffect(statusEffect);
    }

    public void InflictStatus(List<StatusEffect> statusEffects)
    {
        foreach (StatusEffect statusEffect in statusEffects)
            InflictStatus(statusEffect);
    }
    #endregion

    #region Token
    public void InflictToken(Token token)
    {
        m_StatusManager.AddToken(token);
    }

    public void InflictTokens(List<Token> tokens)
    {
        foreach (Token token in tokens)
            InflictToken(token);
    }
    #endregion

    #region Mana
    public float CurrentMana => m_Mana;
    public float MaxMana => GetTotalStat(StatType.MANA);

    private void AlterMana(float amount)
    {
        Logger.Log(this.GetType().Name, $"Add {amount} mana to {name}", name, this.gameObject, LogLevel.LOG);
        var max = MaxMana;
        var value = Mathf.Clamp(m_Mana + amount, 0f, max);
        var change = value - m_Mana;
        m_Mana = value;
        OnManaChange?.Invoke(change, value, max);
    }

    public event TrackedValueEvent OnManaChange;
    #endregion

    #region Death
    public void Die()
    {
        PlayAnimations(DeathAnimHash);
        Destroy(gameObject, 1f);
    }
    #endregion

    #region Attack Animations
    public void PlayAnimations(int animationId)
    {
        m_Animator.Play(animationId);
    }

    public void PlayAttackAnimation(bool isSupport)
    {
        if (isSupport)
        {
            // support for other weapon types???
            PlayAnimations(MagicSupportAnimHash);
        }

        switch (m_Class.m_Weapon.m_WeaponType)
        {
            case WeaponType.SWORD:
            case WeaponType.AXE:
            case WeaponType.LANCE:
            case WeaponType.BOW:
                PlayAnimations(SwordAttackAnimHash);
                break;
            case WeaponType.MAGIC:
                PlayAnimations(MagicAttackAnimHash);
                break;
        }
    }
    #endregion

    #region Skills
    public void PerformSKill(ActiveSkillSO attackSO, List<IHealth> targets)
    {
        List<StatusEffect> inflictedStatusEffects = new();
        
        if (attackSO.IsPhysicalAttack)
            inflictedStatusEffects.AddRange(GetInflictedStatusEffects(ConsumeType.CONSUME_ON_PHYS_ATTACK));
        else if (attackSO.IsMagicAttack)
            inflictedStatusEffects.AddRange(GetInflictedStatusEffects(ConsumeType.CONSUME_ON_MAG_ATTACK));

        inflictedStatusEffects.AddRange(attackSO.m_InflictedStatusEffects.Select(x => new StatusEffect(x.m_StatusEffect, x.m_Stack)));
        
        List<Token> inflictedTokens = attackSO.m_InflictedTokens;

        GlobalEvents.Battle.CompleteAttackAnimationEvent += CompleteAttackAnimationEvent;
        GlobalEvents.Battle.AttackAnimationEvent?.Invoke(attackSO, this, targets.Select(x => (Unit) x).ToList());

        foreach (Unit target in targets)
        {
            if (attackSO.DealsDamage)
            {
                target.TakeDamage(DamageCalc.CalculateDamage(this, target, attackSO));
                target.ClearTokens(attackSO.IsMagic ? ConsumeType.CONSUME_ON_MAG_DEFEND : ConsumeType.CONSUME_ON_PHYS_DEFEND);
            }
            else if (attackSO.ContainsAttackType(SkillType.HEAL_SUPPORT))
                target.Heal(attackSO.m_Amount);
            
            if (!target.IsDead)
            {
                target.InflictStatus(inflictedStatusEffects);
                target.InflictTokens(inflictedTokens);
            }
        }

        if (attackSO.IsMagic)
            AlterMana(- ((MagicActiveSkillSO) attackSO).m_ConsumedManaAmount);

        if (attackSO.IsMagicAttack)
            ClearTokens(ConsumeType.CONSUME_ON_MAG_ATTACK);
        else if (attackSO.IsPhysicalAttack)
            ClearTokens(ConsumeType.CONSUME_ON_PHYS_ATTACK);

        void CompleteAttackAnimationEvent()
        {
            GlobalEvents.Battle.CompleteAttackAnimationEvent -= CompleteAttackAnimationEvent;
            PostAttackEvent?.Invoke();
        }
    }
    #endregion
}
