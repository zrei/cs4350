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

        public List<Tintable> graphics;

        private void UpdateColors()
        {
            graphics.ForEach(x => x.graphic.color = color * x.colorMult);
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
