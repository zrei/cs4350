using UnityEngine;

public class BattleNode : NodeInternal
{
    [SerializeField] private BattleSO m_BattleSO;
    public BattleSO BattleSO => m_BattleSO;
    
    public override void StartNodeEvent()
    {
        Debug.Log("Starting Battle Node");
        GlobalEvents.Level.BattleNodeStartEvent?.Invoke(this);
    }
}
