using Game.Input;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public struct BattleResultUIData
    {
        public BattleSO BattleSO;
        public UnitAllegiance Victor;
        public bool IsSkipped;
        public int ExpReward;
        public int NumTurns;

        public BattleResultUIData(BattleSO battleSO, UnitAllegiance victor, bool isSkipped, int expReward, int numTurns)
        {
            BattleSO = battleSO;
            Victor = victor;
            IsSkipped = isSkipped;
            ExpReward = expReward;
            NumTurns = numTurns;
        }
    }

    public class NodeBattleResultScreen : BaseUIScreen
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

            ShowBattleNodeEnd((BattleResultUIData) args[0]);

            base.Show();
        }

        private void ShowBattleNodeEnd(BattleResultUIData battleResultUIData)
        {
            if (battleResultUIData.IsSkipped)
            {
                m_TitleText.text = "Battle Avoided!";
                m_ResultText.text = $"Gained {battleResultUIData.ExpReward} EXP!";
                m_GraphicGroup.color = ColorUtils.VictoryColor;
            }
            else if (battleResultUIData.Victor == UnitAllegiance.PLAYER)
            {
                m_TitleText.text = "Victory!";
                m_ResultText.text = $"Gained {battleResultUIData.ExpReward} EXP!";
                m_GraphicGroup.color = ColorUtils.VictoryColor;
            }
            else
            {
                m_TitleText.text = "Defeat...";
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
