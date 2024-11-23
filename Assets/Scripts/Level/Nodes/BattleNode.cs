using System.Collections.Generic;
using UnityEngine;

public class BattleNodePreviewData : NodePreviewData
{
    public List<EnemyUnitPlacement> EnemyUnits;
    public List<ObjectiveSO> Objectives;
}
    
public class BattleNode : NodeInternal
{
    [SerializeField] private BattleSO m_BattleSO;
    public BattleSO BattleSO => m_BattleSO;
    
    private UnitAllegiance m_Victor;
    private int m_NumTurns;
    
    public override NodePreviewData GetNodePreviewData()
    {
        return new BattleNodePreviewData
        {
            NodeName = NodeInfo.m_NodeName,
            NodeDescription = NodeInfo.m_NodeDescription,
            IsMoralityLocked = IsMoralityLocked,
            MoralityThreshold = MoralityThreshold,
            EnemyUnits = m_BattleSO.m_EnemyUnitsToSpawn,
            Objectives = m_BattleSO.m_Objectives
        };
    }

    protected override void PerformNode(VoidEvent postEvent = null)
    {
        GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
        GlobalEvents.Level.BattleNodeStartEvent?.Invoke(this);
    }

    private void OnDestroy()
    {
        GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
    }

    private void OnBattleEnd(UnitAllegiance victor, int numTurns)
    {
        // Save the battle result
        m_Victor = victor;
        m_NumTurns = numTurns;
        
        // Remove the event listener
        GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;

        GlobalEvents.Scene.OnSceneTransitionCompleteEvent += OnSceneLoad;
    }
    
    private void OnSceneLoad(SceneEnum fromScene, SceneEnum toScene)
    {
        if (toScene != SceneEnum.LEVEL)
            return;

        GlobalEvents.Scene.OnSceneTransitionCompleteEvent -= OnSceneLoad;

        GlobalEvents.Level.BattleNodeEndEvent?.Invoke(this, m_Victor, m_NumTurns);
    }

    public void PostTutorial(VoidEvent postEvent)
    {
        if (!m_HasPlayedPostTutorial)
        {
            m_HasPlayedPostTutorial = true;
            PlayTutorial(m_PostTutorial, postEvent);
        }
        else
        {
            postEvent?.Invoke();
        }
    }
}
