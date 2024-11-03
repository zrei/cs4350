using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public static class UIConstants
    {
        public static readonly int ShowAnimHash = Animator.StringToHash("Show");
        public static readonly int HideAnimHash = Animator.StringToHash("Hide");

        public static readonly Color VictoryColor = new Color(0.9f, 0.9f, 0.2f);
        public static readonly Color DefeatColor = new Color(0.9f, 0.2f, 0.2f);
    }
}
