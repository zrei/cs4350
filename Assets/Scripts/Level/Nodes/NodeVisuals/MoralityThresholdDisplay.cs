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

        private void Awake()
        {
            m_Renderer = GetComponent<Renderer>();
            m_Renderer.enabled = false;
        }

        public void SetMoralityThresholdText(Threshold threshold)
        {
            int thresholdValue = MoralityManager.Instance.GetMoralityValue(threshold.m_Threshold);
            
            if (threshold.m_GreaterThan)
            {
                m_MoralityThresholdText.text = $">{thresholdValue}<sprite name=\"Morality\" tint>";
                m_MoralityThresholdText.color = GOOD_COLOR;
            }
            else
            {
                m_MoralityThresholdText.text = $"<{thresholdValue}<sprite name=\"Morality\" tint>";
                m_MoralityThresholdText.color = BAD_COLOR;
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