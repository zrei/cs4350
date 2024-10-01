using UnityEngine;

public class BattleNode : NodeInternal
{
    [SerializeField] private BattleSO m_BattleSO;
    public BattleSO BattleSO => m_BattleSO;
    
    private UnitAllegiance m_Victor;
    
    public override void StartNodeEvent()
    {
        Debug.Log("Starting Battle Node");
        GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
        GlobalEvents.Battle.ReturnFromBattleEvent += OnReturnFromBattle;
        GlobalEvents.Level.BattleNodeStartEvent?.Invoke(this);
    }

    private void OnBattleEnd(UnitAllegiance victor)
    {
        // Save the battle result
        m_Victor = victor;
        
        // Remove the event listener
        GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
    }
    
    private void OnReturnFromBattle()
    {
        GlobalEvents.Battle.ReturnFromBattleEvent -= OnReturnFromBattle;
        GlobalEvents.Level.BattleNodeEndEvent?.Invoke(this, m_Victor);
    }
}
