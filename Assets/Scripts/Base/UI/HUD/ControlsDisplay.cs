using UnityEngine;

namespace Game.UI
{
    public class ControlsDisplay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup m_LevelControls;
        [SerializeField] private CanvasGroup m_BattleControls;
        [SerializeField] private CanvasGroup m_WorldMapControls;

        private void Awake()
        {
            GlobalEvents.Scene.OnSceneTransitionEvent += ShowControls;
            
            HideAll();
        }
        
        private void OnDestroy()
        {
            GlobalEvents.Scene.OnSceneTransitionEvent -= ShowControls;
        }

        private void ShowControls(SceneEnum sceneEnum)
        {
            switch (sceneEnum)
            {
                case SceneEnum.WORLD_MAP:
                    ShowWorldMapControls();
                    break;
                case SceneEnum.LEVEL:
                    ShowLevelControls();
                    break;
                case SceneEnum.BATTLE:
                    ShowBattleControls();
                    break;
                default:
                    HideAll();
                    break;
            }
        }

        private void ShowLevelControls()
        {
            Show(m_LevelControls);
            Hide(m_BattleControls);
            Hide(m_WorldMapControls);
        }
        
        private void ShowBattleControls()
        {
            Show(m_BattleControls);
            Hide(m_LevelControls);
            Hide(m_WorldMapControls);
        }

        private void ShowWorldMapControls()
        {
            Show(m_WorldMapControls);
            Hide(m_LevelControls);
            Hide(m_BattleControls);
        }

        private void HideAll()
        {
            Hide(m_WorldMapControls);
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