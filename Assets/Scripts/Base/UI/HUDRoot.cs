using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(UIAnimator))]
    public class HUDRoot : Singleton<HUDRoot>
    {
        private UIAnimator m_UIAnimator;

        protected override void HandleAwake()
        {
            m_UIAnimator = GetComponent<UIAnimator>();
        }

        public void Show()
        {
            m_UIAnimator.Show();
        }

        public void Hide()
        {
            m_UIAnimator.Hide();
        }
    }
}
