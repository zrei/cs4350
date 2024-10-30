using TMPro;
using UnityEngine.UI;

namespace Game.UI
{
    public class NamedObjectButton : SelectableBase
    {
        public Image icon;
        public TextMeshProUGUI nameText;
        public Image glow;

        protected override void Awake()
        {
            base.Awake();
            if (glow == null)
                Logger.Log(this.GetType().Name, this.name, "Check glow: " + (glow == null), this.gameObject, LogLevel.ERROR);
            glow.CrossFadeAlpha(0, 0, true);
        }

        public void SetGlowActive(bool active)
        {
            glow.CrossFadeAlpha(active ? 1 : 0, 0.2f, false);
        }
        
        public void SetObjectName(string objectName)
        {
            nameText.text = objectName;
        }
    }
}
