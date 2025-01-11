using System.Collections.Generic;
using System.Linq;
using Game;
using UnityEngine;

/// <summary>
/// Test script to initialise a once-off battle in place of the level manager when testing.
/// </summary>
public class TestBattleInitialiser : TestSceneInitialiser
{
    [Header("Additional Battle Data")]
    [SerializeField] private BattleSO m_TestBattle;

    [Header("Fatigue Tokens")]
    [SerializeField] private InflictedToken m_FatigueToken;
    [SerializeField] private bool m_ApplyFatigueTokens = false;

    protected override void Initialise()
    {
        List<PlayerCharacterBattleData> finalData = new();
        foreach (TestCharacterData testCharacterData in m_TestCharacterData)
        {
            int? weaponId = null;
            if (testCharacterData.m_OverrideWeapon)
            {
                weaponId = InventoryManager.Instance.ObtainWeapon(testCharacterData.m_OverriddenWeaponInstance);
            }
            finalData.Add(testCharacterData.GetPlayerCharacterBattleData(weaponId));
        }

        List<InflictedToken> inflictedTokens = new();
        if (m_ApplyFatigueTokens)
        {
            inflictedTokens.Add(m_FatigueToken);
        }

        int maxLevel = Mathf.Max(m_TestCharacterData.Select(x => x.m_CurrLevel).ToArray());

        if (BattleManager.IsReady)
            BattleManager.Instance.InitialiseBattle(m_TestBattle, finalData, maxLevel, inflictedTokens);
        else 
            BattleManager.OnReady += () => BattleManager.Instance.InitialiseBattle(m_TestBattle, finalData, maxLevel, inflictedTokens);

        if (CameraManager.IsReady)
            CameraManager.Instance.SetUpBattleCamera();
        else
            CameraManager.OnReady += () => CameraManager.Instance.SetUpBattleCamera();
        
        GlobalEvents.Scene.OnSceneTransitionCompleteEvent?.Invoke(SceneEnum.LEVEL, SceneEnum.BATTLE);
    }
}
