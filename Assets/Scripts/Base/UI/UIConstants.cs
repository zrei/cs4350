using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public static class UIConstants
    {
        public static readonly int ShowAnimHash = Animator.StringToHash("Show");
        public static readonly int HideAnimHash = Animator.StringToHash("Hide");

        public static readonly Vector4 AnchorUL = new(0, 1, 0, 1);
        public static readonly Vector4 AnchorUC = new(0.5f, 1, 0.5f, 1);
        public static readonly Vector4 AnchorUR = new(1, 1, 1, 1);
        public static readonly Vector4 AnchorCL = new(0, 0.5f, 0, 0.5f);
        public static readonly Vector4 AnchorC = new(0.5f, 0.5f, 0.5f, 0.5f);
        public static readonly Vector4 AnchorCR = new(1, 0.5f, 1, 0.5f);
        public static readonly Vector4 AnchorDL = new(0, 0, 0, 0);
        public static readonly Vector4 AnchorDC = new(0.5f, 0, 0.5f, 0);
        public static readonly Vector4 AnchorDR = new(1, 0, 1, 0);
    }

    public static class ColorUtils
    {
        public static readonly Color VictoryColor = new Color(0.9f, 0.9f, 0.2f);
        public static readonly Color DefeatColor = new Color(0.9f, 0.2f, 0.2f);

        public static readonly Color AllyColor = new Color32(0, 201, 255, 255);
        public static readonly Color EnemyColor = new Color32(255, 0, 0, 255);
    }
}
