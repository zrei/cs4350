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

    void IHealth.Heal(float healAmount)
    {

    }

    void IHealth.SetHealth(float health)
    {

    }

    void IHealth.TakeDamage(float damage)
    {

    }
}