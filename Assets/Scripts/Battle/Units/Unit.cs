using System.Collections;
using System.Collections.Generic;
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
    public float m_Attack;
    public float m_Speed;
    public int m_MovementRange;
    public TileType[] m_TraversableTileTypes;
}

public enum UnitAllegiance
{
    PLAYER,
    ENEMY,
    NONE
}

// TODO: Store position here so we don't have to keep raycasting :|
public abstract class Unit : MonoBehaviour, IHealth
{
    // current health
    private float m_Health;
    public bool IsDead => m_Health <= 0;

    private Stats m_Stats;
    public Stats Stat => m_Stats;

    public virtual UnitAllegiance UnitAllegiance => UnitAllegiance.NONE;

    private const float CHECKPOINT_MOVE_TIME = 0.5f;

    private CoordPair m_CurrPosition;
    public CoordPair CurrPosition => m_CurrPosition;

    void IHealth.Heal(float healAmount)
    {

    }

    void IHealth.SetHealth(float health)
    {

    }

    /*
    void IHealth.TakeDamage(float damage)
    {

    }
    */

    public void TakeDamage(float damage)
    {
        m_Health -= damage;
    }

    public void Move(CoordPair endPosition, Stack<Vector3> positionsToMoveThrough, VoidEvent onCompleteMovement)
    {
        StartCoroutine(MoveThroughCheckpoints(positionsToMoveThrough, onCompleteMovement));
        m_CurrPosition = endPosition;
    }

    private IEnumerator MoveThroughCheckpoints(Stack<Vector3> positionsToMoveThrough, VoidEvent onCompleteMovement)
    {
        while (positionsToMoveThrough.Count > 0)
        {
            float time = 0f;
            Vector3 currPos = transform.position;
            Vector3 nextPos = positionsToMoveThrough.Pop();
            if (currPos == nextPos)
                continue;
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

    /// <summary>
    /// Initialise stats, position, etc.
    /// </summary>
    public virtual void Initialise(Stats stats)
    {
        m_Stats = stats;
        m_Health = stats.m_Health;
    }
}