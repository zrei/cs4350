using System.Collections;
using UnityEngine.UI;
using UnityEngine;

namespace Game.UI
{
    public class ExpDisplay : MonoBehaviour
    {
        [SerializeField] FormattedTextDisplay m_CharacterName;
        [SerializeField] ProgressBar m_ProgressBar;
        [SerializeField] Image m_LevelUpArrow;

        private const float TRANSITION_TIME = 1.0f;

        public void SetDisplay(ExpGainSummary expGainInfo, VoidEvent onCompleteAnimation = null)
        {
            m_LevelUpArrow.gameObject.SetActive(false);
            m_CharacterName?.SetValue(expGainInfo.m_CharacterSO.m_CharacterName);
            
            if (expGainInfo.m_InitialLevel == LevellingManager.Instance.MaxLevel)
            {
                m_ProgressBar.SetValue(1f, 1f, 0f);
                onCompleteAnimation?.Invoke();
                return;
            }

            StartCoroutine(PerformAnimation(expGainInfo, onCompleteAnimation));
        }

        private IEnumerator PerformAnimation(ExpGainSummary expGainInfo, VoidEvent onCompleteAnimation = null)
        {
            int currLevel = expGainInfo.m_InitialLevel;
            int initialExp = expGainInfo.m_FinalExp - expGainInfo.m_ExpGrowth - LevellingManager.Instance.GetExpToNextLevel(currLevel);
            int totalExp = LevellingManager.Instance.GetExpToNextLevel(currLevel + 1) - LevellingManager.Instance.GetExpToNextLevel(currLevel);
            
            int gainedLevels = expGainInfo.m_FinalLevel - expGainInfo.m_InitialLevel;
            int targetExp = gainedLevels <= 0 ? expGainInfo.m_FinalExp - LevellingManager.Instance.GetExpToNextLevel(currLevel) : totalExp;

            float indivTransitionTime = TRANSITION_TIME / (gainedLevels + 1);

            for (int i = gainedLevels; i >= 0; --i)
            {
                // set initial bar
                m_ProgressBar.SetValue(initialExp, totalExp, 0f);
                m_ProgressBar.SetValue(targetExp, totalExp, indivTransitionTime);
                Debug.Log(targetExp / totalExp);
                yield return new WaitForSeconds(indivTransitionTime);

                if (i >= 1)
                    m_LevelUpArrow.gameObject.SetActive(true);

                ++currLevel;
                initialExp = 0;
                targetExp = i == 1 ? expGainInfo.m_FinalExp - LevellingManager.Instance.GetExpToNextLevel(currLevel) : LevellingManager.Instance.GetExpToNextLevel(currLevel);
                Debug.Log(targetExp);
            }

            onCompleteAnimation?.Invoke();
        }
    }
}