using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// This is used to store what their stats SHOULD be,
/// accounting for base stats + growths so far.
/// It DOES NOT account for any transient stat changes
/// due to buffs/debuffs.
/// TODO: Should account for class stats? Or just leave it
/// as separate
/// </summary>
[System.Serializable]
public struct Stats
{
    public float m_Health;
    public float m_Mana;
    public float m_PhysicalAttack;
    public float m_MagicAttack;
    public float m_PhysicalDefence;
    public float m_MagicDefence;
    public float m_Speed;
    public int m_MovementRange;
    public TileType[] m_TraversableTileTypes;
    public bool m_CanSwapTiles;
}

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
public abstract class Unit : MonoBehaviour, IHealth
{
    [SerializeField] Animator m_Animator;

    // current health
    private float m_Health;
    public bool IsDead => m_Health <= 0;

    // note that this should already have accounted for the class boost
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
        m_Animator.SetBool("IsHurt", true);
        m_Health -= damage;
    }
    #endregion

    #region Movement
    public void Move(CoordPair endPosition, Stack<Vector3> positionsToMoveThrough, VoidEvent onCompleteMovement)
    {
        m_Animator.SetBool("MoveForward", true);
        StartCoroutine(MoveThroughCheckpoints(positionsToMoveThrough, FinishMovement));
        m_CurrPosition = endPosition;

        void FinishMovement()
        {
            m_Animator.SetBool("MoveForward", false);
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
            if (directionVec != currDirectionVec)
            {
                if (!string.IsNullOrEmpty(currentParam))
                {
                    m_Animator.SetBool(currentParam, false);
                }

                if (directionVec == transform.forward)
                {
                    m_Animator.SetBool("MoveForward", true);
                }
                else if (directionVec == -transform.forward)
                {
                    m_Animator.SetBool("MoveBack", true);
                }
                else if (directionVec == transform.right)
                {
                    m_Animator.SetBool("MoveRight", true);
                }
                else
                {
                    m_Animator.SetBool("MoveLeft", true);
                }
            }
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
        m_Animator.SetBool(currentParam, false);
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
    #endregion

    #region Stats
    public float GetTotalMovementRange()
    {
        // account for buffs at some point
        return Stat.m_MovementRange;
    }

    // for preview purposes
    public Stats GetTotalStats()
    {
        return m_Stats;
    }

    public float GetFlatStatChange(StatType statType)
    {
        return m_StatusManager.GetFlatStatChange(statType);
    }

    public float GetMultStatChange(StatType statType)
    {
        return m_StatusManager.GetMultStatChange(statType);
    }
    #endregion

    #region Status
    public void InflictStatus(StatusEffect statusEffect)
    {
        m_StatusManager.AddEffect(statusEffect);
    }
    #endregion

    #region Death
    public void Die()
    {
        m_Animator.SetTrigger("IsDead");
        Destroy(gameObject, 1f);
    }
    #endregion
}