using Game.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum LevelResultType
{
    SUCCESS,
    DEFEAT,
    OUT_OF_TIME
}

namespace Game.UI
{
    public class LevelResultScreen : BaseUIScreen
    {
        [SerializeField] TextMeshProUGUI m_ResultText;
        [SerializeField] Button m_ReturnButton;
        
        private int currentLevelId;

        public override void Initialize()
        {
            base.Initialize();
            GlobalEvents.Level.LevelEndEvent += OnLevelEnd;
        }

        private void OnDestroy()
        {
            GlobalEvents.Level.LevelEndEvent -= OnLevelEnd;
        }
        
        private void OnLevelEnd(int levelId, LevelResultType result)
        {
            m_ResultText.text = result switch
            {
                LevelResultType.SUCCESS => "Level Completed!",
                LevelResultType.DEFEAT => "Defeat...",
                LevelResultType.OUT_OF_TIME => "Out of time...",
                _ => "???"
            };

            currentLevelId = levelId;
            m_ReturnButton.onClick.AddListener(ReturnFromLevel);
        }
        
        public void ReturnFromLevel()
        {
            UIScreenManager.Instance.CloseScreen();
            m_ReturnButton.onClick.RemoveListener(ReturnFromLevel);
            GameSceneManager.Instance.UnloadLevelScene(currentLevelId);
        }
        
        public override void ScreenUpdate()
        {
        }
        
        public override void OnCancel(IInput input)
        {
            ReturnFromLevel();
        }
    }
}
