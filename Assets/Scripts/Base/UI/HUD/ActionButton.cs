using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class ActionButton : SelectableBase
    {
        public Image icon;
        public Image glow;

        protected override void Awake()
        {
            base.Awake();
            glow.CrossFadeAlpha(0, 0, true);
        }

        public void SetGlowActive(bool active)
        {
            glow.CrossFadeAlpha(active ? 1 : 0, 0.2f, false);
        }

        public void SetActive(bool active)
        {
            interactable = active;
            icon.color = active ? Color.red : Color.clear;
        }
    }
}
