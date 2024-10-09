using Game.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class BattleNodeResultScreen : BaseUIScreen
    {
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

        private void OnBattleNodeEnd(BattleNode battleNode, UnitAllegiance victor)
        {
            if (victor != UnitAllegiance.PLAYER) return;
        
            var expReward = battleNode.BattleSO.m_ExpReward;
        
            m_ResultText.text = $"Gained {expReward} EXP!";
            
            m_ReturnButton.onClick.AddListener(CloseResults);
        }
    
        private void CloseResults()
        {
            GlobalEvents.Level.CloseRewardScreenEvent?.Invoke();
            UIScreenManager.Instance.CloseScreen();
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
