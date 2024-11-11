using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class StatusOverviewDisplay : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI noStatusText;

        [SerializeField]
        private StatusDisplay statusDisplay;

        [SerializeField]
        private PassiveSkillDisplay_Reclass passiveSkillDisplay;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            Hide();
        }

        public void DisplayUnitStatuses(PlayerCharacterData playerCharacterData)
        {
            var regularStatuses = new List<IStatus>();
            var permanentStatuses = new List<IStatus>();

            var hasStatus = false;

            var characterTokens = playerCharacterData.GetBattleData().GetInflictedTokens(
                MoralityManager.IsReady ? MoralityManager.Instance.CurrMoralityPercentage : 0f);
            foreach (var token in characterTokens)
            {
                permanentStatuses.Add(token);
                hasStatus = true;
            }

            var classTokens = playerCharacterData.CurrClass.GetInflictedTokens(playerCharacterData.m_CurrLevel);
            foreach (var token in classTokens)
            {
                permanentStatuses.Add(token);
                hasStatus = true;
            }

            if (LevelRationsManager.IsReady)
            {
                var hungerTokens = LevelRationsManager.Instance.GetInflictedTokens();
                foreach (var token in hungerTokens)
                {
                    permanentStatuses.Add(token);
                    hasStatus = true;
                }
            }

            statusDisplay.SetStatuses(regularStatuses, permanentStatuses);

            noStatusText.gameObject.SetActive(!hasStatus);

            passiveSkillDisplay.SetDisplay(playerCharacterData, playerCharacterData.CurrClass);
        }

        public void Hide()
        {
            canvasGroup ??= GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0;
        }

        public void Show()
        {
            canvasGroup ??= GetComponent<CanvasGroup>();
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1;
        }
    }
}
