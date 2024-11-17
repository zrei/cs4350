using System.Collections;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BattleResultDisplay : MonoBehaviour
    {
        private const float TransitionDuration = 0.5f;

        [SerializeField] GraphicGroup graphicGroup;
        [SerializeField] TextMeshProUGUI m_ResultText;
        [SerializeField] SelectableBase m_ReturnButton;

        private CanvasGroup m_CanvasGroup;
        private UIFader m_UIFader;

        private void Awake()
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
            m_UIFader = new(m_CanvasGroup);

            m_ReturnButton.onSubmit.AddListener(ReturnFromBattle);

            GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
        }

        private void OnDestroy()
        {
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
        }

        private void OnBattleEnd(UnitAllegiance victor, int numTurns)
        {
            graphicGroup.color = victor switch
            {
                UnitAllegiance.PLAYER => ColorUtils.VictoryColor,
                UnitAllegiance.ENEMY => ColorUtils.DefeatColor,
                _ => Color.white,
            };
            m_ResultText.text = victor switch
            {
                UnitAllegiance.PLAYER => "Victory!",
                UnitAllegiance.ENEMY => "Defeat!",
                _ => "???"
            };

            SetActive(true);
        }

        public void ReturnFromBattle()
        {
            SetActive(false);
            GameSceneManager.Instance.UnloadBattleScene();
        }

        private void SetActive(bool active)
        {
            if (active) m_UIFader.Show(TransitionDuration);
            else m_UIFader.Hide(TransitionDuration);
        }
    }
}
