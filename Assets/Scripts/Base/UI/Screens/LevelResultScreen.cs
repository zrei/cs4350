using System.Text;
using Game.Input;
using TMPro;
using UnityEngine;

public enum LevelResultType
{
    SUCCESS,
    DEFEAT,
    OUT_OF_TIME
}

namespace Game.UI
{
    public struct LevelResultUIData
    {
        public LevelSO LevelSO;
        public LevelResultType LevelResultType;

        public LevelResultUIData(LevelSO levelSO, LevelResultType levelResultType)
        {
            LevelSO = levelSO;
            LevelResultType = levelResultType;
        }
    }

    public class LevelResultScreen : BaseUIScreen
    {
        [SerializeField] GraphicGroup m_GraphicGroup;

        [SerializeField] GameObject m_ResultPanel;
        [SerializeField] TextMeshProUGUI m_ResultText;
        [SerializeField] SelectableBase m_ReturnButton;
        
        [SerializeField] GameObject m_RewardsPanel;
        [SerializeField] TextMeshProUGUI m_RewardsText;
        [SerializeField] SelectableBase m_RewardsReturnButton;
        
        private int currentLevelId;

        public override void Show(params object[] args)
        {
            if (args.Length == 0)
                return;

            ShowLevelResult((LevelResultUIData) args[0]);

            base.Show();
        }

        private void ShowLevelResult(LevelResultUIData levelResultUIData)
        {
            m_GraphicGroup.color = levelResultUIData.LevelResultType switch
            {
                LevelResultType.SUCCESS => ColorUtils.VictoryColor,
                LevelResultType.DEFEAT => ColorUtils.DefeatColor,
                LevelResultType.OUT_OF_TIME => ColorUtils.DefeatColor,
                _ => Color.white
            };
            m_ResultText.text = levelResultUIData.LevelResultType switch
            {
                LevelResultType.SUCCESS => "Level Completed!",
                LevelResultType.DEFEAT => "Defeat...",
                LevelResultType.OUT_OF_TIME => "Out of time...",
                _ => "???"
            };

            currentLevelId = levelResultUIData.LevelSO.m_LevelId;
            
            bool hasRewards = levelResultUIData.LevelSO.m_RewardCharacters.Count > 0 || levelResultUIData.LevelSO.m_RewardWeapons.Count > 0;
            if (levelResultUIData.LevelResultType == LevelResultType.SUCCESS && hasRewards)
                m_ReturnButton.onSubmit.AddListener(ShowRewards);
            else
                m_ReturnButton.onSubmit.AddListener(ReturnFromLevel);
            
            m_ResultPanel.SetActive(true);
            m_RewardsPanel.SetActive(false);

            return;
            
            void ShowRewards()
            {
                m_ResultPanel.SetActive(false);

                var builder = new StringBuilder();
                foreach (var rewardChar in levelResultUIData.LevelSO.m_RewardCharacters)
                {
                    builder.AppendLine($"{rewardChar.m_CharacterName} has joined your party!");
                }
                
                builder.AppendLine();
                
                foreach (var rewardWeapon in levelResultUIData.LevelSO.m_RewardWeapons)
                {
                    builder.AppendLine($"Gained {rewardWeapon.m_WeaponName}!");
                }

                m_RewardsText.text = builder.ToString();
                m_RewardsReturnButton.onSubmit.AddListener(ReturnFromLevel);
                m_RewardsPanel.SetActive(true);
            }
        }
        
        private void ReturnFromLevel()
        {
            m_ReturnButton.onSubmit.RemoveListener(ReturnFromLevel);
            m_RewardsReturnButton.onSubmit.RemoveListener(ReturnFromLevel);
            
            UIScreenManager.Instance.CloseScreen();
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
