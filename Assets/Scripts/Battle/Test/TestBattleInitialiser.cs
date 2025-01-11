using System.Collections.Generic;
using System.Linq;
using Game;
using UnityEngine;

/// <summary>
/// Test script to initialise a once-off battle in place of the level manager when testing.
/// </summary>
public class TestBattleInitialiser : MonoBehaviour
{
    [Header("Battle Data")]
    [SerializeField] private List<PlayerCharacterData> m_TestData;
    [SerializeField] private BattleSO m_TestBattle;

    [Header("Fatigue Tokens")]
    [SerializeField] private InflictedToken m_FatigueToken;
    [SerializeField] private bool m_ApplyFatigueTokens = false;

    private void Start()
    {
        for (int i = 0; i < m_TestData.Count; i++)
        {
            m_TestData[i].m_CurrStats = m_TestData[i].m_BaseData.m_StartingStats;
            m_TestData[i].m_CurrClassIndex = m_TestData[i].m_BaseData.m_PathGroup.GetDefaultClassIndex();
        }

        List<InflictedToken> inflictedTokens = new();
        if (m_ApplyFatigueTokens)
        {
            inflictedTokens.Add(m_FatigueToken);
        }

        int maxLevel = Mathf.Max(m_TestData.Select(x => x.m_CurrLevel).ToArray());

        if (BattleManager.IsReady)
            BattleManager.Instance.InitialiseBattle(m_TestBattle, m_TestData.Select(x => x.GetBattleData()).ToList(), maxLevel, inflictedTokens);
        else 
            LevelManager.OnReady += () => BattleManager.Instance.InitialiseBattle(m_TestBattle, m_TestData.Select(x => x.GetBattleData()).ToList(), maxLevel, inflictedTokens);

        if (CameraManager.IsReady)
            CameraManager.Instance.SetUpBattleCamera();
        else
            CameraManager.OnReady += () => CameraManager.Instance.SetUpBattleCamera();
        
        GlobalEvents.Scene.OnSceneTransitionCompleteEvent?.Invoke(SceneEnum.LEVEL, SceneEnum.BATTLE);
    }
}
