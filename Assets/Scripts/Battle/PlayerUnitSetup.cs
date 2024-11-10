using Game.Input;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitSetup : MonoBehaviour
{
    private MapLogic m_MapLogic;
    private List<CoordPair> m_PlayerSquares;

    private TileVisual m_TileToSwap;

    private TileVisual selectedTileVisual;

    private VoidEvent m_CompleteSetupEvent;

    public bool IsSetupStarted { get; private set; }

    public void Initialise(MapLogic mapLogic, VoidEvent completeSetupEvent)
    {
        m_MapLogic = mapLogic;
        m_CompleteSetupEvent = completeSetupEvent;
    }

    public void BeginSetup(List<CoordPair> playerBeginningSquares)
    {
        Logger.Log(this.GetType().Name, "Begin player unit set up", LogLevel.LOG);
        m_PlayerSquares = playerBeginningSquares;

        m_MapLogic.onTileSelect += OnTileSelect;
        m_MapLogic.onTileSubmit += OnTileSubmit;
        m_MapLogic.ResetMap();
        m_MapLogic.ShowSetupTiles(GridType.PLAYER, m_PlayerSquares);
        m_MapLogic.ShowInspectable(GridType.ENEMY, true);

        IsSetupStarted = true;
        GlobalEvents.Battle.PlayerUnitSetupStartEvent?.Invoke();
    }

    public void EndSetup()
    {
        Logger.Log(this.GetType().Name, "Complete player unit set up", LogLevel.LOG);
        m_CompleteSetupEvent?.Invoke();
        GlobalEvents.Battle.PlayerUnitSetupEndEvent?.Invoke();

        GlobalEvents.Battle.PreviewUnitEvent(null);

        m_MapLogic.onTileSelect -= OnTileSelect;
        m_MapLogic.onTileSubmit -= OnTileSubmit;
        m_MapLogic.ResetMap();
    }

    private void OnTileSelect(TileData data, TileVisual visual)
    {
        if (selectedTileVisual != null && selectedTileVisual != m_TileToSwap)
        {
            selectedTileVisual.ToggleSwapTarget(false);
        }

        GlobalEvents.Battle.PreviewUnitEvent(data.m_CurrUnit);

        if (visual.GridType != GridType.PLAYER)
            return;

        selectedTileVisual = visual;
        if (selectedTileVisual != null && m_PlayerSquares.Contains(selectedTileVisual.Coordinates))
        {
            selectedTileVisual.ToggleSwapTarget(true);
        }
    }

    private void OnTileSubmit(TileData data, TileVisual visual)
    {
        if (visual == null) return;

        if (visual.GridType != GridType.PLAYER || !m_PlayerSquares.Contains(visual.Coordinates))
            return;

        if (visual.Equals(m_TileToSwap))
        {
            visual.ToggleSwapTarget(false);
            m_TileToSwap = null;
            return;
        }

        if (m_TileToSwap != null)
        {
            m_MapLogic.SwapTiles(GridType.PLAYER, m_TileToSwap.Coordinates, visual.Coordinates);
            m_TileToSwap.ToggleSwapTarget(false);
            visual.ToggleSwapTarget(false);
            m_TileToSwap = null;
            Logger.Log(this.GetType().Name, $"Swap {m_TileToSwap} with {visual}", LogLevel.LOG);
        }
        else
        {
            visual.ToggleSwapTarget(true);
            m_TileToSwap = visual;
            Logger.Log(this.GetType().Name, $"Select initial tile {m_TileToSwap}", LogLevel.LOG);
        }
    }
}