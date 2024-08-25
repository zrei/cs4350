using System.Collections.Generic;
using UnityEngine;

public enum PlayerTurnState
{
    SELECTING_MOVEMENT_SQUARE,
    SELECTING_ATTACK_TARGET
}

public class PlayerTurnManager : TurnManager
{
    private PlayerTurnState m_CurrState = PlayerTurnState.SELECTING_MOVEMENT_SQUARE;
    // handle units

    private bool m_WithinTurn = false;
    private PlayerUnit m_CurrUnit;
    private CoordPair m_CurrTile;
    public void PerformTurn(PlayerUnit playerUnit)
    {
        Logger.Log(this.GetType().Name, "Start turn with: " + playerUnit.name, LogLevel.LOG);
        m_WithinTurn = true;
        m_CurrUnit = playerUnit;
        m_MapLogic.RetrieveClickedTile(new Ray(m_CurrUnit.transform.position, -m_CurrUnit.transform.up), out m_CurrTile, out GridType _);
        Logger.Log(this.GetType().Name, "Tile unit is on: " + m_CurrTile, LogLevel.LOG);
        m_CurrState = PlayerTurnState.SELECTING_MOVEMENT_SQUARE;
    }

    private void Update()
    {
        if (!m_WithinTurn)
            return;

        //Debug.Log("WHEEE!");

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            EndTurn();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("<3");
            if (m_MapLogic.RetrieveClickedTile(Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane)), out CoordPair coordPair, out GridType gridType) && m_CurrState == PlayerTurnState.SELECTING_MOVEMENT_SQUARE)
            {
                Debug.Log("???");
                m_MapLogic.MoveUnit(GridType.PLAYER, m_CurrUnit, m_CurrTile, coordPair);
                m_CurrState = PlayerTurnState.SELECTING_ATTACK_TARGET;
                EndTurn();
            }
        }
    }

    private void EndTurn()
    {
        m_WithinTurn = false;
        m_CompleteTurnEvent?.Invoke();
    }
}