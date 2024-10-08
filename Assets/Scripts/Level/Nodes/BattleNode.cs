using UnityEngine;

public class BattleNode : NodeInternal
{
    [SerializeField] private BattleSO m_BattleSO;
    public BattleSO BattleSO => m_BattleSO;
    
    private UnitAllegiance m_Victor;
    private int m_NumTurns;
    
    public override void StartNodeEvent()
    {
        Debug.Log("Starting Battle Node");
        GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
        GlobalEvents.Battle.ReturnFromBattleEvent += OnReturnFromBattle;
        GlobalEvents.Level.BattleNodeStartEvent?.Invoke(this);
    }

    private void OnBattleEnd(UnitAllegiance victor, int numTurns)
    {
        // Save the battle result
        m_Victor = victor;
        m_NumTurns = numTurns;
        
        // Remove the event listener
        GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
    }
    
    private void OnReturnFromBattle()
    {
        GlobalEvents.Battle.ReturnFromBattleEvent -= OnReturnFromBattle;
        GlobalEvents.Level.BattleNodeEndEvent?.Invoke(this, m_Victor, m_NumTurns);
    }
}
