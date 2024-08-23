using UnityEngine;

public enum BattlePhase
{
    PLAYER,
    ENEMY
}

// may or may not become a singleton
[RequireComponent(typeof(PlayerTurnManager))]
[RequireComponent(typeof(EnemyTurnManager))]
public class BattleManager : MonoBehaviour
{
    private BattlePhase m_CurrPhase = BattlePhase.PLAYER;
    public BattlePhase CurrPhase => m_CurrPhase;

    private PlayerTurnManager m_PlayerTurnManager;
    private EnemyTurnManager m_EnemyTurnManager;

    private void Start()
    {
        m_PlayerTurnManager = GetComponent<PlayerTurnManager>();
        m_EnemyTurnManager = GetComponent<EnemyTurnManager>();
    }

    
}