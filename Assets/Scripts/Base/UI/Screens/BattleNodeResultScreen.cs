using Game.Input;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public struct BattleNodeResultUIData
    {
        public BattleSO BattleSO;
        public UnitAllegiance Victor;
        public int NumTurns;

        public BattleNodeResultUIData(BattleSO battleSO, UnitAllegiance victor, int numTurns)
        {
            BattleSO = battleSO;
            Victor = victor;
            NumTurns = numTurns;
        }
    }

    public class BattleNodeResultScreen : BaseUIScreen
    {
        [SerializeField] TextMeshProUGUI m_TitleText;
        [SerializeField] TextMeshProUGUI m_TimeTakenText;
        [SerializeField] TextMeshProUGUI m_ResultText;
        [SerializeField] SelectableBase m_ReturnButton;
        [SerializeField] GraphicGroup m_GraphicGroup;

        public override void Show(params object[] args)
        {
            if (args.Length == 0)
                return;

            ShowBattleNodeEnd((BattleNodeResultUIData) args[0]);

            base.Show();
        }

        private void ShowBattleNodeEnd(BattleNodeResultUIData battleNodeResultUIData)
        {
            if (battleNodeResultUIData.Victor == UnitAllegiance.PLAYER)
            {
                var expReward = battleNodeResultUIData.BattleSO.m_ExpReward;
                
                m_TitleText.text = "Victory!";
                //m_TimeTakenText.text = $"Time taken: {numTurns}";
                m_ResultText.text = $"Gained {expReward} EXP!";
                m_GraphicGroup.color = ColorUtils.VictoryColor;
            }
            else
            {
                m_TitleText.text = "Defeat...";
                //m_TimeTakenText.text = $"Time taken: {numTurns}";
                m_ResultText.text = "";
                m_GraphicGroup.color = ColorUtils.DefeatColor;
            }
            
            m_ReturnButton.onSubmit.AddListener(CloseResults);
        }
    
        private void CloseResults()
        {
            UIScreenManager.Instance.CloseScreen();
            m_ReturnButton.onSubmit.RemoveListener(CloseResults);
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
