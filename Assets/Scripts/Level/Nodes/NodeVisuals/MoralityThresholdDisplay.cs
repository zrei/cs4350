using TMPro;
using UnityEngine;

namespace Level.Nodes.NodeVisuals
{
    public class MoralityThresholdDisplay : MonoBehaviour
    {
        private Renderer m_Renderer;
        
        [SerializeField] private Color GOOD_COLOR = Color.blue;
        [SerializeField] private  Color BAD_COLOR = Color.red;
        
        [SerializeField]
        private TextMeshPro m_MoralityThresholdText;

        public void Initialise()
        {
            m_Renderer = GetComponent<Renderer>();
            m_Renderer.enabled = false;
        }

        public void SetMoralityThresholdText(MoralityCondition condition)
        {
            int thresholdValue = condition.threshold;

            switch (condition.mode)
            {
                case MoralityCondition.Mode.GreaterThan:
                    m_MoralityThresholdText.text = $">{thresholdValue}<sprite name=\"Morality\" tint>";
                    m_MoralityThresholdText.color = GOOD_COLOR;
                    break;
                case MoralityCondition.Mode.GreaterThanOrEqual:
                    m_MoralityThresholdText.text = $"≥{thresholdValue}<sprite name=\"Morality\" tint>";
                    m_MoralityThresholdText.color = GOOD_COLOR;
                    break;
                case MoralityCondition.Mode.LessThan:
                    m_MoralityThresholdText.text = $"<{thresholdValue}<sprite name=\"Morality\" tint>";
                    m_MoralityThresholdText.color = BAD_COLOR;
                    break;
                case MoralityCondition.Mode.LessThanOrEqual:
                    m_MoralityThresholdText.text = $"≤{thresholdValue}<sprite name=\"Morality\" tint>";
                    m_MoralityThresholdText.color = BAD_COLOR;
                    break;
                default:
                    Debug.LogError("Invalid morality condition mode");
                    break;
            }
        }

        public void Show()
        {
            m_Renderer.enabled = true;
        }
        
        public void Hide()
        {
            m_Renderer.enabled = false;
        }
    }
}