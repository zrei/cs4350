using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Game.UI
{
    public class SaveDisplay : BaseUIScreen
    {
        [SerializeField] List<CanvasGroup> m_ChildCanvasGroups;

        private const float INTERVAL = 0.5f;

        public override void Show(object[] args)
        {            
            StartCoroutine(DisplayCoroutine());
            base.Show();
        }

        protected override void HideDone()
        {
            base.HideDone();
            StopAllCoroutines();
        }

        private void ResetDisplay()
        {
            foreach (CanvasGroup cg in m_ChildCanvasGroups)
            {
                cg.alpha = 0f;
            }
        }

        private IEnumerator DisplayCoroutine()
        {
            int index = 0;
            while (true)
            {
                yield return new WaitForSeconds(INTERVAL);
                if (index == 0)
                {
                    ResetDisplay();
                }
                m_ChildCanvasGroups[index].alpha = 1f;
                index = (index + 1) % m_ChildCanvasGroups.Count;
            }
        }

        public override void ScreenUpdate()
        {
            // pass
        }
    }
}
