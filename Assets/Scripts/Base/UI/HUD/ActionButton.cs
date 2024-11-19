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
            
            if (Application.isPlaying)
            {
                Color color = icon.color;
                color.a = glow.color.a;
                glow.color = color;
                glow.CrossFadeAlpha(0, 0, true);
            }
        }

        public void SetGlowActive(bool active)
        {
            glow.CrossFadeAlpha(active ? 1 : 0, 0.2f, false);
        }
    }
}
