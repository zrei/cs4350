using System.Text;
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
        [SerializeField] GameObject m_ResultPanel;
        [SerializeField] TextMeshProUGUI m_ResultText;
        [SerializeField] SelectableBase m_ReturnButton;
        
        [SerializeField] GameObject m_RewardsPanel;
        [SerializeField] TextMeshProUGUI m_RewardsText;
        [SerializeField] SelectableBase m_RewardsReturnButton;
        
        private int currentLevelId;

        public override void Initialize()
        {
            base.Initialize();
            GlobalEvents.Level.LevelResultsEvent += OnLevelEnd;
        }

        private void OnDestroy()
        {
            GlobalEvents.Level.LevelResultsEvent -= OnLevelEnd;
        }
        
        private void OnLevelEnd(LevelSO levelSo, LevelResultType result)
        {
            m_ResultText.text = result switch
            {
                LevelResultType.SUCCESS => "Level Completed!",
                LevelResultType.DEFEAT => "Defeat...",
                LevelResultType.OUT_OF_TIME => "Out of time...",
                _ => "???"
            };

            currentLevelId = levelSo.m_LevelId;
            
            bool hasRewards = levelSo.m_RewardCharacters.Count > 0 || levelSo.m_RewardWeapons.Count > 0;
            if (result == LevelResultType.SUCCESS && hasRewards)
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
                foreach (var rewardChar in levelSo.m_RewardCharacters)
                {
                    builder.AppendLine($"{rewardChar.m_CharacterName} has joined your party!");
                }
                
                builder.AppendLine();
                
                foreach (var rewardWeapon in levelSo.m_RewardWeapons)
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
