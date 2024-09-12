using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static class RenderingUtils
    {
        /// <summary>
        /// Renders the output of the given camera stack to a Texture2D.
        /// </summary>
        /// <param name="cameras">The camera stack to render from.</param>
        /// <returns>Texture2D containing the output of the camera stack.</returns>
        public static Texture2D Capture(List<Camera> cameras)
        {
            if (cameras == null || cameras.Count == 0) return null;

            var width = cameras[0].pixelWidth;
            var height = cameras[0].pixelHeight;

            RenderTexture resultRT = RenderTexture.GetTemporary(width, height, 24);
            for (int i = cameras.Count - 1; i >= 0; i--)
            {
                var camera = cameras[i];
                var original = camera.targetTexture;
                camera.targetTexture = resultRT;
                camera.Render();
                camera.targetTexture = original;
            }

            RenderTexture.active = resultRT;
            Texture2D result = new(width, height);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(resultRT);

            return result;
        }
    }
}
