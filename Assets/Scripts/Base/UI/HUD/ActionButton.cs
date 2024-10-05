using System.Collections;
using System.Collections.Generic;
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
    }
}
