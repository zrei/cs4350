using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class UnitDisplay : MonoBehaviour
    {
        [SerializeField]
        private FormattedTextDisplay nameDisplay;

        [SerializeField]
        private FormattedTextDisplay classDisplay;

        [SerializeField]
        private FormattedTextDisplay levelDisplay;

        [SerializeField]
        private FormattedTextDisplay moveDisplay;

        [SerializeField]
        private FormattedTextDisplay hpDisplay;

        [SerializeField]
        private FormattedTextDisplay mpDisplay;

        [SerializeField]
        private FormattedTextDisplay phyAtkDisplay;

        [SerializeField]
        private FormattedTextDisplay mgcAtkDisplay;

        [SerializeField]
        private FormattedTextDisplay phyDefDisplay;

        [SerializeField]
        private FormattedTextDisplay mgcDefDisplay;

        [SerializeField]
        private FormattedTextDisplay spdDisplay;

        private Animator animator;
        private AnimatorCallbackExecuter animatorCallbackExecuter;
        private bool isHidden;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.enabled = false;
            animatorCallbackExecuter = animator.GetBehaviour<AnimatorCallbackExecuter>();
            Hide();

            GlobalEvents.Battle.PreviewCurrentUnitEvent += PreviewCurrentUnit;
        }
        
        private void OnDestroy()
        {
            GlobalEvents.Battle.PreviewCurrentUnitEvent -= PreviewCurrentUnit;
        }

        private void PreviewCurrentUnit(Unit currentUnit)
        {
            if (currentUnit == null)
            {
                if (!isHidden) Hide();
            }
            else if (currentUnit != null)
            {
                if (isHidden) Show();

                var totalStats = currentUnit.GetTotalStats();
                nameDisplay?.SetValue(totalStats.m_name); // placeholder
                classDisplay?.SetValue(totalStats.m_class); // placeholder
                levelDisplay?.SetValue("1"); // placeholder
                moveDisplay?.SetValue(totalStats.m_MovementRange);
                hpDisplay?.SetValue(currentUnit.CurrentHealth, totalStats.m_Health);
                mpDisplay?.SetValue(totalStats.m_Mana, totalStats.m_Mana); // placeholder
                phyAtkDisplay?.SetValue(totalStats.m_PhysicalAttack);
                mgcAtkDisplay?.SetValue(totalStats.m_MagicAttack);
                phyDefDisplay?.SetValue(totalStats.m_PhysicalDefence);
                mgcDefDisplay?.SetValue(totalStats.m_MagicDefence);
                spdDisplay?.SetValue(totalStats.m_Speed);
            }
        }

        private void Show()
        {
            isHidden = false;
            animator.enabled = true;
            animatorCallbackExecuter.RemoveAllListeners();
            animatorCallbackExecuter.AddListener(() => animator.enabled = false);
            animator.Play(UIConstants.ShowAnimHash);
        }

        private void Hide()
        {
            isHidden = true;
            animator.enabled = true;
            animatorCallbackExecuter.RemoveAllListeners();
            animatorCallbackExecuter.AddListener(() => animator.enabled = false);
            animator.Play(UIConstants.HideAnimHash);
        }
    }
}
