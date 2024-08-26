using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Stats
{
    public float m_Health;
    public float m_Attack;
    public float m_Speed;
}

public enum UnitAllegiance
{
    PLAYER,
    ENEMY,
    NONE
}

public abstract class Unit : MonoBehaviour, IHealth
{
    private float m_Health;
    public bool IsDead => m_Health <= 0;
    private Stats m_stats;
    public Stats Stat => m_stats;

    public virtual UnitAllegiance UnitAllegiance => UnitAllegiance.NONE;

    private const float CHECKPOINT_MOVE_TIME = 0.5f;

    void IHealth.Heal(float healAmount)
    {

    }

    void IHealth.SetHealth(float health)
    {

    }

    void IHealth.TakeDamage(float damage)
    {

    }

    public void Move(Stack<Vector3> positionsToMoveThrough)
    {
        StartCoroutine(MoveThroughCheckpoints(positionsToMoveThrough));
    }

    private IEnumerator MoveThroughCheckpoints(Stack<Vector3> positionsToMoveThrough)
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
    }
}