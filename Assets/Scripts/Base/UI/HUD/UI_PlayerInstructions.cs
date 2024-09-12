using TMPro;
using UnityEngine;

public class UI_PlayerInstructions : MonoBehaviour
{
    [SerializeField] GameObject m_InstructionGroup;
    [SerializeField] GameObject m_SwitchPhaseText;
    [SerializeField] TextMeshProUGUI m_PhaseText;
 
    private void Awake()
    {
        GlobalEvents.Battle.PlayerTurnStartEvent += OnPlayerTurnStart;
        GlobalEvents.Battle.EnemyTurnStartEvent += OnEnemyTurnStart;
        GlobalEvents.Battle.PlayerPhaseUpdateEvent += OnPlayerPhaseUpdate;
        GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
        GlobalEvents.Battle.PlayerUnitSetupStartEvent += OnPlayerUnitSetupStartEvent;
    }

    private void OnDestroy()
    {
        GlobalEvents.Battle.PlayerTurnStartEvent -= OnPlayerTurnStart;
        GlobalEvents.Battle.EnemyTurnStartEvent -= OnEnemyTurnStart;
        GlobalEvents.Battle.PlayerPhaseUpdateEvent -= OnPlayerPhaseUpdate;
        GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
        GlobalEvents.Battle.PlayerUnitSetupStartEvent -= OnPlayerUnitSetupStartEvent;
    }

    private void OnPlayerPhaseUpdate(PlayerTurnState currState)
    {
        m_PhaseText.text = currState switch
        {
            PlayerTurnState.SELECTING_MOVEMENT_SQUARE => "Move",
            PlayerTurnState.SELECTING_ACTION_TARGET => "Attack",
            _ => "Unknown"
        };
    }

    private void OnPlayerTurnStart()
    {
        TogglePlayerInstructions(true);
    }

    private void OnEnemyTurnStart()
    {
        TogglePlayerInstructions(false);
    }

    private void OnPlayerUnitSetupStartEvent()
    {
        TogglePlayerInstructions(true, false);
        m_PhaseText.text = "Unit setup";
    }

    private void TogglePlayerInstructions(bool isActive, bool displaySwitchPhaseText = true)
    {
        m_InstructionGroup.SetActive(isActive);
        m_SwitchPhaseText.SetActive(isActive && displaySwitchPhaseText);
    }

    private void OnBattleEnd(UnitAllegiance _)
    {
        gameObject.SetActive(false);
    }
}