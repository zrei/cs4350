using System;
using UnityEngine;

namespace Game.UI
{
    public class ControlsDisplay : MonoBehaviour
    {

        [SerializeField] private CanvasGroup m_LevelControls;
        [SerializeField] private CanvasGroup m_BattleControls;

        private void Awake()
        {
            GlobalEvents.Scene.LevelSceneLoadedEvent += ShowLevelControls;
            GlobalEvents.Scene.BattleSceneLoadedEvent += ShowBattleControls;
            GlobalEvents.Battle.ReturnFromBattleEvent += ShowLevelControls;
        }
        
        private void OnDestroy()
        {
            GlobalEvents.Scene.LevelSceneLoadedEvent -= ShowLevelControls;
            GlobalEvents.Scene.BattleSceneLoadedEvent -= ShowBattleControls;
            GlobalEvents.Battle.ReturnFromBattleEvent -= ShowLevelControls;
        }

        private void ShowLevelControls()
        {
            Show(m_LevelControls);
            Hide(m_BattleControls);
        }
        
        private void ShowBattleControls()
        {
            Show(m_BattleControls);
            Hide(m_LevelControls);
        }

        private void Hide()
        {
            Hide(m_LevelControls);
            Hide(m_BattleControls);
        }

        private void Hide(CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        private void Show(CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
}