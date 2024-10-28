using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Game.UI
{
    [RequireComponent(typeof(RawImage))]
    public class BackgroundBlur : MonoBehaviour
    {
        private const float minStrength = 0.01f;

        [SerializeField]
        private float transitionTime = 0.2f;
        [SerializeField]
        private float maxStrength = 6.0f;

        private Material blurMat;
        private RawImage background;

        private Texture2D backgroundTex;
        private RenderTexture tempRT1;
        private RenderTexture tempRT2;

        private bool init;

        public bool IsActive => backgroundTex != null;

        public float Strength
        {
            get => strength;
            set
            {
                strength = Mathf.Clamp(value, minStrength, maxStrength);

                if (backgroundTex == null) return;

                var gridSize = Mathf.CeilToInt(strength * 6.0f);
                if (gridSize % 2 == 0)
                {
                    gridSize++;
                }
                blurMat.SetFloat("_Spread", strength);
                blurMat.SetInteger("_GridSize", gridSize);

                Graphics.Blit(backgroundTex, tempRT1, blurMat, 0);
                Graphics.Blit(tempRT1, tempRT2, blurMat, 1);
            }
        }
        [SerializeField]
        [SerializeProperty("Strength")]
        private float strength;

        private bool DebugCapture
        {
            get => debugCapture;
            set
            {
                debugCapture = value;
                CaptureBackground();
            }
        }
        [SerializeProperty("DebugCapture")]
        [Tooltip("Toggle this to capture the current screen.")]
        [SerializeField]
        private bool debugCapture;

        private void Init()
        {
            if (init) return;

            blurMat = new Material(Shader.Find("GaussianBlur"));
            background = GetComponent<RawImage>();
            
            init = true;
        }

        private void CaptureBackground()
        {
            Init();

            backgroundTex = RenderingUtils.Capture(new()
            {
                CameraManager.Instance.MainCamera,
                CameraManager.Instance.UICamera
            });

            if (tempRT1 != null)
            {
                RenderTexture.ReleaseTemporary(tempRT1);
            }
            if (tempRT2 != null)
            {
                RenderTexture.ReleaseTemporary(tempRT2);
            }
            tempRT1 = RenderTexture.GetTemporary(
                backgroundTex.width, 
                backgroundTex.height);
            tempRT2 = RenderTexture.GetTemporary(tempRT1.descriptor);

            background.texture = tempRT2;
            var color = background.color;
            color.a = 1;
            background.color = color;
        }

        public void ApplyBlur()
        {
            CaptureBackground();

            StopAllCoroutines();
            StartCoroutine(AnimateBlur());
        }

        private IEnumerator AnimateBlur()
        {
            var time = 0f;
            while (time < transitionTime)
            {
                time += Time.unscaledDeltaTime;
                Strength = Mathf.Lerp(minStrength, maxStrength, time / transitionTime);
                yield return null;
            }
        }

        public void RemoveBlur()
        {
            background.texture = null;
            var color = background.color;
            color.a = 0;
            background.color = color;

            backgroundTex = null;
            if (tempRT1 != null)
            {
                RenderTexture.ReleaseTemporary(tempRT1);
                tempRT1 = null;
            }
            if (tempRT2 != null)
            {
                RenderTexture.ReleaseTemporary(tempRT2);
                tempRT2 = null;
            }
        }
    }
}
