using System.Collections.Generic;
using UnityEngine;
using Game.Input;

public class PlayerUnitSetup : MonoBehaviour
{
    private MapLogic m_MapLogic;
    private bool m_IsSettingUp = false;
    private List<CoordPair> m_PlayerSquares;

    private CoordPair m_TileToSwap;
    private bool m_HasSelectedTile = false;

    private VoidEvent m_CompleteSetupEvent;

    public void Initialise(MapLogic mapLogic, VoidEvent completeSetupEvent)
    {
        m_MapLogic = mapLogic;
        m_CompleteSetupEvent = completeSetupEvent;
    }

    public void BeginSetup(List<CoordPair> playerBeginningSquares)
    {
        Logger.Log(this.GetType().Name, "Begin player unit set up", LogLevel.LOG);
        m_PlayerSquares = playerBeginningSquares;
        m_IsSettingUp = true;
        GlobalEvents.Battle.PlayerUnitSetupStartEvent?.Invoke();

        InputManager.Instance.EndTurn.OnPressEvent += OnEndTurn;
        InputManager.Instance.PointerSelect.OnPressEvent += OnPointerSelect;
    }

    private void OnEndTurn(IInput input)
    {
        Logger.Log(this.GetType().Name, "Complete player unit set up", LogLevel.LOG);
        m_IsSettingUp = false;
        m_CompleteSetupEvent?.Invoke();

        InputManager.Instance.EndTurn.OnPressEvent -= OnEndTurn;
        InputManager.Instance.PointerSelect.OnPressEvent -= OnPointerSelect;
    }

    private void OnPointerSelect(IInput input)
    {
        var inputVector = InputManager.Instance.PointerPosition.GetValue<Vector2>();
        Vector3 mousePos = new Vector3(inputVector.x, inputVector.y, Camera.main.nearClipPlane);
        bool hasHitGrid = m_MapLogic.TryRetrieveTile(Camera.main.ScreenPointToRay(mousePos), out CoordPair targetTile, out GridType gridType);

        if (!hasHitGrid || gridType != GridType.PLAYER || !m_PlayerSquares.Contains(targetTile))
            return;

        if (m_HasSelectedTile && targetTile.Equals(m_TileToSwap))
            return;

        if (m_HasSelectedTile)
        {
            m_MapLogic.SwapTiles(GridType.PLAYER, m_TileToSwap, targetTile);
            m_HasSelectedTile = false;
            Logger.Log(this.GetType().Name, $"Swap {m_TileToSwap} with {targetTile}", LogLevel.LOG);
        }
        else
        {
            m_HasSelectedTile = true;
            m_TileToSwap = targetTile;
            Logger.Log(this.GetType().Name, $"Select initial tile {m_TileToSwap}", LogLevel.LOG);
        }
    }
}