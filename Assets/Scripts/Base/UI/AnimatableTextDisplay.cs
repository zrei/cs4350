using System;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class AnimatableTextDisplay : MonoBehaviour
    {
        #region SFX
        [Tooltip("Leave empty for no sound")]
        public AudioDataSO m_AnimationAudio = null;
        #endregion
        public float DelayBetweenChars = 0.01f;

        private TextMeshProUGUI TextComponent;

        private Color32[][] initialColors;
        private int currentIndex;

        public bool IsAnimating => initialColors != null && currentIndex < TextComponent.textInfo.characterCount;

        private float timer;

        public event Action onTextComplete;

        private int? m_SFXToken = null;

        private void Awake()
        {
            TextComponent = GetComponent<TextMeshProUGUI>();
        }

        public void SetText(string text, float time = -1)
        {
            TextComponent.text = text;
            TextComponent.ForceMeshUpdate();

            // Remember initial colors
            initialColors = new Color32[TextComponent.textInfo.meshInfo.Length][];
            for (int i = 0; i < initialColors.Length; i++)
            {
                Color32[] targetColors = TextComponent.textInfo.meshInfo[i].colors32;
                initialColors[i] = new Color32[targetColors.Length];
                Array.Copy(targetColors, initialColors[i], targetColors.Length);
            }

            // Set all characters to alpha 0 color
            for (int i = 0; i < TextComponent.textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = TextComponent.textInfo.characterInfo[i];

                if (charInfo.isVisible)
                {
                    int materialIndex = charInfo.materialReferenceIndex;
                    int vertexIndex = charInfo.vertexIndex;
                    Color32[] targetColors = TextComponent.textInfo.meshInfo[materialIndex].colors32;
                    for (int j = 0; j < 4; j++)
                    {
                        targetColors[vertexIndex + j] = new Color32(0, 0, 0, 0);
                    }
                }
            }

            currentIndex = 0;
            TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            if (time >= 0)
            {
                if (TextComponent.textInfo.characterCount > 0)
                {
                    
                    DelayBetweenChars = time / TextComponent.textInfo.characterCount;
                }
            }

            if (m_AnimationAudio != null)
            {
                m_SFXToken = SoundManager.Instance.Play(m_AnimationAudio);
            }
        }

        private void ColorNextChar()
        {
            if (IsAnimating)
            {
                TMP_CharacterInfo charInfo = TextComponent.textInfo.characterInfo[currentIndex];

                if (charInfo.isVisible)
                {
                    int materialIndex = charInfo.materialReferenceIndex;
                    int vertexIndex = charInfo.vertexIndex;
                    Color32[] targetColors = TextComponent.textInfo.meshInfo[materialIndex].colors32;
                    for (int j = 0; j < 4; j++)
                    {
                        targetColors[vertexIndex + j] = initialColors[materialIndex][vertexIndex];
                    }
                }

                currentIndex++;

                TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                // play sound effect
            }
        }

        private void UpdateColors()
        {
            bool changed = false;

            for (int i = 0; i < TextComponent.textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = TextComponent.textInfo.characterInfo[i];

                if (charInfo.isVisible)
                {
                    int materialIndex = charInfo.materialReferenceIndex;
                    int vertexIndex = charInfo.vertexIndex;
                    Color32[] targetColors = TextComponent.textInfo.meshInfo[materialIndex].colors32;
                    Color32 newColor = i <= currentIndex ? initialColors[materialIndex][vertexIndex] : new Color32(0, 0, 0, 0);
                    for (int j = 0; j < 4; j++)
                    {
                        if (!newColor.Equals(targetColors[vertexIndex + j]))
                        {
                            targetColors[vertexIndex + j] = newColor;
                            changed = true;
                        }
                    }
                }
            }

            if (changed)
            {
                // play sound effect
            }

            TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            if (!IsAnimating)
            {
                onTextComplete?.Invoke();

                if (m_SFXToken.HasValue)
                {
                    SoundManager.Instance.Stop(m_SFXToken.Value);
                    m_SFXToken = null;
                }
            }
        }

        public void SkipToEnd()
        {
            currentIndex = TextComponent.textInfo.characterCount - 1;
            UpdateColors();
        }

        private void Update()
        {
            if (IsAnimating)
            {
                if (timer >= DelayBetweenChars)
                {
                    currentIndex++;
                    UpdateColors();

                    timer = 0f;
                }

                timer += Time.unscaledDeltaTime;
            }
        }
    }
}
