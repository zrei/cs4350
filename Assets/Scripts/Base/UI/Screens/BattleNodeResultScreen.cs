using Game.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class BattleNodeResultScreen : BaseUIScreen
    {
        [SerializeField] TextMeshProUGUI m_TitleText;
        [SerializeField] TextMeshProUGUI m_TimeTakenText;
        [SerializeField] TextMeshProUGUI m_ResultText;
        [SerializeField] Button m_ReturnButton;

        public override void Initialize()
        {
            base.Initialize();
            GlobalEvents.Level.BattleNodeEndEvent += OnBattleNodeEnd;
        }

        private void OnDestroy()
        {
            GlobalEvents.Level.BattleNodeEndEvent -= OnBattleNodeEnd;
        }

        private void OnBattleNodeEnd(BattleNode battleNode, UnitAllegiance victor, int numTurns)
        {
            if (victor == UnitAllegiance.PLAYER)
            {
                var expReward = battleNode.BattleSO.m_ExpReward;
                
                m_TitleText.text = "Victory!";
                m_TimeTakenText.text = $"Time taken: {numTurns}";
                m_ResultText.text = $"Gained {expReward} EXP!";
            }
            else
            {
                m_TitleText.text = "Defeat...";
                m_TimeTakenText.text = $"Time taken: {numTurns}";
                m_ResultText.text = "";
            }
            
            m_ReturnButton.onClick.AddListener(CloseResults);
        }
    
        private void CloseResults()
        {
            UIScreenManager.Instance.CloseScreen();
            GlobalEvents.Level.CloseRewardScreenEvent?.Invoke();
            m_ReturnButton.onClick.RemoveListener(CloseResults);
        }
        
        public override void ScreenUpdate()
        {
        }
        
        public override void OnCancel(IInput input)
        {
            CloseResults();
        }
    }
}
