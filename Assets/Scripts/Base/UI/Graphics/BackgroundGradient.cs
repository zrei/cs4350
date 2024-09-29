using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class BackgroundGradient : MaskableGraphic
    {
        [SerializeField]
        [Range(1, 100)]
        private int divisions = 40;

        public float rotation;

        private Color32[] colors;

        public Gradient gradient = new();

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            var rect = GetPixelAdjustedRect();
            var dx = rect.width / divisions;
            var dy = rect.height / divisions;

            var totalVerts = (divisions + 1) * (divisions + 1);
            if (colors == null || colors.Length != totalVerts)
            {
                colors = new Color32[totalVerts];
            }

            var rotationVector = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation));
            var center = new Vector2(0.5f, 0.5f);

            vh.Clear();
            for (var y = 0; y <= divisions; y++)
            {
                for (var x = 0; x <= divisions; x++)
                {
                    var index = y * (divisions + 1) + x;

                    var pos = new Vector2(rect.x + (dx * x), rect.y + (dy * y));

                    var uv = new Vector2((float)x / divisions, (float)y / divisions);

                    var gradientPos = Vector2.Dot(uv - center, rotationVector) + 0.5f;
                    var col = gradient.Evaluate(gradientPos);

                    col.a *= color.a;
                    colors[index] = col;
                    vh.AddVert(pos, col, uv);

                    if (x == 0) continue;

                    var vD = index - (divisions + 1);
                    var dL = vD - 1;
                    var hL = index - 1;
                    if (vD >= 0 && dL >= 0)
                    {
                        vh.AddTriangle(index, vD, dL);
                    }
                    if (dL >= 0 && hL >= 0)
                    {
                        vh.AddTriangle(index, dL, hL);
                    }
                }
            }
        }

        private void UpdateColors()
        {
            if (colors.Length != workerMesh.vertexCount) UpdateGeometry();

            var rotationVector = new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation));
            var center = new Vector2(0.5f, 0.5f);
            for (var y = 0; y <= divisions; y++)
            {
                for (var x = 0; x <= divisions; x++)
                {
                    var index = y * (divisions + 1) + x;
                    var uv = new Vector2((float)x / divisions, (float)y / divisions);

                    var gradientPos = Vector2.Dot(uv - center, rotationVector) + 0.5f;
                    var col = gradient.Evaluate(gradientPos);

                    col.a *= color.a;
                    colors[index] = col;
                }
            }

            workerMesh.SetColors(colors);
            canvasRenderer.SetMesh(workerMesh);
        }
    }
}
