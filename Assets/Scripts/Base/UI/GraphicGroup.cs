using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class GraphicGroup : Graphic
    {
        [Serializable]
        public struct Tintable
        {
            public Graphic graphic;
            public Color colorMult;
        }

        public List<Tintable> graphics = new();

        public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha, bool useRGB)
        {
            graphics.ForEach(x => x.graphic.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha, useRGB));
        }

        public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
        {
            graphics.ForEach(x => x.graphic.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha));
        }

        public override void CrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale)
        {
            graphics.ForEach(x => x.graphic.CrossFadeAlpha(alpha, duration, ignoreTimeScale));
        }

        private void UpdateColors()
        {
            graphics.ForEach(x => { if (x.graphic != null) x.graphic.color = color * x.colorMult; });
        }

        public override void SetVerticesDirty()
        {
            UpdateColors();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh?.Clear();
        }
    }
}
