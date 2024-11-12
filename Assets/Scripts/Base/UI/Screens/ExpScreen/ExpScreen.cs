using System.Collections.Generic;
using Game.Input;
using UnityEngine;

namespace Game.UI
{
    public class ExpScreen : BaseUIScreen
    {
        [SerializeField] ExpDisplay m_ExpDisplayPrefab;
        [SerializeField] SelectableBase m_ReturnButton;
        [SerializeField] RectTransform m_ExpDisplayParent;

        public override void Initialize()
        {
            base.Initialize();
            GlobalEvents.Level.ExpGainEvent += OnExpGain;
        }

        private void OnDestroy()
        {
            GlobalEvents.Level.ExpGainEvent -= OnExpGain;
        }

        private void OnExpGain(List<ExpGainSummary> expSummaries)
        {
            m_ReturnButton.interactable = false;
            ClearChildren();

            int numAnimations = expSummaries.Count;
            int numCompletedAnimations = 0;

            foreach (ExpGainSummary expGainSummary in expSummaries)
            {
                ExpDisplay expDisplay = Instantiate(m_ExpDisplayPrefab, m_ExpDisplayParent);
                expDisplay.SetDisplay(expGainSummary, CompleteAnimation);
            }
        
            void CompleteAnimation()
            {
                ++numCompletedAnimations;

                if (numCompletedAnimations >= numAnimations)
                {
                    m_ReturnButton.interactable = true;
                }
            }
            
            m_ReturnButton.onSubmit.AddListener(CloseExpScreen);
        }

        private void ClearChildren()
        {
            int childCount = m_ExpDisplayParent.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                Destroy(m_ExpDisplayParent.GetChild(i).gameObject);
            }
        }

        private void CloseExpScreen()
        {
            UIScreenManager.Instance.CloseScreen();
            GlobalEvents.Level.CompleteExpGainEvent?.Invoke();
            m_ReturnButton.onSubmit.RemoveListener(CloseExpScreen);
        }
        
        public override void ScreenUpdate()
        {
        }
        
        public override void OnCancel(IInput input)
        {
            CloseExpScreen();
        }
    }
}
