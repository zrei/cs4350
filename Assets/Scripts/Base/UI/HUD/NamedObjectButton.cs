using TMPro;
using UnityEngine.UI;

namespace Game.UI
{
    public class NamedObjectButton : SelectableBase
    {
        public Image icon;
        public TextMeshProUGUI nameText;
        public Image glow;

        protected override void Start()
        {
            base.Start();
            glow?.CrossFadeAlpha(0, 0, true);
        }

        public void SetGlowActive(bool active)
        {
            glow?.CrossFadeAlpha(active ? 1 : 0, 0.2f, false);
        }
        
        public void SetObjectName(string objectName)
        {
            nameText.text = objectName;
        }
    }
}
