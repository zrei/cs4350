using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class TurnQueue
{
    #region Configuration
    private const float DISTANCE_THRESHOLD = 50f;
    private const float TICK_AMOUNT = 1f;
    #endregion

    #region Turn
    private class TurnWrapper
    {
        public float m_TimeRemaining;
        public Unit m_Unit;

        public TurnWrapper(float timeRemaining, Unit unit)
        {
            m_TimeRemaining = timeRemaining;
            m_Unit = unit;
        }

        public override string ToString()
        {
            return $"Unit {m_Unit.name} with time {m_TimeRemaining} remaining to act";
        }

        public void TickTime(float tickAmount)
        {
            m_TimeRemaining -= tickAmount;
        }
    }
    private List<TurnWrapper> m_Turns = new List<TurnWrapper>();
    #endregion

    public bool TryGetReadyUnit(out Unit readyUnit)
    {
        if (m_Turns.Count <= 0)
        {
            readyUnit = null;
            return false;
        }

        if (m_Turns[0].m_TimeRemaining == 0)
        {
            readyUnit = m_Turns[0].m_Unit;
            m_Turns.RemoveAt(0);
            return true;
        }
        else
        {
            readyUnit = null;
            return false;
        }
    }

    #region Edit Turn Queue
    public void RemoveUnit(Unit unit)
    {
        int idx = -1;
        for (int i = 0; i < m_Turns.Count; ++i)
        {
            if (m_Turns[i].m_Unit == unit)
            {
                idx = i;
                break;
            }
        }

        if (idx >= 0)
            m_Turns.RemoveAt(idx);
    }

    public void AddUnit(Unit unit)
    {
        m_Turns.Add(new TurnWrapper(DISTANCE_THRESHOLD / unit.Stat.m_Speed, unit));
    }

    public void Clear()
    {
        m_Turns.Clear();
    }
    #endregion

    public void OrderTurnQueue()
    {
        m_Turns.Sort(UnitSpeedComparer);
    }

    public void Tick()
    {
        if (m_Turns.Count <= 0)
            return;

        float tick = Mathf.Min(TICK_AMOUNT, m_Turns[0].m_TimeRemaining);
        foreach (TurnWrapper turnWrapper in m_Turns)
        {
            turnWrapper.TickTime(tick);
        }
    }

    #region Helper
    // TODO: Decide on timebreaker for units with the same time remaining
    private int UnitSpeedComparer(TurnWrapper unit1, TurnWrapper unit2)
    {
        return unit1.m_TimeRemaining.CompareTo(unit2.m_TimeRemaining); //unit1.Stat.m_Speed.CompareTo(unit2.Stat.m_Speed);
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder("Current state of the turn order:\n");
        foreach (TurnWrapper turnWrapper in m_Turns)
        {
            stringBuilder.Append(turnWrapper + "\n");
        }
        return stringBuilder.ToString();
    }

    public List<Unit> GetTurnOrder()
    {
        List<Unit> units = new() {};
        m_Turns.ForEach(x => units.Add(x.m_Unit));
        return units;
    }
    #endregion
}