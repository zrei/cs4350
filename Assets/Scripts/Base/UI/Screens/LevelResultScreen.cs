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

        public override void Initialize()
        {
            base.Initialize();
            GlobalEvents.Level.LevelEndEvent += OnLevelEnd;
        }

        private void OnDestroy()
        {
            GlobalEvents.Level.LevelEndEvent -= OnLevelEnd;
        }
        
        private void OnLevelEnd(LevelResultType result)
        {
            m_ResultText.text = result switch
            {
                LevelResultType.SUCCESS => "Level Completed!",
                LevelResultType.DEFEAT => "Defeat...",
                LevelResultType.OUT_OF_TIME => "Out of time...",
                _ => "???"
            };

            m_ReturnButton.onClick.AddListener(ReturnFromLevel);
        }
        
        public void ReturnFromLevel()
        {
            UIScreenManager.Instance.CloseScreen();
            GlobalEvents.Level.ReturnFromLevelEvent?.Invoke();
            m_ReturnButton.onClick.RemoveListener(ReturnFromLevel);
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
