using System.Collections;
using System.Collections.Generic;
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

// TODO: Store position here so we don't have to keep raycasting :|
public abstract class Unit : MonoBehaviour, IHealth, ICanAttack, IStatChange
{
    private const string DirXAnimParam = "DirX";
    private const string DirYAnimParam = "DirY";
    private const string IsMoveAnimParam = "IsMove";

    private static int SwordAttackAnimHash = Animator.StringToHash("SwordAttack");
    private static int MagicAttackAnimHash = Animator.StringToHash("MagicAttack");
    private static int MagicSupportAnimHash = Animator.StringToHash("MagicSupport");
    private static int DeathAnimHash = Animator.StringToHash("Death");
    private static int HurtAnimHash = Animator.StringToHash("Hurt");

    [SerializeField] Animator m_Animator;

    // current health
    private float m_Health;
    public bool IsDead => m_Health <= 0;

    /// <summary>
    /// This is used to store what their stats SHOULD be,
    /// accounting for base stats + growths + class bonuses so far.
    /// It DOES NOT account for any transient stat changes
    /// due to buffs/debuffs.
    /// </summary>
    private Stats m_Stats;
    public Stats Stat => m_Stats;

    public virtual UnitAllegiance UnitAllegiance => UnitAllegiance.NONE;

    private const float CHECKPOINT_MOVE_TIME = 0.5f;

    private CoordPair m_CurrPosition;
    public CoordPair CurrPosition => m_CurrPosition;

    protected StatusManager m_StatusManager = new StatusManager();

    #region Initialisation
    /// <summary>
    /// Initialise stats, position, etc.
    /// Called when the unit is first spawned onto the battlefield
    /// </summary>
    public virtual void Initialise(Stats stats)
    {
        m_Stats = stats;
        m_Health = stats.m_Health;
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

    void IHealth.Heal(float healAmount)
    {
        m_Health = Mathf.Min(m_Stats.m_Health, m_Health + healAmount);
    }

    void IHealth.SetHealth(float health)
    {
        m_Health = health;
    }

    // account for status conditions/inflicted tokens here
    public void TakeDamage(float damage)
    {
        m_Animator.Play(HurtAnimHash);
        m_Health -= damage;
    }
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

    public float GetTotalStat(StatType statType)
    {
        return (m_Stats.GetStat(statType) + m_StatusManager.GetFlatStatChange(statType)) * m_StatusManager.GetMultStatChange(statType);
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

    #region Death
    public void Die()
    {
        m_Animator.Play(DeathAnimHash);
        Destroy(gameObject, 1f);
    }
    #endregion

    #region Attack
    public void Attack(ActiveSkillSO attackSO, List<IHealth> targets)
    {
        List<StatusEffect> inflictedStatusEffects = GetInflictedStatusEffects(attackSO.m_AttackType == SkillType.PHYSICAL_ATTACK ? ConsumeType.CONSUME_ON_PHYS_ATTACK : ConsumeType.CONSUME_ON_MAG_ATTACK);
        m_Animator.Play(SwordAttackAnimHash);

        foreach (Unit target in targets)
        {
            target.TakeDamage(DamageCalc.CalculateDamage(this, target, attackSO));
            target.ClearTokens(attackSO.m_AttackType == SkillType.PHYSICAL_ATTACK ? ConsumeType.CONSUME_ON_PHYS_DEFEND : ConsumeType.CONSUME_ON_MAG_DEFEND);
            if (!target.IsDead)
            {
                target.InflictStatus(inflictedStatusEffects);
            }
        }

        ClearTokens(attackSO.m_AttackType == SkillType.PHYSICAL_ATTACK ? ConsumeType.CONSUME_ON_PHYS_ATTACK : ConsumeType.CONSUME_ON_MAG_ATTACK);
    }
    #endregion
}